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

        protected void MapCommonParameters(UnityEngine.Material material, MaterialModel model)
        {
            var alphaTest = material.renderQueue == (int)RenderQueue.AlphaTest;

            model.PSDefines.Add("PBR");
            model.VSDefines.Add("PBR");
            if (alphaTest)
                model.PSDefines.Add("ALPHAMASK");
            if (settings.PackNormals)
                model.PSDefines.Add("PACKEDNORMAL");
        }

        protected void MapDefaultTechnique(UnityEngine.Material material, MaterialModel model)
        {
            var transparent = material.renderQueue == (int)RenderQueue.Transparent;
            model.Techniques.Add(new TechniqueModel() { Name = transparent ? "Techniques/LitTransparent.xml" : "Techniques/LitOpaque.xml" });
        }
    }
}