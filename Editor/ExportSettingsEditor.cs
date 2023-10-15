using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Zenject;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace UnityToRebelFork.Editor
{
    [CustomEditor(typeof(ExportSettings))]
    public class ExportSettingsEditor : UnityEditor.Editor
    {
        private DiContainer _container;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = (ExportSettings)target;

            var hasPath = !string.IsNullOrWhiteSpace(script.path);

            if (GUILayout.Button("Pick folder"))
            {
                script.path = PickFolder(script.path);
            }

            EditorGUI.BeginDisabledGroup(!hasPath);
            if (GUILayout.Button("Open folder"))
            {
                EditorUtility.RevealInFinder(script.path);
            }

            //var selected = Selection.assetGUIDs;
            //if (selected.Length > 0)
            //{
            //    string selectedStr;
            //    if (selected.Length == 1)
            //    {
            //        selectedStr = AssetDatabase.GUIDToAssetPath(selected[0]);
            //    }
            //    else
            //    {
            //        selectedStr = $"{selected.Length} assets: {AssetDatabase.GUIDToAssetPath(selected[0])}, ...";
            //    }
            //    if (GUILayout.Button($"Export {selectedStr}", GUILayout.Height(40)))
            //    {
            //    }
            //}
            //else
            {
                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

                if (prefabStage != null)
                {
                    if (GUILayout.Button($"Export Prefab ({Path.GetFileName(prefabStage.prefabAssetPath)})", GUILayout.Height(40)))
                    {
                        _container = new DiContainer(new[] { StaticContext.Container });
                        var prefabRootObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabStage.assetPath);
                        RebelForkInstaller.Install(_container, script, new ExportContext(prefabRootObject));
                        var orchestrator = _container.Resolve<ExportOrchestrator>();
                        orchestrator.ScheduleExport(prefabRootObject);
                    }
                }
                else
                {
                    if (GUILayout.Button($"Export Scene ({Path.GetFileName(SceneManager.GetActiveScene().path)})", GUILayout.Height(40)))
                    {
                        _container = new DiContainer(new[] { StaticContext.Container });
                        var activeScene = SceneManager.GetActiveScene();
                        RebelForkInstaller.Install(_container, script, new ExportContext(activeScene));
                        var orchestrator = _container.Resolve<ExportOrchestrator>();
                        orchestrator.ScheduleExport(activeScene);
                    }
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        private string PickFolder(string path)
        {
            for (;;)
            {
                var exportFolder = EditorUtility.SaveFolderPanel("Save assets to Data folder", path, "");
                if (string.IsNullOrWhiteSpace(exportFolder))
                {
                    return path;
                }
                if (!ValidateExportPath(exportFolder))
                {
                    EditorUtility.DisplayDialog("Error",
                        "Selected path is inside Unity folder. Please select a different folder.", "Ok");
                    continue;
                }

                return exportFolder;
            }
        }

        [MenuItem("Tools/RebelFork/Generate Shader Helpers")]
        private static void GenerateShaderHelpers()
        {
            var exportFolder = EditorUtility.SaveFolderPanel("Save generated files to...", "", "");
            if (string.IsNullOrWhiteSpace(exportFolder))
            {
                return;
            }

            //var shaders = AssetDatabase.FindAssets("t:Shader");
            //foreach (var shaderId in shaders)
            //{
            //    var shader = AssetDatabase.LoadAssetAtPath<UnityEngine.Shader>(AssetDatabase.GUIDToAssetPath(shaderId));
            //    if (shader != null)
            //    {
            //        GenerateShaderHelper(exportFolder, shader);
            //    }
            //}

            GenerateShaderHelper(exportFolder, UnityEngine.Shader.Find("Standard"));
            GenerateShaderHelper(exportFolder, UnityEngine.Shader.Find("Standard (Specular setup)"));
        }

        private static void GenerateShaderHelper(string exportFolder, UnityEngine.Shader shader)
        {
            if (shader == null)
                return;

            var nameSegments = shader.name.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            for (var index = 0; index < nameSegments.Length; index++)
            {
                nameSegments[index] = ExporterBase.SafeFileName(nameSegments[index]).Replace(" ", "").Replace('(', '_').Replace(')', '_');
            }

            var path = Path.Combine(exportFolder, string.Join(Path.DirectorySeparatorChar, nameSegments)+".cs");
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (var stream = System.IO.File.Create(path))
            {
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    var name = Path.GetFileNameWithoutExtension(path);

                    writer.Write($"namespace {typeof(ExportSettingsEditor).Namespace}.Shaders");
                    for (var index = 0; index < nameSegments.Length-1; index++)
                    {
                        var nameSegment = nameSegments[index];
                        writer.Write($".{nameSegment}");
                    }
                    writer.WriteLine();

                    writer.WriteLine("{");
                    writer.WriteLine($"    public class {name}ShaderAdapter");
                    writer.WriteLine("    {");
                    writer.WriteLine("        UnityEngine.Material material;");
                    writer.WriteLine();
                    writer.WriteLine($"        public {name}ShaderAdapter(UnityEngine.Material material)");
                    writer.WriteLine("        {");
                    writer.WriteLine("            this.material = material;");
                    writer.WriteLine("        }");
                    for (int i = 0; i < shader.GetPropertyCount(); ++i)
                    {
                        writer.WriteLine();
                        var propertyName = shader.GetPropertyName(i);
                        switch (shader.GetPropertyType(i))
                        {
                            case ShaderPropertyType.Color:
                            {
                                writer.WriteLine();
                                writer.WriteLine($"        public UnityEngine.Color {propertyName}");
                                writer.WriteLine("        {");
                                writer.WriteLine("            get { return material.GetColor(\""+propertyName+"\"); }");
                                writer.WriteLine("        }");
                                break;
                            }
                            case ShaderPropertyType.Float:
                            case ShaderPropertyType.Range:
                            {
                                writer.WriteLine();
                                writer.WriteLine($"        public float {propertyName}");
                                writer.WriteLine("        {");
                                writer.WriteLine("            get { return material.GetFloat(\"" + propertyName + "\"); }");
                                writer.WriteLine("        }");
                                break;
                            }
                            case ShaderPropertyType.Texture:
                            {
                                writer.WriteLine();
                                writer.WriteLine($"        public UnityEngine.Texture {propertyName}");
                                writer.WriteLine("        {");
                                writer.WriteLine("            get { return material.GetTexture(\"" + propertyName + "\"); }");
                                writer.WriteLine("        }");
                                break;
                            }
                        }
                    }
                    writer.WriteLine("    }");
                    writer.WriteLine("}");
                }
            }
        }


        private static bool ValidateExportPath(string exportFolder)
        {
            var normalizedFolder = FixDirectorySeparator(exportFolder);
            var dataPath = FixDirectorySeparator(UnityEngine.Application.dataPath);
            return !string.IsNullOrWhiteSpace(exportFolder) &&
                   !normalizedFolder.StartsWith(dataPath, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static string FixDirectorySeparator(string path)
        {
            if (path == null)
                return null;
            if (Path.DirectorySeparatorChar == '/')
                return path.Replace('\\', Path.DirectorySeparatorChar);
            return path.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}