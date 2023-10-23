using UnityEngine.Rendering;
using Zenject;

namespace UnityToRebelFork.Editor
{
    public class ShaderMappingBase
    {
        [Inject] protected ExportOrchestrator orchestrator;

        [Inject] protected ExportSettings settings;

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