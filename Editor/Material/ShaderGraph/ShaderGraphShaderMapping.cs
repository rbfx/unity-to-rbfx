using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityToRebelFork.Editor.ShaderGraphs;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// The mapping of materials whose shader is a Shader Graph. The graph is read from its file and evaluated with the material's own values
    /// (<see cref="ShaderGraphAnalyzer"/>) to find out which textures are the albedo, the normal map, the roughness / metallic / occlusion masks, the emission and the opacity,
    /// what colour they are multiplied by and how the coordinates are tiled, whatever the properties are called. A graph the reader cannot make sense of goes to the default mapping.
    /// </summary>
    public class ShaderGraphShaderMapping : ShaderMappingBase, IShaderMapping
    {
        private sealed class CachedGraph
        {
            public DateTime WriteTime;
            public ShaderGraphDocument Document;
        }

        private readonly DefaultShaderMapping fallback;
        private readonly Dictionary<string, CachedGraph> graphs = new Dictionary<string, CachedGraph>();
        private readonly HashSet<string> reported = new HashSet<string>();

        public int Priority { get; } = -5;

        public ShaderGraphShaderMapping(Lazy<ExportOrchestrator> orchestrator, ExportSettings settings, DefaultShaderMapping fallback) : base(orchestrator, settings)
        {
            this.fallback = fallback;
        }

        public bool CanMap(UnityEngine.Shader shader)
        {
            if (shader == null) return false;
            var path = AssetDatabase.GetAssetPath(shader);
            return path != null && path.EndsWith(".shadergraph", StringComparison.OrdinalIgnoreCase) && LoadGraph(path) != null;
        }

        private ShaderGraphDocument LoadGraph(string assetPath)
        {
            try
            {
                var file = new System.IO.FileInfo(assetPath);
                if (!file.Exists) return null;
                if (graphs.TryGetValue(assetPath, out var cached) && cached.WriteTime == file.LastWriteTimeUtc) return cached.Document;

                ShaderGraphDocument document = null;
                var text = System.IO.File.ReadAllText(assetPath);
                if (ShaderGraphDocument.IsSupported(text))
                    document = ShaderGraphDocument.Parse(text);
                else if (reported.Add(assetPath))
                    UnityEngine.Debug.LogWarning("[ShaderGraph] '" + assetPath + "' is not in the Shader Graph text format of Unity 2020.2 and later: its materials use the default mapping.");
                graphs[assetPath] = new CachedGraph { WriteTime = file.LastWriteTimeUtc, Document = document };
                return document;
            }
            catch (Exception exception)
            {
                if (reported.Add(assetPath))
                    UnityEngine.Debug.LogWarning("[ShaderGraph] could not read '" + assetPath + "': " + exception.Message);
                return null;
            }
        }

        private ShaderGraphDocument LoadSubGraph(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(path) ? null : LoadGraph(path);
        }

        public MaterialModel Map(UnityEngine.Material material)
        {
            ShaderGraphDocument document;
            ShaderGraphAnalysis analysis;
            MaterialGraphInputs inputs;
            try
            {
                document = LoadGraph(AssetDatabase.GetAssetPath(material.shader));
                if (document == null) return fallback.Map(material);
                inputs = new MaterialGraphInputs(material, PlayerSettings.colorSpace == UnityEngine.ColorSpace.Linear);
                analysis = new ShaderGraphAnalyzer(document, inputs, LoadSubGraph).Analyze();
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning("[ShaderGraph] material '" + material.name + "': the graph could not be analysed (" + exception.Message + "), using the default mapping.", material);
                return fallback.Map(material);
            }

            var model = Build(material, document, analysis, inputs);
            UnityEngine.Debug.Log("[ShaderGraph] material '" + material.name + "' (" + material.shader.name + "): " + string.Join(" | ", analysis.Notes)
                                  + (analysis.Warnings.Count > 0 ? " | warnings: " + string.Join(", ", analysis.Warnings) : ""), material);
            return model;
        }

        private static float Gamma(float linear, bool linearColorSpace) => linearColorSpace ? ShaderGraphEvaluator.LinearToGamma(linear) : linear;

        private static float Clamp01(float v) => Mathf.Clamp01(v);

        private static bool IsTransparent(UnityEngine.Material material)
        {
            if (material.HasProperty("_Surface")) return material.GetFloat("_Surface") > 0.5f;
            if (material.HasProperty("_SurfaceType")) return material.GetFloat("_SurfaceType") > 0.5f;
            return material.renderQueue >= (int)RenderQueue.Transparent;
        }

        private static bool IsAlphaClip(UnityEngine.Material material)
        {
            if (material.HasProperty("_AlphaClip")) return material.GetFloat("_AlphaClip") > 0.5f;
            if (material.HasProperty("_AlphaCutoffEnable")) return material.GetFloat("_AlphaCutoffEnable") > 0.5f;
            return material.IsKeywordEnabled("_ALPHATEST_ON");
        }

        private static bool IsDoubleSided(UnityEngine.Material material)
        {
            if (material.HasProperty("_Cull")) return (int)material.GetFloat("_Cull") == (int)UnityEngine.Rendering.CullMode.Off;
            if (material.HasProperty("_DoubleSidedEnable")) return material.GetFloat("_DoubleSidedEnable") > 0.5f;
            return false;
        }

        private static Vector4 Unit(int channel) => new Vector4(channel == 0 ? 1 : 0, channel == 1 ? 1 : 0, channel == 2 ? 1 : 0, channel == 3 ? 1 : 0);

        private MaterialModel Build(UnityEngine.Material material, ShaderGraphDocument document, ShaderGraphAnalysis analysis, MaterialGraphInputs inputs)
        {
            var model = new MaterialModel();
            var linear = inputs.LinearColorSpace;

            var albedo = inputs.Resolve(analysis.Albedo);
            var normal = inputs.Resolve(analysis.Normal);
            var alphaTexture = inputs.Resolve(analysis.Alpha.Texture);
            var emission = inputs.Resolve(analysis.Emission);

            var transparent = IsTransparent(material);
            var clip = IsAlphaClip(material);
            var needAlpha = alphaTexture != null && (clip || transparent);

            MapCommonParameters(material, model, normal != null);
            // The queue says nothing about a graph: the alpha test is on when the material clips and the graph's alpha comes from a texture.
            model.PSDefines.Remove("ALPHAMASK");
            if (clip && needAlpha)
            {
                model.PSDefines.Add("ALPHAMASK");
                model.AlphaCutoff = analysis.AlphaCutoff;
            }
            model.Techniques.Add(new TechniqueModel { Name = transparent ? "Techniques/LitTransparent.xml" : "Techniques/LitOpaque.xml" });
            if (document.IsUnlit)
                analysis.Notes.Add("unlit graph exported as a lit material");

            if (IsDoubleSided(material))
            {
                model.Cull = CullMode.None;
                model.ShadowCull = CullMode.None;
            }

            // Albedo ---------------------------------------------------------------------------------------------------------------------------------------------
            var tint = analysis.AlbedoTint;
            model.MatDiffColor = new Vector4(Gamma(tint.X, linear), Gamma(tint.Y, linear), Gamma(tint.Z, linear), Clamp01(tint.W));
            var recipeNeeded = analysis.AlbedoInverted || needAlpha && !analysis.AlphaFromAlbedo;
            if (recipeNeeded && (albedo != null || alphaTexture != null))
            {
                model.Albedo = orchestrator.Value.ScheduleExport(BuildAlbedoRecipe(analysis, albedo, needAlpha ? alphaTexture : null, linear));
                // The colour constant of an albedo built here stays in the material.
            }
            else if (albedo != null)
            {
                model.Albedo = orchestrator.Value.ScheduleExport(albedo);
            }

            // Normal map -----------------------------------------------------------------------------------------------------------------------------------------
            if (normal != null)
            {
                model.Normal = orchestrator.Value.ScheduleExport(normal);
                model.NormalScale = analysis.NormalScale;
            }

            // Roughness, metallic, occlusion: the engine reads them from one texture (R roughness, G metallic, A occlusion) that its parameters multiply. ----------
            var roughnessTexture = inputs.Resolve(analysis.Roughness.Texture);
            var metallicTexture = inputs.Resolve(analysis.Metallic.Texture);
            var occlusionTexture = inputs.Resolve(analysis.Occlusion.Texture);
            if (roughnessTexture != null || metallicTexture != null || occlusionTexture != null)
            {
                var host = roughnessTexture ?? metallicTexture ?? occlusionTexture;
                var recipe = new TextureRecipe();
                Channel(analysis.Roughness, roughnessTexture, host, out recipe.RChannel, out recipe.RMask, out recipe.RSourceRange, out recipe.RDestinationRange);
                Channel(analysis.Metallic, metallicTexture, host, out recipe.GChannel, out recipe.GMask, out recipe.GSourceRange, out recipe.GDestinationRange);
                Channel(analysis.Occlusion, occlusionTexture, host, out recipe.AChannel, out recipe.AMask, out recipe.ASourceRange, out recipe.ADestinationRange);
                model.Properties = orchestrator.Value.ScheduleExport(recipe);
                model.Roughness = 1f;
                model.Metallic = 1f;
            }
            else
            {
                model.Roughness = Clamp01(analysis.Roughness.Constant);
                model.Metallic = Clamp01(analysis.Metallic.Constant);
            }

            // Emission -------------------------------------------------------------------------------------------------------------------------------------------
            var emissionTint = analysis.EmissionTint;
            if (emission != null)
                model.Emission = orchestrator.Value.ScheduleExport(emission);
            model.MatEmissiveColor = new Vector3(Gamma(emissionTint.X, linear), Gamma(emissionTint.Y, linear), Gamma(emissionTint.Z, linear));

            // Tiling ---------------------------------------------------------------------------------------------------------------------------------------------
            if (analysis.HasUv)
            {
                // The graph's coordinates are Unity's (v up); the engine's are v down, so the meshes' v is 1 - v and the textures are flipped: u' = UA*u + UB*v + UC
                // with v = 1 - v_e, and the texture row is 1 - v'.
                model.UOffset = new Vector4(analysis.UA, -analysis.UB, 0f, analysis.UB + analysis.UC);
                model.VOffset = new Vector4(-analysis.VA, analysis.VB, 0f, 1f - analysis.VB - analysis.VC);
            }

            return model;
        }

        /// <summary>
        /// The albedo texture with what the graph does to it that a multiplier cannot say: the colour channels remapped (a graph that inverts the texture) and the alpha channel
        /// taken from the opacity mask. A texture without a source colour is white, so that the material's colour is the colour.
        /// </summary>
        private static TextureRecipe BuildAlbedoRecipe(ShaderGraphAnalysis analysis, UnityEngine.Texture albedo, UnityEngine.Texture alphaTexture, bool linear)
        {
            var host = albedo ?? alphaTexture;
            var importer = albedo == null ? null : AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(albedo)) as TextureImporter;
            var recipe = new TextureRecipe { ToGamma = linear && (importer == null || importer.sRGBTexture) && albedo != null };

            for (var channel = 0; channel < 3; channel++)
            {
                UnityEngine.Texture texture;
                Vector4 mask;
                Vector2 destination;
                if (albedo != null)
                {
                    texture = albedo;
                    mask = Unit(channel);
                    destination = analysis.AlbedoInverted ? new Vector2(analysis.AlbedoAtZero[channel], analysis.AlbedoAtOne[channel]) : new Vector2(0f, 1f);
                }
                else
                {
                    texture = host;
                    mask = Vector4.zero;
                    destination = new Vector2(1f, 1f);
                }
                if (channel == 0) { recipe.RChannel = texture; recipe.RMask = mask; recipe.RDestinationRange = destination; }
                else if (channel == 1) { recipe.GChannel = texture; recipe.GMask = mask; recipe.GDestinationRange = destination; }
                else { recipe.BChannel = texture; recipe.BMask = mask; recipe.BDestinationRange = destination; }
            }

            if (alphaTexture != null)
            {
                recipe.AChannel = alphaTexture;
                recipe.AMask = Unit(analysis.Alpha.Channel);
                recipe.ADestinationRange = new Vector2(analysis.Alpha.AtZero, analysis.Alpha.AtOne);
            }
            else
            {
                recipe.AChannel = host;
                recipe.AMask = Vector4.zero;
                recipe.ADestinationRange = new Vector2(1f, 1f);
            }
            return recipe;
        }

        /// <summary>One channel of the packed properties texture: a channel of a texture mapped to what the graph makes of it, or a constant (a zero mask of any texture and a flat range).</summary>
        private static void Channel(ScalarSource source, UnityEngine.Texture texture, UnityEngine.Texture host, out UnityEngine.Texture channel, out Vector4 mask, out Vector2 sourceRange, out Vector2 destinationRange)
        {
            sourceRange = new Vector2(0f, 1f);
            if (texture != null)
            {
                channel = texture;
                mask = Unit(source.Channel);
                destinationRange = new Vector2(source.AtZero, source.AtOne);
                return;
            }
            var constant = Clamp01(source.Constant);
            channel = host;
            mask = Vector4.zero;
            destinationRange = new Vector2(constant, constant);
        }
    }
}
