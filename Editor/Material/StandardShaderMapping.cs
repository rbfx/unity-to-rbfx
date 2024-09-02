using System;
using UnityEngine;
using UnityToRebelFork.Editor.Shaders;

namespace UnityToRebelFork.Editor
{
    public class StandardShaderMapping: ShaderMappingBase, IShaderMapping
    {

        public int Priority { get; } = 0;

        public StandardShaderMapping(Lazy<ExportOrchestrator> orchestrator, ExportSettings settings) : base(orchestrator, settings)
        {
        }

        public bool CanMap(UnityEngine.Shader shader)
        {
            if (shader.name == StandardShaderAdapter.ShaderName)
                return true;
            return false;
        }

        public MaterialModel Map(UnityEngine.Material material)
        {
            var model = new MaterialModel();

            MapCommonParameters(material, model);
            MapDefaultTechnique(material, model);

            var shaderArgs = new Shaders.StandardShaderAdapter(material);

            model.MatDiffColor = shaderArgs._Color;
            model.MatEmissiveColor = (Vector4)shaderArgs._EmissionColor;
            model.NormalScale = shaderArgs._BumpScale;
            model.AlphaCutoff = shaderArgs._Cutoff;
            model.Metallic = shaderArgs._Metallic;
            model.Roughness = 1.0f-shaderArgs._Glossiness;

            if(shaderArgs._BumpMap != null)
                model.Normal = orchestrator.Value.ScheduleExport(shaderArgs._BumpMap);

            if (shaderArgs._EmissionMap != null)
                model.Emission = orchestrator.Value.ScheduleExport(shaderArgs._EmissionMap);

            if (shaderArgs._MainTex != null)
                model.Albedo = orchestrator.Value.ScheduleExport(shaderArgs._MainTex);

            if (shaderArgs._OcclusionMap != null || shaderArgs._MetallicGlossMap != null)
            {
                var recipe = new TextureRecipe();
                // R - Roughness ------------
                if (shaderArgs._SmoothnessTextureChannel < 0.5)
                {
                    if (shaderArgs._MetallicGlossMap != null)
                    {
                        recipe.RChannel = shaderArgs._MetallicGlossMap;
                        recipe.RMask = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                        recipe.RSourceRange = new Vector2(1.0f, 0.0f);
                    }
                }
                else
                {
                    if (shaderArgs._MainTex != null)
                    {
                        recipe.RChannel = shaderArgs._MainTex;
                        recipe.RMask = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                        recipe.RSourceRange = new Vector2(1.0f, 0.0f);
                    }
                }
                // G - Metallic ------------
                if (shaderArgs._MetallicGlossMap != null)
                {
                    recipe.GChannel = shaderArgs._MetallicGlossMap;
                    recipe.GMask = new Vector4(0.299f, 0.587f, 0.114f, 0.0f);
                }

                // A - Occlusion ------------
                if (shaderArgs._OcclusionMap != null)
                {
                    recipe.AChannel = shaderArgs._OcclusionMap;
                    recipe.AMask = new Vector4(0.299f, 0.587f, 0.114f, 0.0f);
                }

                model.Properties = orchestrator.Value.ScheduleExport(recipe);
            }

            return model;
        }
    }
}