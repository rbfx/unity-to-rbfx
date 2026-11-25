using System;
using NUnit.Framework;
using UnityEngine;
using UnityToRebelFork.Editor;

namespace UnityToRebelFork
{
    /// <summary>The four RBFX lit shaders (opaque and transparent, built-in and URP) map to the Toe / rbfx material: the transparent ones to the transparent technique, with the emission.</summary>
    [TestFixture]
    public class LitShadersTests
    {
        [TestCase("RBFX/LitOpaque", false)]
        [TestCase("RBFX/URP/LitOpaque", false)]
        [TestCase("RBFX/LitTransparent", true)]
        [TestCase("RBFX/URP/LitTransparent", true)]
        public void TheShaderMapsToTheTechniqueOfItsBlendMode_WithTheEmission(string shaderName, bool transparent)
        {
            var shader = UnityEngine.Shader.Find(shaderName);
            Assert.IsNotNull(shader, shaderName + " is part of this package");
            var material = new UnityEngine.Material(shader);
            try
            {
                material.SetColor("_EmissionColor", new Color(2f, 1f, 0.5f, 1f));
                var mapping = new LitOpaqueShaderMapping(new Lazy<ExportOrchestrator>(() => null), ScriptableObject.CreateInstance<ExportSettings>());
                Assert.IsTrue(mapping.CanMap(shader));
                var model = mapping.Map(material);
                Assert.AreEqual(transparent ? "Techniques/LitTransparent.xml" : "Techniques/LitOpaque.xml", model.Techniques[0].Name);
                Assert.AreEqual(2f, model.MatEmissiveColor.x, 1e-4f);
                Assert.AreEqual(0.5f, model.MatEmissiveColor.z, 1e-4f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
            }
        }
    }
}
