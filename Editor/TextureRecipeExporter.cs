using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public class TextureRecipeExporter : ExporterBase<TextureRecipe>
    {
        public TextureRecipeExporter()
        {
        }

        public override string EvaluateResourcePath(TextureRecipe asset)
        {
            return EvaluateResourcePath(asset.RChannel ?? asset.GChannel ?? asset.BChannel ?? asset.AChannel, "." + asset.GetHashCode() + ".dds");
        }

        public override IEnumerable Export(TextureRecipe asset)
        {
            var resourcePath = EvaluateResourcePath(asset);

            yield return Path.GetFileName(resourcePath);

            UnityEngine.Material material = null;

            var shader = UnityEngine.Shader.Find("Hidden/RebelFork/BuildTexture");
            try
            {
                material = new UnityEngine.Material(shader);
                if (asset.RChannel != null)
                {
                    material.SetTexture("_RChannelMap", asset.RChannel);
                    material.SetVector("_RChannelRanges", new Vector4(asset.RSourceRange.x, asset.RSourceRange.y, asset.RDestinationRange.x, asset.RDestinationRange.y));
                    material.SetVector("_RChannelMask", asset.RMask);
                }
                if (asset.GChannel != null)
                {
                    material.SetTexture("_GChannelMap", asset.GChannel);
                    material.SetVector("_GChannelRanges", new Vector4(asset.GSourceRange.x, asset.GSourceRange.y, asset.GDestinationRange.x, asset.GDestinationRange.y));
                    material.SetVector("_GChannelMask", asset.GMask);
                }
                if (asset.BChannel != null)
                {
                    material.SetTexture("_BChannelMap", asset.BChannel);
                    material.SetVector("_BChannelRanges", new Vector4(asset.BSourceRange.x, asset.BSourceRange.y, asset.BDestinationRange.x, asset.BDestinationRange.y));
                    material.SetVector("_BChannelMask", asset.BMask);
                }
                if (asset.AChannel != null)
                {
                    material.SetTexture("_AChannelMap", asset.AChannel);
                    material.SetVector("_AChannelRanges", new Vector4(asset.ASourceRange.x, asset.ASourceRange.y, asset.ADestinationRange.x, asset.ADestinationRange.y));
                    material.SetVector("_AChannelMask", asset.AMask);
                }

                int width = 1;
                int height = 1;
                UnityEngine.Texture sourceTexture = null;
                foreach (var texture in new []{asset.RChannel, asset.GChannel, asset.BChannel, asset.AChannel}.Where(_=>_!=null))
                {
                    width = Math.Max(width, texture.width);
                    height = Math.Max(height, texture.width);
                    if (sourceTexture == null)
                        sourceTexture = texture;
                }

                var textureProcessor = new TextureProcessor();
                textureProcessor.ProcessTexture(sourceTexture, width, height, material,
                    texture => textureProcessor.SaveTexture(texture, _settings.ResolveResourceFileName(resourcePath), asset.AChannel != null));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
            }
        }
    }
}