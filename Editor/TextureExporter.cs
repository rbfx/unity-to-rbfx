using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEditor;

namespace UnityToRebelFork.Editor
{
    public class TextureExporter: ExporterBase<UnityEngine.Texture>
    {
        public TextureExporter()
        {
        }

        public override string EvaluateResourcePath(UnityEngine.Texture asset)
        {
            return EvaluateResourcePath(asset, ".dds");
        }

        public override IEnumerable Export(UnityEngine.Texture asset)
        {
            yield return asset.name;

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
            new TextureProcessor().ProcessAndSaveTexture(asset,
                "Hidden/UnityToRebelFork/Copy", Settings.ResolveResourceFileName(EvaluateResourcePath(asset)),
                textureImporter?.GetAutomaticFormat("Standalone") != TextureImporterFormat.DXT1, new Dictionary<string, float>
                {
                    {"_GammaInput",( PlayerSettings.colorSpace == UnityEngine.ColorSpace.Linear)?0.0f:1.0f},
                    {"_GammaOutput",1.0f},
                });
        }

        private void ExportNormalMap(UnityEngine.Texture asset)
        {
            new TextureProcessor().ProcessAndSaveTexture(asset,
                Settings.PackNormals
                    ? "Hidden/UnityToRebelFork/DecodeNormalMapPackedNormal"
                    : "Hidden/UnityToRebelFork/DecodeNormalMap",
                Settings.ResolveResourceFileName(EvaluateResourcePath(asset)));
        }
    }
}
