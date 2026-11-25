using System;
using UnityEngine.Rendering;

namespace UnityToRebelFork.Editor
{
    public class ShaderMappingBase
    {
        protected Lazy<ExportOrchestrator> orchestrator;

        protected ExportSettings settings;

        public ShaderMappingBase(Lazy<ExportOrchestrator> orchestrator, ExportSettings settings)
        {
            this.orchestrator = orchestrator;
            this.settings = settings;
        }

        protected void MapCommonParameters(UnityEngine.Material material, MaterialModel model, bool normalMap)
        {
            var alphaTest = material.renderQueue == (int)RenderQueue.AlphaTest;

            model.PSDefines.Add("PBR");
            model.VSDefines.Add("PBR");
            if (normalMap)
            {
                model.PSDefines.Add("NORMALMAP");
                model.VSDefines.Add("NORMALMAP");
            }
            if (alphaTest)
                model.PSDefines.Add("ALPHAMASK");
            if (settings.PackNormals)
                model.PSDefines.Add("PACKEDNORMAL");
            // The shaders of Unity that map to this material do not look at the colour of a vertex, but rbfx multiplies the albedo by it when the mesh has one (a mesh authored with
            // vertex colours for a mask, such as a head with all zero, would come out black). The materials of custom shaders keep it: they may be made to use it.
            if (material.shader != null && IgnoresVertexColor(material.shader.name))
            {
                model.PSDefines.Add("IGNOREVERTEXCOLOR");
                model.VSDefines.Add("IGNOREVERTEXCOLOR");
            }
        }

        private static readonly string[] VertexColorIgnoringPrefixes =
        {
            "Universal Render Pipeline/Lit",
            "Universal Render Pipeline/Simple Lit",
            "Universal Render Pipeline/Complex Lit",
            "Universal Render Pipeline/Baked Lit",
            "HDRP/Lit",
            "Standard",
            "Legacy Shaders/",
            "Mobile/",
        };

        /// <summary>True for the shaders of Unity (and of this package) that do not use the colour of the vertices, whatever the mesh holds: the material of rbfx must not either.</summary>
        public static bool IgnoresVertexColor(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName)) return false;
            foreach (var prefix in VertexColorIgnoringPrefixes)
                if (shaderName.StartsWith(prefix, StringComparison.Ordinal)) return true;
            return false;
        }

        protected void MapDefaultTechnique(UnityEngine.Material material, MaterialModel model)
        {
            var transparent = material.renderQueue == (int)RenderQueue.Transparent;
            model.Techniques.Add(new TechniqueModel() { Name = transparent ? "Techniques/LitTransparent.xml" : "Techniques/LitOpaque.xml" });
        }
    }
}