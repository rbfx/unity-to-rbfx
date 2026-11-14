using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;

namespace UnityToRebelFork.Editor
{
    public class MaterialExporter: ExporterBase<UnityEngine.Material>
    {
        private IShaderMapping[] shaderMappings;
        private IShaderMapping defaultShaderMapping;

        public MaterialExporter(IEnumerable<IShaderMapping> shaderMappings, DefaultShaderMapping defaultShaderMapping, NameCollisionResolver nameCollisionResolver, ExportContext context, ExportSettings settings) : base(nameCollisionResolver, context, settings)
        {
            this.shaderMappings = shaderMappings.Where(_=>_ != defaultShaderMapping).OrderBy(_=>_.Priority).ToArray();
            this.defaultShaderMapping = defaultShaderMapping;
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
                    if (!TryExport(asset, asset.shader, writer))
                    {
                        var fallback = GetFallbackShader(asset.shader);
                        if (!TryExport(asset, fallback, writer))
                        {
                            defaultShaderMapping.Map(asset).Save(writer);
                        }
                    }
                }
            }
        }

        private UnityEngine.Shader GetFallbackShader(UnityEngine.Shader shader)
        {
            string shaderPath = AssetDatabase.GetAssetPath(shader);
            // Built-in shaders have a path like "Library/unity default resources" that is not a file.
            if (string.IsNullOrEmpty(shaderPath) || !System.IO.File.Exists(shaderPath))
            {
                return null;
            }

            // Read the shader file
            string[] shaderLines = System.IO.File.ReadAllLines(shaderPath);
            foreach (string line in shaderLines)
            {
                // Check if the line contains the 'Fallback' directive
                if (line.TrimStart().StartsWith("Fallback", System.StringComparison.OrdinalIgnoreCase))
                {
                    // Extract the fallback shader name
                    string[] parts = line.Split('"');
                    if (parts.Length >= 2)
                    {
                        return UnityEngine.Shader.Find(parts[1]);
                    }
                }
            }
            return null;
        }

        public bool TryExport(UnityEngine.Material asset, UnityEngine.Shader shader, StreamWriter writer)
        {
            if (shader == null)
                return false;

            foreach (var shaderMapping in shaderMappings)
            {
                if (shaderMapping.CanMap(shader))
                {
                    shaderMapping.Map(asset).Save(writer);
                    return true;
                }
            }
            return false;
        }
    }
}