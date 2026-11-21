using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using UnityToRebelFork.Editor.ShaderGraphs;

namespace UnityToRebelFork
{
    /// <summary>Plain C# (no Unity types): the reader, the evaluator and the analyser of Shader Graphs run on graphs written by hand in the text format of Shader Graph.</summary>
    [TestFixture]
    public class ShaderGraphAnalyzerTests
    {
        /// <summary>Writes a graph in the MultiJson format: objects of the graph, its properties, nodes with their slots, edges and the blocks of the master stack.</summary>
        private sealed class GraphWriter
        {
            private readonly List<string> objects = new List<string>();
            private readonly List<string> properties = new List<string>();
            private readonly List<string> nodes = new List<string>();
            private readonly List<string> edges = new List<string>();
            private int counter;

            private static string F(float v) => v.ToString("0.0###", CultureInfo.InvariantCulture);

            private string NewId() => (++counter).ToString("x32");

            public string Property(string type, string reference, string value)
            {
                var id = NewId();
                objects.Add("{\"m_Type\":\"UnityEditor.ShaderGraph.Internal." + type + "\",\"m_ObjectId\":\"" + id + "\",\"m_Guid\":{\"m_GuidSerialized\":\"g" + id + "\"},\"m_Name\":\"" + reference
                            + "\",\"m_DefaultReferenceName\":\"" + reference + "\",\"m_OverrideReferenceName\":\"\"," + value + "}");
                properties.Add(id);
                return id;
            }

            public string FloatProperty(string reference, float value) => Property("Vector1ShaderProperty", reference, "\"m_Value\":" + F(value));
            public string BoolProperty(string reference, bool value) => Property("BooleanShaderProperty", reference, "\"m_Value\":" + (value ? "true" : "false"));
            public string ColorProperty(string reference, float r, float g, float b) => Property("ColorShaderProperty", reference, "\"m_Value\":{\"r\":" + F(r) + ",\"g\":" + F(g) + ",\"b\":" + F(b) + ",\"a\":1.0},\"m_ColorMode\":1");
            public string TextureProperty(string reference) => Property("Texture2DShaderProperty", reference, "\"m_Value\":{\"m_SerializedTexture\":\"\",\"m_Guid\":\"\"},\"m_DefaultType\":0");

            /// <summary>A node with slots given as (id, name, isInput, value); returns the node's id.</summary>
            public string Node(string type, string extra, params (int id, string name, bool input, float value)[] slots)
            {
                var id = NewId();
                var slotIds = new List<string>();
                foreach (var slot in slots)
                {
                    var slotId = NewId();
                    var slotType = slot.input && slot.name == "UV" ? "UVMaterialSlot" : slot.input && slot.name == "Texture" ? "Texture2DInputMaterialSlot" : "DynamicVectorMaterialSlot";
                    objects.Add("{\"m_Type\":\"UnityEditor.ShaderGraph." + slotType + "\",\"m_ObjectId\":\"" + slotId + "\",\"m_Id\":" + slot.id + ",\"m_DisplayName\":\"" + slot.name
                                + "\",\"m_SlotType\":" + (slot.input ? 0 : 1) + ",\"m_ShaderOutputName\":\"" + slot.name + "\",\"m_Value\":{\"x\":" + F(slot.value) + ",\"y\":" + F(slot.value) + ",\"z\":"
                                + F(slot.value) + ",\"w\":" + F(slot.value) + "}}");
                    slotIds.Add("{\"m_Id\":\"" + slotId + "\"}");
                }
                objects.Add("{\"m_Type\":\"UnityEditor.ShaderGraph." + type + "\",\"m_ObjectId\":\"" + id + "\",\"m_Name\":\"" + type + "\",\"m_Slots\":[" + string.Join(",", slotIds) + "]" + extra + "}");
                nodes.Add(id);
                return id;
            }

            public string PropertyNode(string propertyId) => Node("PropertyNode", ",\"m_Property\":{\"m_Id\":\"" + propertyId + "\"}", (0, "Out", false, 0f));
            public string Multiply(float b = 1f) => Node("MultiplyNode", "", (0, "A", true, 1f), (1, "B", true, b), (2, "Out", false, 0f));
            public string Lerp() => Node("LerpNode", "", (0, "A", true, 0f), (1, "B", true, 1f), (2, "T", true, 0f), (3, "Out", false, 0f));
            public string OneMinus() => Node("OneMinusNode", "", (0, "In", true, 0f), (1, "Out", false, 0f));
            public string Split() => Node("SplitNode", "", (0, "In", true, 0f), (1, "R", false, 0f), (2, "G", false, 0f), (3, "B", false, 0f), (4, "A", false, 0f));
            public string Branch() => Node("BranchNode", "", (0, "Predicate", true, 0f), (1, "True", true, 0f), (2, "False", true, 0f), (3, "Out", false, 0f));
            public string Uv() => Node("UVNode", ",\"m_OutputChannel\":0", (0, "Out", false, 0f));

            public string Sample(bool normal = false) => Node("SampleTexture2DNode", ",\"m_TextureType\":" + (normal ? 1 : 0),
                (0, "RGBA", false, 0f), (4, "R", false, 0f), (5, "G", false, 0f), (6, "B", false, 0f), (7, "A", false, 0f), (1, "Texture", true, 0f), (2, "UV", true, 0f));

            public void Edge(string from, int fromSlot, string to, int toSlot)
            {
                edges.Add("{\"m_OutputSlot\":{\"m_Node\":{\"m_Id\":\"" + from + "\"},\"m_SlotId\":" + fromSlot + "},\"m_InputSlot\":{\"m_Node\":{\"m_Id\":\"" + to + "\"},\"m_SlotId\":" + toSlot + "}}");
            }

            /// <summary>Connects an output of a node to a block of the master stack (creating the block).</summary>
            public void Block(string name, string from, int fromSlot)
            {
                var block = Node("BlockNode", ",\"m_SerializedDescriptor\":\"SurfaceDescription." + name + "\"", (0, name, true, 0f));
                Edge(from, fromSlot, block, 0);
            }

            public string Build()
            {
                var graphId = NewId();
                var builder = new StringBuilder();
                foreach (var o in objects) builder.AppendLine(o);
                builder.AppendLine("{\"m_Type\":\"UnityEditor.ShaderGraph.GraphData\",\"m_ObjectId\":\"" + graphId + "\",\"m_Properties\":[" + Refs(properties) + "],\"m_Keywords\":[],\"m_Nodes\":[" + Refs(nodes)
                                   + "],\"m_Edges\":[" + string.Join(",", edges) + "]}");
                return builder.ToString();
            }

            private static string Refs(List<string> ids) => string.Join(",", ids.ConvertAll(i => "{\"m_Id\":\"" + i + "\"}"));
        }

        private sealed class Inputs : IShaderGraphInputs
        {
            public readonly Dictionary<string, float> Floats = new Dictionary<string, float>();
            public readonly Dictionary<string, Val> Colors = new Dictionary<string, Val>();
            public readonly HashSet<string> Textures = new HashSet<string>();

            public bool TryGetValue(SgProperty property, out Val value)
            {
                value = default;
                if (Floats.TryGetValue(property.Reference, out var f))
                {
                    value = Val.Splat(f);
                    return true;
                }
                return Colors.TryGetValue(property.Reference, out value);
            }

            public string GetTexture(SgProperty property) => Textures.Contains(property.Reference) ? property.Reference : null;
            public bool IsKeywordEnabled(string reference) => false;

            public Val Sample(string texture, SgSampleKind kind, Val uv) => kind == SgSampleKind.Normal ? new Val(0.5f, 0.5f, 1f, 1f) : new Val(0.5f, 0.5f, 0.5f, 1f);
        }

        private static ShaderGraphAnalysis Analyze(GraphWriter writer, Inputs inputs)
        {
            var document = ShaderGraphDocument.Parse(writer.Build());
            return new ShaderGraphAnalyzer(document, inputs, null).Analyze();
        }

        /// <summary>A texture property, its sample node and an edge between them.</summary>
        private static string SampleOf(GraphWriter g, Inputs inputs, string reference, bool normal = false, string uv = null)
        {
            var texture = g.PropertyNode(g.TextureProperty(reference));
            var sample = g.Sample(normal);
            g.Edge(texture, 0, sample, 1);
            if (uv != null) g.Edge(uv, 0, sample, 2);
            inputs.Textures.Add(reference);
            return sample;
        }

        [Test]
        public void TheTextureMultipliedByATintIsTheAlbedoAndTheTintIsItsColor()
        {
            var g = new GraphWriter();
            var inputs = new Inputs();
            var sample = SampleOf(g, inputs, "_Base");
            var tint = g.PropertyNode(g.ColorProperty("_Tint", 0.8f, 0.4f, 0.2f));
            var multiply = g.Multiply();
            g.Edge(sample, 0, multiply, 0);
            g.Edge(tint, 0, multiply, 1);
            g.Block("BaseColor", multiply, 2);
            inputs.Colors["_Tint"] = new Val(0.8f, 0.4f, 0.2f, 1f);

            var analysis = Analyze(g, inputs);

            Assert.AreEqual("_Base", analysis.Albedo);
            Assert.AreEqual(0.8f, analysis.AlbedoTint.X, 0.01f);
            Assert.AreEqual(0.4f, analysis.AlbedoTint.Y, 0.01f);
            Assert.AreEqual(0.2f, analysis.AlbedoTint.Z, 0.01f);
            Assert.IsFalse(analysis.AlbedoInverted);
        }

        [Test]
        public void OfTwoLayersTheOneTheStrengthSelectsIsTheAlbedo()
        {
            foreach (var strength in new[] { 0f, 1f })
            {
                var g = new GraphWriter();
                var inputs = new Inputs();
                var layer1 = SampleOf(g, inputs, "_Layer_1");
                var layer2 = SampleOf(g, inputs, "_Layer_2");
                var amount = g.PropertyNode(g.FloatProperty("_Strength", strength));
                var lerp = g.Lerp();
                g.Edge(layer1, 0, lerp, 0);
                g.Edge(layer2, 0, lerp, 1);
                g.Edge(amount, 0, lerp, 2);
                g.Block("BaseColor", lerp, 3);
                inputs.Floats["_Strength"] = strength;

                var analysis = Analyze(g, inputs);

                Assert.AreEqual(strength == 0f ? "_Layer_1" : "_Layer_2", analysis.Albedo, "strength " + strength);
            }
        }

        [Test]
        public void ABranchOnABooleanPicksTheTextureTheMaterialSelects()
        {
            foreach (var on in new[] { false, true })
            {
                var g = new GraphWriter();
                var inputs = new Inputs();
                var a = SampleOf(g, inputs, "_A");
                var b = SampleOf(g, inputs, "_B");
                var flag = g.PropertyNode(g.BoolProperty("_UseB", false));
                var branch = g.Branch();
                g.Edge(flag, 0, branch, 0);
                g.Edge(b, 0, branch, 1);
                g.Edge(a, 0, branch, 2);
                g.Block("BaseColor", branch, 3);
                inputs.Floats["_UseB"] = on ? 1f : 0f;

                Assert.AreEqual(on ? "_B" : "_A", Analyze(g, inputs).Albedo);
            }
        }

        [Test]
        public void AMaskChannelThatTheGraphReadsIsTheSourceOfMetallicAndRoughness()
        {
            var g = new GraphWriter();
            var inputs = new Inputs();
            var color = SampleOf(g, inputs, "_Base");
            var mask = SampleOf(g, inputs, "_Mask");
            var split = g.Split();
            g.Edge(mask, 0, split, 0);
            var smoothness = g.OneMinus();
            g.Edge(split, 2, smoothness, 0);
            g.Block("BaseColor", color, 0);
            g.Block("Metallic", split, 3);
            g.Block("Smoothness", smoothness, 1);

            var analysis = Analyze(g, inputs);

            Assert.AreEqual("_Base", analysis.Albedo);
            Assert.AreEqual("_Mask", analysis.Metallic.Texture);
            Assert.AreEqual(2, analysis.Metallic.Channel);
            Assert.AreEqual(0f, analysis.Metallic.AtZero, 0.01f);
            Assert.AreEqual(1f, analysis.Metallic.AtOne, 0.01f);

            // Smoothness is 1 - green, so the roughness is the green channel itself.
            Assert.AreEqual("_Mask", analysis.Roughness.Texture);
            Assert.AreEqual(1, analysis.Roughness.Channel);
            Assert.AreEqual(0f, analysis.Roughness.AtZero, 0.01f);
            Assert.AreEqual(1f, analysis.Roughness.AtOne, 0.01f);
        }

        [Test]
        public void ATextureThatOnlyMovesTheNormalIsTheNormalMap()
        {
            var g = new GraphWriter();
            var inputs = new Inputs();
            var color = SampleOf(g, inputs, "_Base");
            var normal = SampleOf(g, inputs, "_Normal", normal: true);
            g.Block("BaseColor", color, 0);
            g.Block("NormalTS", normal, 0);

            var analysis = Analyze(g, inputs);

            Assert.AreEqual("_Base", analysis.Albedo);
            Assert.AreEqual("_Normal", analysis.Normal);
            Assert.AreEqual(1f, analysis.NormalScale, 0.15f);
        }

        [Test]
        public void TilingInTheCoordinatesOfTheAlbedoIsFound()
        {
            var g = new GraphWriter();
            var inputs = new Inputs();
            var tiling = g.PropertyNode(g.FloatProperty("_Tiling", 3f));
            var uv = g.Uv();
            var scaled = g.Multiply();
            g.Edge(uv, 0, scaled, 0);
            g.Edge(tiling, 0, scaled, 1);
            var color = SampleOf(g, inputs, "_Base", uv: scaled);
            g.Block("BaseColor", color, 0);
            inputs.Floats["_Tiling"] = 3f;

            var analysis = Analyze(g, inputs);

            Assert.IsTrue(analysis.HasUv);
            Assert.AreEqual(3f, analysis.UA, 0.001f);
            Assert.AreEqual(3f, analysis.VB, 0.001f);
            Assert.AreEqual(0f, analysis.UB, 0.001f);
            Assert.AreEqual(0f, analysis.UC, 0.001f);
        }

        [Test]
        public void AGraphThatInvertsTheTextureIsReportedAsInverted()
        {
            var g = new GraphWriter();
            var inputs = new Inputs();
            var color = SampleOf(g, inputs, "_Base");
            var inverted = g.OneMinus();
            g.Edge(color, 0, inverted, 0);
            g.Block("BaseColor", inverted, 1);

            var analysis = Analyze(g, inputs);

            Assert.AreEqual("_Base", analysis.Albedo);
            Assert.IsTrue(analysis.AlbedoInverted);
            Assert.AreEqual(1f, analysis.AlbedoAtZero.X, 0.01f);
            Assert.AreEqual(0f, analysis.AlbedoAtOne.X, 0.01f);
        }

        [Test]
        public void AnOpacityMaskThatIsNotTheAlbedoIsTheSourceOfAlpha()
        {
            var g = new GraphWriter();
            var inputs = new Inputs();
            var color = SampleOf(g, inputs, "_Base");
            var mask = SampleOf(g, inputs, "_Opacity");
            g.Block("BaseColor", color, 0);
            g.Block("Alpha", mask, 4);

            var analysis = Analyze(g, inputs);

            Assert.AreEqual("_Opacity", analysis.Alpha.Texture);
            Assert.AreEqual(0, analysis.Alpha.Channel);
            Assert.IsFalse(analysis.AlphaFromAlbedo);
        }

        [Test]
        public void AnUnconnectedBlockIsAConstant()
        {
            var g = new GraphWriter();
            var inputs = new Inputs();
            var color = SampleOf(g, inputs, "_Base");
            g.Block("BaseColor", color, 0);

            var analysis = Analyze(g, inputs);

            Assert.IsNull(analysis.Normal);
            Assert.IsNull(analysis.Metallic.Texture);
            Assert.IsNull(analysis.Roughness.Texture);
            Assert.IsNull(analysis.Alpha.Texture);
        }
    }
}
