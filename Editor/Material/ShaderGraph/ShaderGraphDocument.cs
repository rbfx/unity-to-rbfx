using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityToRebelFork.Editor.ShaderGraphs
{
    // The Shader Graph reader and evaluator are plain C# (no UnityEngine) so they can be tested outside the editor.

    /// <summary>A value flowing through a graph: up to four components (a scalar is splatted to all four) or a texture handle.</summary>
    public struct Val
    {
        public float X, Y, Z, W;

        /// <summary>The identity of the texture when the value is a Texture2D (the material property it comes from or an inline asset); null otherwise.</summary>
        public string Tex;

        public Val(float x, float y, float z, float w)
        {
            X = x; Y = y; Z = z; W = w; Tex = null;
        }

        public static Val Splat(float v) => new Val(v, v, v, v);

        public float this[int i]
        {
            get { return i == 0 ? X : i == 1 ? Y : i == 2 ? Z : W; }
            set
            {
                switch (i)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    default: W = value; break;
                }
            }
        }

        public static Val Map(Val a, Func<float, float> f) => new Val(f(a.X), f(a.Y), f(a.Z), f(a.W));
        public static Val Map(Val a, Val b, Func<float, float, float> f) => new Val(f(a.X, b.X), f(a.Y, b.Y), f(a.Z, b.Z), f(a.W, b.W));
        public static Val Map(Val a, Val b, Val c, Func<float, float, float, float> f) => new Val(f(a.X, b.X, c.X), f(a.Y, b.Y, c.Y), f(a.Z, b.Z, c.Z), f(a.W, b.W, c.W));

        public override string ToString() => "(" + X.ToString("0.###", CultureInfo.InvariantCulture) + ", " + Y.ToString("0.###", CultureInfo.InvariantCulture) + ", "
                                             + Z.ToString("0.###", CultureInfo.InvariantCulture) + ", " + W.ToString("0.###", CultureInfo.InvariantCulture) + ")" + (Tex != null ? " tex:" + Tex : "");
    }

    public enum SgPropertyKind
    {
        Float,
        Vector,
        Color,
        Boolean,
        Texture2D,
        Other,
    }

    public sealed class SgProperty
    {
        public string Id;
        public string Guid;
        public string Name;

        /// <summary>The name the material stores the property under.</summary>
        public string Reference;

        public SgPropertyKind Kind;
        public Val Default;

        /// <summary>A Color property in HDR mode: its value is linear, a plain colour is entered in gamma space.</summary>
        public bool Hdr;

        /// <summary>The GUID of the texture asset the graph assigns by default (Texture2D properties), if any.</summary>
        public string DefaultTextureGuid;

        /// <summary>0 white, 1 black, 2 grey, 3 bump: what the property samples as when no texture is assigned.</summary>
        public int DefaultType;
    }

    public sealed class SgSlot
    {
        public int Id;
        public bool IsInput;

        /// <summary>The slot's shader name (what the node types name their slots: "A", "In", "RGBA"); the display name when it has none.</summary>
        public string Name;

        public string DisplayName;
        public string Type;
        public bool HasValue;
        public Val Value;

        /// <summary>The GUID of an inline texture of a Texture2D input slot.</summary>
        public string TextureGuid;
    }

    public sealed class SgNode
    {
        public string Id;

        /// <summary>The type name without the namespace ("MultiplyNode").</summary>
        public string Type;

        public string Name;
        public JObject Raw;
        public List<SgSlot> Slots = new List<SgSlot>();

        public SgSlot Slot(int id)
        {
            foreach (var slot in Slots)
                if (slot.Id == id) return slot;
            return null;
        }

        public SgSlot Input(string name)
        {
            foreach (var slot in Slots)
                if (slot.IsInput && slot.Name == name) return slot;
            return null;
        }

        public SgSlot Output(string name)
        {
            foreach (var slot in Slots)
                if (!slot.IsInput && slot.Name == name) return slot;
            return null;
        }

        public SgSlot InputAt(int index)
        {
            foreach (var slot in Slots)
                if (slot.IsInput && index-- == 0) return slot;
            return null;
        }

        public override string ToString() => Type + " '" + Name + "'";
    }

    public struct SgEdge
    {
        public string FromNode;
        public int FromSlot;
        public string ToNode;
        public int ToSlot;
    }

    /// <summary>
    /// A Shader Graph (.shadergraph) or Sub Graph (.shadersubgraph) file read without Unity's own graph classes, so the reader does not depend on the Shader Graph package
    /// version (the "MultiJson" text format is the same since Unity 2020.2): the nodes, their slots with default values, the edges and the properties.
    /// </summary>
    public sealed class ShaderGraphDocument
    {
        public readonly Dictionary<string, SgNode> Nodes = new Dictionary<string, SgNode>();
        public readonly Dictionary<string, SgProperty> PropertiesById = new Dictionary<string, SgProperty>();
        public readonly Dictionary<string, SgProperty> PropertiesByGuid = new Dictionary<string, SgProperty>();
        public readonly List<SgProperty> Properties = new List<SgProperty>();
        public readonly List<SgEdge> Edges = new List<SgEdge>();
        public readonly Dictionary<string, JObject> Keywords = new Dictionary<string, JObject>();

        /// <summary>The master stack's blocks by descriptor name without the "SurfaceDescription." prefix ("BaseColor", "NormalTS").</summary>
        public readonly Dictionary<string, SgNode> Blocks = new Dictionary<string, SgNode>();

        /// <summary>The output node of a Sub Graph.</summary>
        public SgNode OutputNode;

        public bool IsSubGraph;

        /// <summary>The sub targets of the graph ("UniversalLitSubTarget", "HDUnlitSubTarget"...).</summary>
        public readonly List<string> SubTargets = new List<string>();

        public bool IsUnlit
        {
            get
            {
                var unlit = false;
                foreach (var target in SubTargets)
                {
                    if (target.IndexOf("Lit", StringComparison.Ordinal) >= 0 && target.IndexOf("Unlit", StringComparison.Ordinal) < 0) return false;
                    if (target.IndexOf("Unlit", StringComparison.Ordinal) >= 0) unlit = true;
                }
                return unlit;
            }
        }

        private readonly Dictionary<long, SgEdge> inputEdges = new Dictionary<long, SgEdge>();
        private readonly Dictionary<string, int> nodeIndex = new Dictionary<string, int>();

        /// <summary>The edge feeding an input slot, if any.</summary>
        public bool TryGetEdge(string node, int slot, out SgEdge edge)
        {
            return inputEdges.TryGetValue(Key(node, slot), out edge);
        }

        private long Key(string node, int slot)
        {
            if (!nodeIndex.TryGetValue(node, out var index))
            {
                index = nodeIndex.Count;
                nodeIndex[node] = index;
            }
            return ((long)index << 32) | (uint)slot;
        }

        /// <summary>Whether <paramref name="text"/> looks like the format this reader supports.</summary>
        public static bool IsSupported(string text)
        {
            return text != null && (text.Contains("\"m_Type\": \"UnityEditor.ShaderGraph.GraphData\"") || text.Contains("\"m_Type\":\"UnityEditor.ShaderGraph.GraphData\""));
        }

        public static ShaderGraphDocument Parse(string text)
        {
            if (!IsSupported(text))
                throw new FormatException("Not a Shader Graph file of the MultiJson format (Unity 2020.2+).");

            var objects = new Dictionary<string, JObject>();
            JObject graph = null;
            using (var stringReader = new System.IO.StringReader(text))
            using (var reader = new JsonTextReader(stringReader) { SupportMultipleContent = true, DateParseHandling = DateParseHandling.None })
            {
                while (reader.Read())
                {
                    if (reader.TokenType != JsonToken.StartObject) continue;
                    var obj = JObject.Load(reader);
                    var id = (string)obj["m_ObjectId"];
                    if (id != null) objects[id] = obj;
                    if (graph == null && (string)obj["m_Type"] == "UnityEditor.ShaderGraph.GraphData") graph = obj;
                }
            }
            if (graph == null) throw new FormatException("The graph has no GraphData object.");

            var doc = new ShaderGraphDocument();
            doc.IsSubGraph = graph["m_OutputNode"] is JObject outputRef && !string.IsNullOrEmpty((string)outputRef["m_Id"]);

            foreach (var reference in References(graph["m_Properties"]))
                if (objects.TryGetValue(reference, out var obj)) doc.AddProperty(obj);
            foreach (var reference in References(graph["m_Keywords"]))
                if (objects.TryGetValue(reference, out var obj)) doc.Keywords[reference] = obj;

            foreach (var reference in References(graph["m_Nodes"]))
            {
                if (!objects.TryGetValue(reference, out var obj)) continue;
                var node = new SgNode { Id = reference, Type = ShortName((string)obj["m_Type"]), Name = (string)obj["m_Name"], Raw = obj };
                foreach (var slotReference in References(obj["m_Slots"]))
                    if (objects.TryGetValue(slotReference, out var slotObject))
                        node.Slots.Add(ReadSlot(slotObject));
                doc.Nodes[reference] = node;

                if (node.Type == "BlockNode")
                {
                    var descriptor = (string)obj["m_SerializedDescriptor"] ?? node.Name;
                    var dot = descriptor.LastIndexOf('.');
                    doc.Blocks[dot >= 0 ? descriptor.Substring(dot + 1) : descriptor] = node;
                }
            }

            if (graph["m_Edges"] is JArray edges)
            {
                foreach (var e in edges)
                {
                    var edge = new SgEdge
                    {
                        FromNode = (string)e["m_OutputSlot"]?["m_Node"]?["m_Id"],
                        FromSlot = (int?)e["m_OutputSlot"]?["m_SlotId"] ?? 0,
                        ToNode = (string)e["m_InputSlot"]?["m_Node"]?["m_Id"],
                        ToSlot = (int?)e["m_InputSlot"]?["m_SlotId"] ?? 0,
                    };
                    if (edge.FromNode == null || edge.ToNode == null) continue;
                    doc.Edges.Add(edge);
                    doc.inputEdges[doc.Key(edge.ToNode, edge.ToSlot)] = edge;
                }
            }

            if (doc.IsSubGraph)
                doc.Nodes.TryGetValue((string)graph["m_OutputNode"]["m_Id"], out doc.OutputNode);

            // The sub targets are objects of their own, referenced from the targets.
            foreach (var target in objects.Values)
            {
                var type = (string)target["m_Type"];
                if (type != null && type.EndsWith("SubTarget", StringComparison.Ordinal)) doc.SubTargets.Add(ShortName(type));
            }
            return doc;
        }

        private void AddProperty(JObject obj)
        {
            var type = ShortName((string)obj["m_Type"]);
            var property = new SgProperty
            {
                Id = (string)obj["m_ObjectId"],
                Guid = (string)obj["m_Guid"]?["m_GuidSerialized"],
                Name = (string)obj["m_Name"],
                Kind = SgPropertyKind.Other,
            };
            var overrideName = (string)obj["m_OverrideReferenceName"];
            property.Reference = !string.IsNullOrEmpty(overrideName) ? overrideName : (string)obj["m_DefaultReferenceName"];
            if (string.IsNullOrEmpty(property.Reference)) property.Reference = "_" + (property.Name ?? property.Id).Replace(' ', '_');
            // A property marked non-exposed has no material entry: the graph's value always applies.
            var value = obj["m_Value"];

            switch (type)
            {
                case "Vector1ShaderProperty":
                    property.Kind = SgPropertyKind.Float;
                    property.Default = Val.Splat(value != null && value.Type != JTokenType.Object ? (float)value : 0f);
                    break;
                case "Vector2ShaderProperty":
                case "Vector3ShaderProperty":
                case "Vector4ShaderProperty":
                    property.Kind = SgPropertyKind.Vector;
                    property.Default = ReadVector(value);
                    break;
                case "ColorShaderProperty":
                    property.Kind = SgPropertyKind.Color;
                    property.Default = ReadVector(value);
                    property.Hdr = ((int?)obj["m_ColorMode"] ?? 0) == 1;
                    break;
                case "BooleanShaderProperty":
                    property.Kind = SgPropertyKind.Boolean;
                    property.Default = Val.Splat(value != null && value.Type == JTokenType.Boolean && (bool)value ? 1f : 0f);
                    break;
                case "Texture2DShaderProperty":
                    property.Kind = SgPropertyKind.Texture2D;
                    property.DefaultTextureGuid = TextureGuid((string)value?["m_SerializedTexture"]);
                    property.DefaultType = (int?)obj["m_DefaultType"] ?? 0;
                    break;
            }
            Properties.Add(property);
            PropertiesById[property.Id] = property;
            if (property.Guid != null) PropertiesByGuid[property.Guid] = property;
        }

        private static SgSlot ReadSlot(JObject obj)
        {
            var slot = new SgSlot
            {
                Id = (int?)obj["m_Id"] ?? 0,
                IsInput = ((int?)obj["m_SlotType"] ?? 0) == 0,
                DisplayName = (string)obj["m_DisplayName"],
                Type = ShortName((string)obj["m_Type"]),
            };
            slot.Name = (string)obj["m_ShaderOutputName"];
            if (string.IsNullOrEmpty(slot.Name)) slot.Name = slot.DisplayName;
            if (slot.Name != null) slot.Name = slot.Name.Replace(" ", "");

            var value = obj["m_Value"];
            if (value is JObject o)
            {
                slot.HasValue = true;
                slot.Value = ReadVector(o);
                if (o["m_SerializedTexture"] != null)
                {
                    slot.HasValue = false;
                    slot.TextureGuid = TextureGuid((string)o["m_SerializedTexture"]);
                }
            }
            else if (value != null && (value.Type == JTokenType.Float || value.Type == JTokenType.Integer))
            {
                slot.HasValue = true;
                slot.Value = Val.Splat((float)value);
            }
            else if (value != null && value.Type == JTokenType.Boolean)
            {
                slot.HasValue = true;
                slot.Value = Val.Splat((bool)value ? 1f : 0f);
            }
            if (obj["m_Texture"] is JObject texture)
                slot.TextureGuid = TextureGuid((string)texture["m_SerializedTexture"]);
            return slot;
        }

        /// <summary>A vector, a colour or a matrix (the first row is the value of a "dynamic" slot) as written by Shader Graph.</summary>
        private static Val ReadVector(JToken token)
        {
            if (!(token is JObject o)) return default;
            if (o["e00"] != null)
                return new Val((float?)o["e00"] ?? 0f, (float?)o["e01"] ?? 0f, (float?)o["e02"] ?? 0f, (float?)o["e03"] ?? 0f);
            if (o["r"] != null)
                return new Val((float?)o["r"] ?? 0f, (float?)o["g"] ?? 0f, (float?)o["b"] ?? 0f, (float?)o["a"] ?? 0f);
            return new Val((float?)o["x"] ?? 0f, (float?)o["y"] ?? 0f, (float?)o["z"] ?? 0f, (float?)o["w"] ?? 0f);
        }

        /// <summary>The asset GUID in a serialized texture reference: {"texture":{"fileID":2800000,"guid":"...","type":3}}.</summary>
        private static string TextureGuid(string serialized)
        {
            if (string.IsNullOrEmpty(serialized)) return null;
            try
            {
                var guid = (string)JObject.Parse(serialized)["texture"]?["guid"];
                return string.IsNullOrEmpty(guid) ? null : guid;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static IEnumerable<string> References(JToken array)
        {
            if (!(array is JArray list)) yield break;
            foreach (var item in list)
            {
                var id = (string)item["m_Id"];
                if (!string.IsNullOrEmpty(id)) yield return id;
            }
        }

        private static string ShortName(string type)
        {
            if (type == null) return "";
            var dot = type.LastIndexOf('.');
            return dot >= 0 ? type.Substring(dot + 1) : type;
        }
    }
}
