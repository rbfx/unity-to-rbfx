using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace UnityToRebelFork.Editor
{
    public class TextureExporter: ExporterBase<UnityEngine.Texture>
    {
        public TextureExporter(NameCollisionResolver nameCollisionResolver, ExportContext context, ExportSettings settings) : base(nameCollisionResolver, context, settings)
        {
        }

        public override string EvaluateResourcePath(UnityEngine.Texture asset)
        {
            return EvaluateResourcePath(asset, ".dds");
        }

        public override IEnumerable Export(UnityEngine.Texture asset)
        {
            yield return asset.name;

            // A cube map is not a 2D image; probes and the scene's reflection go through CubemapExporter (HDR, cube DDS plus XML).
            if (asset.dimension == UnityEngine.Rendering.TextureDimension.Cube)
            {
                UnityEngine.Debug.LogWarning($"[TextureExporter] '{asset.name}' is a cube map; only reflection probes and the scene's default reflection export those, so {EvaluateResourcePath(asset)} is not written.", asset);
                yield break;
            }

            var tImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;
            var texType = tImporter?.textureType ?? TextureImporterType.Default;
            if (texType == TextureImporterType.NormalMap)
            {
                ExportNormalMap(asset);
            }
            else
            {
                ExportDefaultMap(asset, tImporter);
            }
        }

        private void ExportDefaultMap(UnityEngine.Texture asset, TextureImporter textureImporter)
        {
            // A colour texture is sampled as linear in a linear project and written back as the gamma values it was made of. A texture that is not sRGB (a normal map, a mask, roughness / metallic
            // / occlusion) is sampled as it is: its values are written as they are, not run through the gamma curve (which would change them).
            bool data = textureImporter != null && !textureImporter.sRGBTexture;
            float gammaInput = data ? 0.0f : (PlayerSettings.colorSpace == UnityEngine.ColorSpace.Linear ? 0.0f : 1.0f);
            float gammaOutput = data ? 0.0f : 1.0f;
            new TextureProcessor().ProcessAndSaveTexture(asset,
                "Hidden/UnityToRebelFork/Copy", Settings.ResolveResourceFileName(EvaluateResourcePath(asset)),
                hasAlpha: textureImporter?.GetAutomaticFormat("Standalone") != TextureImporterFormat.DXT1, shaderArgs: new Dictionary<string, float>
                {
                    {"_GammaInput", gammaInput},
                    {"_GammaOutput", gammaOutput},
                });
        }

        private void ExportNormalMap(UnityEngine.Texture asset)
        {
            new TextureProcessor().ProcessAndSaveTexture(asset,
                Settings.PackNormals
                    ? "Hidden/UnityToRebelFork/DecodeNormalMapPackedNormal"
                    : "Hidden/UnityToRebelFork/DecodeNormalMap",
                Settings.ResolveResourceFileName(EvaluateResourcePath(asset)), compress: Settings.PackNormals);
        }
    }
}
