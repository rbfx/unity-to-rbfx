using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UnityToRebelFork.Editor.ShaderGraphs
{
    public enum SgSampleKind
    {
        Default,
        Normal,
    }

    /// <summary>What an evaluation reads from the world: the material's property values and the texels a texture sample returns.</summary>
    public interface IShaderGraphInputs
    {
        /// <summary>The material's value of a Float, Vector, Color or Boolean property (colours in linear space); false when the material does not have it and the graph's default applies.</summary>
        bool TryGetValue(SgProperty property, out Val value);

        /// <summary>The identity of the texture the material assigns to a Texture2D property, or null when it has none.</summary>
        string GetTexture(SgProperty property);

        bool IsKeywordEnabled(string reference);

        /// <summary>The raw texel (0..1, as stored) of a texture at a coordinate. <paramref name="texture"/> is an identity from <see cref="GetTexture"/>, "inline:&lt;guid&gt;" or "#none&lt;default type&gt;".</summary>
        Val Sample(string texture, SgSampleKind kind, Val uv);
    }

    /// <summary>One texture sample met while evaluating.</summary>
    public sealed class SgSampleRecord
    {
        public string NodeId;
        public string Texture;
        public SgSampleKind Kind;

        /// <summary>The coordinate the sample was taken at.</summary>
        public Val Uv;
    }

    /// <summary>
    /// A small CPU interpreter of a Shader Graph: gives the value of a block of the master stack (BaseColor, NormalTS...) for the given material values and texture samples.
    /// The node set is what artists use for materials (maths, blends, texture samples, UV, branches, Sub Graphs); nodes it does not know give an average of their inputs
    /// and a warning, nodes that depend on the world (time, position, noise) give fixed plausible values.
    /// </summary>
    public sealed class ShaderGraphEvaluator
    {
        private sealed class Frame
        {
            public ShaderGraphDocument Doc;
            public Dictionary<string, Val> Args;
            public readonly Dictionary<string, Val> Cache = new Dictionary<string, Val>();
            public readonly HashSet<string> Active = new HashSet<string>();
        }

        private readonly ShaderGraphDocument doc;
        private readonly IShaderGraphInputs inputs;
        private readonly Func<string, ShaderGraphDocument> subGraphs;
        private readonly Frame root;
        private int depth;

        /// <summary>The coordinate UV nodes (and unconnected UV inputs) give.</summary>
        public Val Uv;

        public readonly List<SgSampleRecord> Samples = new List<SgSampleRecord>();
        public readonly HashSet<string> Warnings;

        public ShaderGraphEvaluator(ShaderGraphDocument doc, IShaderGraphInputs inputs, Func<string, ShaderGraphDocument> subGraphs, HashSet<string> warnings = null)
        {
            this.doc = doc;
            this.inputs = inputs;
            this.subGraphs = subGraphs;
            Warnings = warnings ?? new HashSet<string>();
            root = new Frame { Doc = doc };
        }

        /// <summary>The value of a block of the master stack; false when the graph has no such block or nothing is connected to it (the value is then the block's default).</summary>
        public bool EvaluateBlock(string name, out Val value)
        {
            value = default;
            if (!doc.Blocks.TryGetValue(name, out var block)) return false;
            var slot = block.InputAt(0);
            if (slot == null) return false;
            return TryInput(root, block, slot, out value);
        }

        /// <summary>The value feeding an input slot (the slot's own value when nothing is connected).</summary>
        public Val EvaluateInput(SgNode node, SgSlot slot)
        {
            TryInput(root, node, slot, out var value);
            return value;
        }

        private bool TryInput(Frame frame, SgNode node, SgSlot slot, out Val value)
        {
            if (frame.Doc.TryGetEdge(node.Id, slot.Id, out var edge))
            {
                value = Out(frame, edge.FromNode, edge.FromSlot);
                return true;
            }
            value = Default(slot);
            return false;
        }

        private Val Default(SgSlot slot)
        {
            if (slot.Type == "UVMaterialSlot") return Uv;
            if (slot.Type == "Texture2DInputMaterialSlot" || slot.Type == "Texture2DMaterialSlot")
                return new Val { Tex = slot.TextureGuid != null ? "inline:" + slot.TextureGuid : "#none0" };
            return slot.Value;
        }

        private Val In(Frame frame, SgNode node, string name)
        {
            var slot = node.Input(name);
            if (slot == null) return default;
            TryInput(frame, node, slot, out var value);
            return value;
        }

        private Val In(Frame frame, SgNode node, int index)
        {
            var slot = node.InputAt(index);
            if (slot == null) return default;
            TryInput(frame, node, slot, out var value);
            return value;
        }

        private Val Out(Frame frame, string nodeId, int slotId)
        {
            var key = nodeId + ":" + slotId;
            if (frame.Cache.TryGetValue(key, out var cached)) return cached;
            if (!frame.Doc.Nodes.TryGetValue(nodeId, out var node)) return default;
            if (!frame.Active.Add(key))
            {
                Warnings.Add("cycle at " + node);
                return default;
            }
            Val value;
            try
            {
                value = Compute(frame, node, node.Slot(slotId));
            }
            finally
            {
                frame.Active.Remove(key);
            }
            frame.Cache[key] = value;
            return value;
        }

        private static float Saturate(float x) => x < 0f ? 0f : x > 1f ? 1f : x;
        private static float Fix(float x) => float.IsNaN(x) || float.IsInfinity(x) ? 0f : x;

        private Val Compute(Frame frame, SgNode node, SgSlot output)
        {
            var outName = output?.Name;
            switch (node.Type)
            {
                case "PropertyNode":
                    return PropertyValue(frame, node);
                case "Vector1Node":
                    return Val.Splat(In(frame, node, "X").X);
                case "Vector2Node":
                    return new Val(In(frame, node, "X").X, In(frame, node, "Y").X, 0, 0);
                case "Vector3Node":
                    return new Val(In(frame, node, "X").X, In(frame, node, "Y").X, In(frame, node, "Z").X, 0);
                case "Vector4Node":
                    return new Val(In(frame, node, "X").X, In(frame, node, "Y").X, In(frame, node, "Z").X, In(frame, node, "W").X);
                case "IntegerNode":
                case "SliderNode":
                    return Val.Splat((float?)node.Raw["m_Value"] ?? (float?)node.Raw["m_Value"]?["x"] ?? 0f);
                case "BooleanNode":
                    return Val.Splat((bool?)node.Raw["m_Value"] == true ? 1f : 0f);
                case "ColorNode":
                {
                    var c = node.Raw["m_Color"]?["color"];
                    var color = new Val((float?)c?["r"] ?? 0f, (float?)c?["g"] ?? 0f, (float?)c?["b"] ?? 0f, (float?)c?["a"] ?? 1f);
                    // Like a Color property, a non-HDR colour node is entered in gamma space.
                    if (((int?)node.Raw["m_Color"]?["mode"] ?? 0) == 0)
                    {
                        color.X = GammaToLinear(color.X);
                        color.Y = GammaToLinear(color.Y);
                        color.Z = GammaToLinear(color.Z);
                    }
                    return color;
                }
                case "ConstantNode":
                    switch ((int?)node.Raw["m_ConstantType"] ?? 0)
                    {
                        case 0: return Val.Splat(3.14159265f);
                        case 1: return Val.Splat(6.28318531f);
                        case 2: return Val.Splat(1.61803399f);
                        case 3: return Val.Splat(2.71828183f);
                        default: return Val.Splat(1.41421356f);
                    }
                case "Texture2DAssetNode":
                {
                    var guid = ExtractGuid((string)node.Raw["m_Texture"]?["m_SerializedTexture"]);
                    return new Val { Tex = guid != null ? "inline:" + guid : "#none0" };
                }
                case "UVNode":
                    return Uv;
                case "TilingAndOffsetNode":
                {
                    var uv = In(frame, node, "UV");
                    var tiling = In(frame, node, "Tiling");
                    var offset = In(frame, node, "Offset");
                    return new Val(uv.X * tiling.X + offset.X, uv.Y * tiling.Y + offset.Y, 0, 0);
                }
                case "SampleTexture2DNode":
                    return SampleTexture(frame, node, outName);

                // arithmetic
                case "AddNode": return Val.Map(In(frame, node, "A"), In(frame, node, "B"), (a, b) => a + b);
                case "SubtractNode": return Val.Map(In(frame, node, "A"), In(frame, node, "B"), (a, b) => a - b);
                case "MultiplyNode": return Val.Map(In(frame, node, "A"), In(frame, node, "B"), (a, b) => a * b);
                case "DivideNode": return Val.Map(In(frame, node, "A"), In(frame, node, "B"), (a, b) => b == 0f ? 0f : a / b);
                case "PowerNode": return Val.Map(In(frame, node, "A"), In(frame, node, "B"), (a, b) => Fix((float)Math.Pow(a, b)));
                case "ModuloNode": return Val.Map(In(frame, node, "A"), In(frame, node, "B"), (a, b) => b == 0f ? 0f : a - b * (float)Math.Floor(a / b));
                case "MinimumNode": return Val.Map(In(frame, node, "A"), In(frame, node, "B"), Math.Min);
                case "MaximumNode": return Val.Map(In(frame, node, "A"), In(frame, node, "B"), Math.Max);
                case "StepNode": return Val.Map(In(frame, node, "Edge"), In(frame, node, "In"), (e, x) => x >= e ? 1f : 0f);
                case "LerpNode": return Val.Map(In(frame, node, "A"), In(frame, node, "B"), In(frame, node, "T"), (a, b, t) => a + (b - a) * t);
                case "OneMinusNode": return Val.Map(In(frame, node, "In"), x => 1f - x);
                case "NegateNode": return Val.Map(In(frame, node, "In"), x => -x);
                case "AbsoluteNode": return Val.Map(In(frame, node, "In"), Math.Abs);
                case "SaturateNode": return Val.Map(In(frame, node, "In"), Saturate);
                case "FractionNode": return Val.Map(In(frame, node, "In"), x => x - (float)Math.Floor(x));
                case "FloorNode": return Val.Map(In(frame, node, "In"), x => (float)Math.Floor(x));
                case "CeilingNode": return Val.Map(In(frame, node, "In"), x => (float)Math.Ceiling(x));
                case "RoundNode": return Val.Map(In(frame, node, "In"), x => (float)Math.Round(x, MidpointRounding.AwayFromZero));
                case "SignNode": return Val.Map(In(frame, node, "In"), x => Math.Sign(x));
                case "SineNode": return Val.Map(In(frame, node, "In"), x => (float)Math.Sin(x));
                case "CosineNode": return Val.Map(In(frame, node, "In"), x => (float)Math.Cos(x));
                case "SquareRootNode": return Val.Map(In(frame, node, "In"), x => x <= 0f ? 0f : (float)Math.Sqrt(x));
                case "ReciprocalNode": return Val.Map(In(frame, node, "In"), x => x == 0f ? 0f : 1f / x);
                case "ExponentialNode": return Val.Map(In(frame, node, "In"), x => Fix((float)Math.Exp(x)));
                case "ClampNode": return Val.Map(In(frame, node, "In"), In(frame, node, "Min"), In(frame, node, "Max"), (x, lo, hi) => Math.Min(Math.Max(x, lo), hi));
                case "SmoothstepNode":
                    return Val.Map(In(frame, node, "Edge1"), In(frame, node, "Edge2"), In(frame, node, "In"), (a, b, x) =>
                    {
                        if (a == b) return x < a ? 0f : 1f;
                        var t = Saturate((x - a) / (b - a));
                        return t * t * (3f - 2f * t);
                    });
                case "RemapNode":
                {
                    var x = In(frame, node, "In");
                    var inMinMax = In(frame, node, "InMinMax");
                    var outMinMax = In(frame, node, "OutMinMax");
                    return Val.Map(x, v => inMinMax.Y == inMinMax.X ? outMinMax.X : outMinMax.X + (v - inMinMax.X) * (outMinMax.Y - outMinMax.X) / (inMinMax.Y - inMinMax.X));
                }
                case "DotProductNode":
                {
                    var a = In(frame, node, "A");
                    var b = In(frame, node, "B");
                    return Val.Splat(a.X * b.X + a.Y * b.Y + a.Z * b.Z);
                }
                case "NormalizeNode":
                {
                    var a = In(frame, node, "In");
                    var length = (float)Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
                    return length == 0f ? a : new Val(a.X / length, a.Y / length, a.Z / length, 0f);
                }

                // colour
                case "ContrastNode":
                {
                    var midpoint = (float)Math.Pow(0.5, 2.2);
                    return Val.Map(In(frame, node, "In"), In(frame, node, "Contrast"), (x, c) => (x - midpoint) * c + midpoint);
                }
                case "SaturationNode":
                {
                    var x = In(frame, node, "In");
                    var saturation = In(frame, node, "Saturation").X;
                    var luma = x.X * 0.2126729f + x.Y * 0.7151522f + x.Z * 0.0721750f;
                    return new Val(luma + saturation * (x.X - luma), luma + saturation * (x.Y - luma), luma + saturation * (x.Z - luma), x.W);
                }
                case "InvertColorsNode":
                    return Val.Map(In(frame, node, "In"), x => 1f - x);
                case "ColorspaceConversionNode":
                    return In(frame, node, "In");

                // channels
                case "SplitNode":
                {
                    var x = In(frame, node, "In");
                    return Val.Splat(outName == "R" ? x.X : outName == "G" ? x.Y : outName == "B" ? x.Z : x.W);
                }
                case "CombineNode":
                {
                    var r = In(frame, node, "R").X;
                    var g = In(frame, node, "G").X;
                    var b = In(frame, node, "B").X;
                    var a = In(frame, node, "A").X;
                    return outName == "RGBA" ? new Val(r, g, b, a) : outName == "RGB" ? new Val(r, g, b, 0f) : new Val(r, g, 0f, 0f);
                }
                case "SwizzleNode":
                {
                    var x = In(frame, node, "In");
                    return new Val(x[(int?)node.Raw["m_RedChannel"] ?? 0], x[(int?)node.Raw["m_GreenChannel"] ?? 1], x[(int?)node.Raw["m_BlueChannel"] ?? 2], x[(int?)node.Raw["m_AlphaChannel"] ?? 3]);
                }
                case "ChannelMaskNode":
                {
                    // The selected channels are kept, the others zeroed.
                    var x = In(frame, node, "In");
                    var mask = (int?)node.Raw["m_ChannelMask"] ?? -1;
                    if (mask == -1) return x;
                    return new Val((mask & 1) != 0 ? x.X : 0f, (mask & 2) != 0 ? x.Y : 0f, (mask & 4) != 0 ? x.Z : 0f, (mask & 8) != 0 ? x.W : 0f);
                }

                // logic
                case "BranchNode":
                    return In(frame, node, "Predicate").X != 0f ? In(frame, node, "True") : In(frame, node, "False");
                case "ComparisonNode":
                {
                    var a = In(frame, node, "A").X;
                    var b = In(frame, node, "B").X;
                    bool result;
                    switch ((int?)node.Raw["m_ComparisonType"] ?? 0)
                    {
                        case 0: result = a == b; break;
                        case 1: result = a != b; break;
                        case 2: result = a < b; break;
                        case 3: result = a <= b; break;
                        case 4: result = a > b; break;
                        default: result = a >= b; break;
                    }
                    return Val.Splat(result ? 1f : 0f);
                }
                case "AndNode": return Val.Splat(In(frame, node, "A").X != 0f && In(frame, node, "B").X != 0f ? 1f : 0f);
                case "OrNode": return Val.Splat(In(frame, node, "A").X != 0f || In(frame, node, "B").X != 0f ? 1f : 0f);
                case "NotNode": return Val.Splat(In(frame, node, "In").X != 0f ? 0f : 1f);
                case "KeywordNode":
                    return KeywordValue(frame, node);

                // normals
                case "NormalStrengthNode":
                {
                    var n = In(frame, node, "In");
                    var strength = In(frame, node, "Strength").X;
                    return new Val(n.X * strength, n.Y * strength, 1f + (n.Z - 1f) * Saturate(strength), 0f);
                }
                case "NormalBlendNode":
                {
                    var a = In(frame, node, "A");
                    var b = In(frame, node, "B");
                    return Normalize3(new Val(a.X + b.X, a.Y + b.Y, a.Z * b.Z, 0f));
                }
                case "NormalUnpackNode":
                    return In(frame, node, "In");

                // the world: fixed values
                case "TimeNode":
                    return outName == "CosineTime" ? Val.Splat(1f) : outName == "DeltaTime" || outName == "SmoothDelta" ? Val.Splat(1f / 60f) : Val.Splat(0f);
                case "PositionNode":
                case "ScreenPositionNode":
                case "ViewDirectionNode":
                case "TangentVectorNode":
                case "BitangentVectorNode":
                    return new Val(0.5f, 0.5f, 0f, 1f);
                case "NormalVectorNode":
                    return new Val(0f, 0f, 1f, 0f);
                case "VertexColorNode":
                    return new Val(1f, 1f, 1f, 1f);
                case "SceneColorNode":
                case "SceneDepthNode":
                    return Val.Splat(0.5f);
                case "TransformNode":
                case "PreviewNode":
                case "RedirectNodeData":
                    return In(frame, node, 0);

                case "SubGraphNode":
                    return EvaluateSubGraph(frame, node, output);
                case "SubGraphOutputNode":
                    return In(frame, node, 0);
            }

            if (node.Type.IndexOf("Noise", StringComparison.Ordinal) >= 0 || node.Type.IndexOf("Voronoi", StringComparison.Ordinal) >= 0)
                return Val.Splat(0.5f);
            return Unknown(frame, node);
        }

        private static Val Normalize3(Val a)
        {
            var length = (float)Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
            return length == 0f ? a : new Val(a.X / length, a.Y / length, a.Z / length, 0f);
        }

        /// <summary>A node the interpreter does not know: the average of what feeds it, so the influence of its inputs still shows.</summary>
        private Val Unknown(Frame frame, SgNode node)
        {
            Warnings.Add("unsupported node " + node.Type);
            var sum = default(Val);
            var count = 0;
            foreach (var slot in node.Slots)
            {
                if (!slot.IsInput || !frame.Doc.TryGetEdge(node.Id, slot.Id, out _)) continue;
                var value = In(frame, node, slot.Name);
                if (value.Tex != null) continue;
                sum = Val.Map(sum, value, (a, b) => a + b);
                count++;
            }
            return count == 0 ? default : Val.Map(sum, x => x / count);
        }

        private Val PropertyValue(Frame frame, SgNode node)
        {
            var propertyId = (string)node.Raw["m_Property"]?["m_Id"];
            if (propertyId == null || !frame.Doc.PropertiesById.TryGetValue(propertyId, out var property)) return default;

            if (frame.Args != null)
            {
                // Inside a Sub Graph the property is an argument of the node that called it.
                return frame.Args.TryGetValue(property.Id, out var arg) ? arg : Default(property);
            }
            if (property.Kind == SgPropertyKind.Texture2D)
            {
                var texture = inputs.GetTexture(property);
                return new Val { Tex = texture ?? (property.DefaultTextureGuid != null ? "inline:" + property.DefaultTextureGuid : "#none" + property.DefaultType) };
            }
            return inputs.TryGetValue(property, out var value) ? value : Default(property);
        }

        /// <summary>The graph's own value of a property (a non-HDR colour is entered in gamma space).</summary>
        private static Val Default(SgProperty property)
        {
            var value = property.Default;
            if (property.Kind == SgPropertyKind.Color && !property.Hdr)
            {
                value.X = GammaToLinear(value.X);
                value.Y = GammaToLinear(value.Y);
                value.Z = GammaToLinear(value.Z);
            }
            if (property.Kind == SgPropertyKind.Texture2D)
                value.Tex = property.DefaultTextureGuid != null ? "inline:" + property.DefaultTextureGuid : "#none" + property.DefaultType;
            return value;
        }

        private Val KeywordValue(Frame frame, SgNode node)
        {
            var keywordId = (string)node.Raw["m_Keyword"]?["m_Id"];
            if (keywordId == null || !frame.Doc.Keywords.TryGetValue(keywordId, out var keyword)) return In(frame, node, 0);

            var overrideName = (string)keyword["m_OverrideReferenceName"];
            var reference = !string.IsNullOrEmpty(overrideName) ? overrideName : (string)keyword["m_DefaultReferenceName"];
            var value = (int?)keyword["m_Value"] ?? 0;
            var enabled = value != 0;
            if (frame.Args == null && reference != null)
                enabled = inputs.IsKeywordEnabled(reference);

            if (((int?)keyword["m_KeywordType"] ?? 0) == 0)
                return In(frame, node, enabled ? "On" : "Off");

            // An enum keyword: pick the entry the keyword's value names.
            var entries = keyword["m_Entries"] as JArray;
            var index = 0;
            if (entries != null && frame.Args == null)
            {
                for (var i = 0; i < entries.Count; i++)
                    if (inputs.IsKeywordEnabled((reference ?? "") + "_" + (string)entries[i]["referenceName"])) index = i;
            }
            return In(frame, node, Math.Min(index, Math.Max(0, node.Slots.FindAll(s => s.IsInput).Count - 1)));
        }

        private Val SampleTexture(Frame frame, SgNode node, string outName)
        {
            var texture = In(frame, node, "Texture");
            var uv = In(frame, node, "UV");
            var kind = ((int?)node.Raw["m_TextureType"] ?? 0) == 1 ? SgSampleKind.Normal : SgSampleKind.Default;
            var id = texture.Tex ?? "#none0";

            Samples.Add(new SgSampleRecord { NodeId = node.Id, Texture = id, Kind = kind, Uv = uv });
            var texel = inputs.Sample(id, kind, uv);

            if (kind == SgSampleKind.Normal)
            {
                // The node unpacks the normal: xyz = 2 * texel - 1 (the engine reconstructs z from the two stored components).
                var x = texel.X * 2f - 1f;
                var y = texel.Y * 2f - 1f;
                var z = (float)Math.Sqrt(Math.Max(0f, 1f - x * x - y * y));
                texel = Normalize3(new Val(x, y, z, 0f));
                texel.W = 1f;
            }
            return outName == "R" ? Val.Splat(texel.X) : outName == "G" ? Val.Splat(texel.Y) : outName == "B" ? Val.Splat(texel.Z) : outName == "A" ? Val.Splat(texel.W) : texel;
        }

        private Val EvaluateSubGraph(Frame frame, SgNode node, SgSlot output)
        {
            var guid = ExtractSubGraphGuid((string)node.Raw["m_SerializedSubGraph"]);
            var sub = guid == null ? null : subGraphs?.Invoke(guid);
            if (sub == null || sub.OutputNode == null)
            {
                Warnings.Add("sub graph " + node.Name + " not found");
                return Unknown(frame, node);
            }
            if (++depth > 16)
            {
                depth--;
                Warnings.Add("sub graphs nested too deep at " + node.Name);
                return default;
            }

            try
            {
                var subFrame = new Frame { Doc = sub, Args = new Dictionary<string, Val>() };
                var ids = node.Raw["m_PropertyIds"] as JArray;
                var guids = node.Raw["m_PropertyGuids"] as JArray;
                if (ids != null && guids != null)
                {
                    for (var i = 0; i < ids.Count && i < guids.Count; i++)
                    {
                        if (!sub.PropertiesByGuid.TryGetValue((string)guids[i], out var property)) continue;
                        var slot = node.Slot((int)ids[i]);
                        if (slot == null) continue;
                        TryInput(frame, node, slot, out var arg);
                        subFrame.Args[property.Id] = arg;
                    }
                }

                // The node's output slot ids are those of the sub graph's output node inputs; fall back to the names.
                var outputSlot = sub.OutputNode.Slot(output?.Id ?? 0);
                if (outputSlot == null || !outputSlot.IsInput)
                    foreach (var candidate in sub.OutputNode.Slots)
                        if (candidate.IsInput && candidate.Name == output?.Name) outputSlot = candidate;
                if (outputSlot == null) return default;
                TryInput(subFrame, sub.OutputNode, outputSlot, out var result);
                return result;
            }
            finally
            {
                depth--;
            }
        }

        private static string ExtractSubGraphGuid(string serialized)
        {
            if (string.IsNullOrEmpty(serialized)) return null;
            try
            {
                return (string)JObject.Parse(serialized)["subGraph"]?["guid"];
            }
            catch (Newtonsoft.Json.JsonException)
            {
                return null;
            }
        }

        private static string ExtractGuid(string serialized)
        {
            if (string.IsNullOrEmpty(serialized)) return null;
            try
            {
                var guid = (string)JObject.Parse(serialized)["texture"]?["guid"];
                return string.IsNullOrEmpty(guid) ? null : guid;
            }
            catch (Newtonsoft.Json.JsonException)
            {
                return null;
            }
        }

        public static float GammaToLinear(float v)
        {
            if (v <= 0f) return 0f;
            return v <= 0.04045f ? v / 12.92f : (float)Math.Pow((v + 0.055f) / 1.055f, 2.4f);
        }

        public static float LinearToGamma(float v)
        {
            if (v <= 0f) return 0f;
            return v <= 0.0031308f ? v * 12.92f : 1.055f * (float)Math.Pow(v, 1f / 2.4f) - 0.055f;
        }
    }
}
