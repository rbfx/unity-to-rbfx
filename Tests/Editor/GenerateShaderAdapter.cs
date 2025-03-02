using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityToRebelFork.Editor;

namespace UnityToRebelFork
{
    [TestFixture]
    public class GenerateShaderAdapter
    {
        [Test]
        [TestCaseSource(nameof(ShaderNames))]
        public void GenerateShader(string shaderName)
        {
            var adapterBuilder = new StringBuilder();

            var pathSeparator = shaderName.LastIndexOf('/');
            var name = shaderName.Substring(pathSeparator + 1);
            var nameSpace = (pathSeparator > 0) ? shaderName.Substring(0, pathSeparator) : "";
            nameSpace = nameSpace.Replace('/', '.').Replace(" ", "");

            var foundShader = UnityEngine.Shader.Find(shaderName);

            if (nameSpace.Length > 0)
                adapterBuilder.AppendLine("namespace UnityToRebelFork.Editor.Shaders."+nameSpace);
            else
                adapterBuilder.AppendLine("namespace UnityToRebelFork.Editor.Shaders");
            adapterBuilder.AppendLine("{");
            adapterBuilder.AppendLine($"    public class {name}ShaderAdapter");
            adapterBuilder.AppendLine("    {");
            adapterBuilder.AppendLine($"        public static readonly string ShaderName = \"{shaderName}\";");
            adapterBuilder.AppendLine();
            adapterBuilder.AppendLine($"        UnityEngine.Material material;");
            adapterBuilder.AppendLine();
            adapterBuilder.AppendLine($"        public {name}ShaderAdapter(UnityEngine.Material material)");
            adapterBuilder.AppendLine("        {");
            adapterBuilder.AppendLine("            this.material = material;");
            adapterBuilder.AppendLine("        }");

            for (int i = 0; i < foundShader.GetPropertyCount(); ++i)
            {
                var propertyName = foundShader.GetPropertyName(i);
                var propertyType = foundShader.GetPropertyType(i);
                switch (propertyType)
                {
                    case ShaderPropertyType.Color:
                        adapterBuilder.AppendLine();
                        adapterBuilder.AppendLine($"        public UnityEngine.Color {propertyName}");
                        adapterBuilder.AppendLine("        {");
                        adapterBuilder.AppendLine($"            get {{ return material.GetColor(\"{propertyName}\"); }}");
                        adapterBuilder.AppendLine("        }");
                        break;
                    case ShaderPropertyType.Texture:
                        adapterBuilder.AppendLine();
                        adapterBuilder.AppendLine($"        public UnityEngine.Texture {propertyName}");
                        adapterBuilder.AppendLine("        {");
                        adapterBuilder.AppendLine($"            get {{ return material.GetTexture(\"{propertyName}\"); }}");
                        adapterBuilder.AppendLine("        }");
                        break;
                    case ShaderPropertyType.Range:
                    case ShaderPropertyType.Float:
                        adapterBuilder.AppendLine();
                        adapterBuilder.AppendLine($"        public float {propertyName}");
                        adapterBuilder.AppendLine("        {");
                        adapterBuilder.AppendLine($"            get {{ return material.GetFloat(\"{propertyName}\"); }}");
                        adapterBuilder.AppendLine("        }");
                        break;
                }
            }

            adapterBuilder.AppendLine("    }");
            adapterBuilder.AppendLine("}");

            var geneatedCode = adapterBuilder.ToString();

            TestContext.Out.WriteLine(geneatedCode);
        }

        [Test]
        public void GenerateShader2()
        {
        }

        public static string[] ShaderNames()
        {
            return ShaderUtil.GetAllShaderInfo().Select(_=>_.name).ToArray();
        }
    }
}