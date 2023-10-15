using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace UnityToRebelFork.Editor
{
    public class MaterialExporter: ExporterBase<UnityEngine.Material>
    {
        private IShaderMapping[] shaderMappings;

        public MaterialExporter(IShaderMapping[] shaderMappings)
        {
            this.shaderMappings = shaderMappings.OrderBy(_=>_.Priority).ToArray();
        }

        public override string EvaluateResourcePath(UnityEngine.Material material)
        {
            return EvaluateResourcePath(material, ".material");
        }

        public override IEnumerable Export(UnityEngine.Material asset)
        {
            yield return asset.name;

            using (var stream = _settings.CreateFile(EvaluateResourcePath(asset)))
            {
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    foreach (var shaderMapping in shaderMappings)
                    {
                        if (shaderMapping.CanMap(asset.shader))
                        {
                            shaderMapping.Map(asset).Save(writer);
                            yield break;
                        }
                    }
                }
            }
        }
    }
}