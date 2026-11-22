using System;
using NUnit.Framework;
using UnityEngine;
using UnityToRebelFork.Editor;

namespace UnityToRebelFork
{
    /// <summary>Materials of the shaders of Unity that ignore vertex colours must tell rbfx to ignore them too (a head authored with black vertex colours came out black).</summary>
    [TestFixture]
    public class MaterialVertexColorTests
    {
        [TestCase("Universal Render Pipeline/Lit", true)]
        [TestCase("Universal Render Pipeline/Simple Lit", true)]
        [TestCase("Standard", true)]
        [TestCase("Standard (Specular setup)", true)]
        [TestCase("Legacy Shaders/Diffuse", true)]
        [TestCase("Mobile/Diffuse", true)]
        [TestCase("Shader Graphs/Synty", false)]
        [TestCase("Sprites/Default", false)]
        [TestCase("Custom/VertexPaint", false)]
        [TestCase("", false)]
        public void OnlyTheShadersThatIgnoreVertexColourAreListed(string shaderName, bool expected)
            => Assert.AreEqual(expected, ShaderMappingBase.IgnoresVertexColor(shaderName));

        [Test]
        public void AStandardMaterial_IsMappedWithTheIgnoreDefine_InBothStages()
        {
            var shader = UnityEngine.Shader.Find("Standard");
            Assert.IsNotNull(shader, "the Standard shader is part of Unity");
            var material = new UnityEngine.Material(shader);
            try
            {
                var settings = ScriptableObject.CreateInstance<ExportSettings>();
                var model = new DefaultShaderMapping(new Lazy<ExportOrchestrator>(() => null), settings).Map(material);
                Assert.IsTrue(model.PSDefines.Contains("IGNOREVERTEXCOLOR"));
                Assert.IsTrue(model.VSDefines.Contains("IGNOREVERTEXCOLOR"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
            }
        }
    }
}
