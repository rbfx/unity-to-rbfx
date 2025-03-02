using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Text;
using Rbfx.LightInject;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Linq;

namespace UnityToRebelFork.Editor
{

    public class ExportRedirector: IExporter
    {
        private readonly ExportOrchestrator _orchestrator;

        public ExportRedirector(ExportOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        public bool CanExport(object asset)
        {
            return true;
        }

        public string EvaluateResourcePath(object asset)
        {
            if (asset == null)
                return null;

            return asset.ToString();
        }

        public IEnumerable Export(object asset)
        {
            if (asset is string path)
            {
                yield return path;

                if (!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path))
                    yield break;

                var attrs = System.IO.File.GetAttributes(path);
                if (attrs.HasFlag(FileAttributes.Directory))
                {
                    foreach (var guid in AssetDatabase.FindAssets("", new[] { path }))
                        _orchestrator.ScheduleExport(AssetDatabase.GUIDToAssetPath(guid), this);
                }
                else
                {
                    // Export main asset.
                    var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                    _orchestrator.ScheduleExport(mainAsset);

                    // Export all other assets except game objects.
                    // Game objects are exported as prefabs and the root prefab game object should be at the main asset anyway.
                    var allAssetsExceptGameObjects = AssetDatabase.LoadAllAssetsAtPath(path)
                        .Where(asset=>asset.GetType() != typeof(GameObject) && asset!= mainAsset);
                    foreach (var subAsset in allAssetsExceptGameObjects)
                    {
                        _orchestrator.ScheduleExport(subAsset);
                    }
                }
            }
        }
    }
    [CustomEditor(typeof(ExportSettings))]
    public class ExportSettingsEditor : UnityEditor.Editor
    {
        private ServiceContainer _container;

        public bool InspectorMode { get; set; } = true;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = (ExportSettings)target;

            if (script == null)
                return;

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

            if (!string.IsNullOrEmpty(script.path) && !script.path.EndsWith("Data") && !script.path.EndsWith("Data/") && !script.path.EndsWith("Data\\"))
            {
                EditorGUILayout.HelpBox("The path doesn't end with Data. Are you sure you've picked a correct path to a Data folder?", MessageType.Warning);
            }

            //if (!InspectorMode)
            {
                var selected = Selection.assetGUIDs;
                if (selected.Length > 0)
                {
                    string selectedStr;
                    if (selected.Length == 1)
                    {
                        selectedStr = AssetDatabase.GUIDToAssetPath(selected[0]);
                    }
                    else
                    {
                        selectedStr = $"{selected.Length} assets: {AssetDatabase.GUIDToAssetPath(selected[0])}, ...";
                    }
                    if (GUILayout.Button($"Export {selectedStr}", GUILayout.Height(40)))
                    {
                        foreach (var selectedGuid in selected)
                        {
                            _container = new ();
                            RebelForkInstaller.Install(_container, script, new ExportContext(SceneManager.GetActiveScene()));
                            var orchestrator = _container.GetInstance<ExportOrchestrator>();
                            orchestrator.ScheduleExport(AssetDatabase.GUIDToAssetPath(selectedGuid), new ExportRedirector(orchestrator));
                        }
                    }
                }
            }
            //else
            {
                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

                if (prefabStage != null)
                {
                    if (GUILayout.Button($"Export Prefab ({Path.GetFileName(prefabStage.prefabAssetPath)})", GUILayout.Height(40)))
                    {
                        _container = new(new ContainerOptions { EnableVariance = true });
                        var prefabRootObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabStage.assetPath);
                        RebelForkInstaller.Install(_container, script, new ExportContext(prefabRootObject));
                        var orchestrator = _container.GetInstance<ExportOrchestrator>();
                        orchestrator.ScheduleExport(prefabRootObject);
                    }
                }
                else
                {
                    if (GUILayout.Button($"Export Scene ({Path.GetFileName(SceneManager.GetActiveScene().path)})", GUILayout.Height(40)))
                    {
                        _container = new();
                        var activeScene = SceneManager.GetActiveScene();
                        RebelForkInstaller.Install(_container, script, new ExportContext(activeScene));
                        var orchestrator = _container.GetInstance<ExportOrchestrator>();
                        orchestrator.ScheduleExport(activeScene);
                    }
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        private static string PickFolder(string path)
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
            GenerateShaderHelper(exportFolder, UnityEngine.Shader.Find("Universal Render Pipeline/Lit"));
            GenerateShaderHelper(exportFolder, UnityEngine.Shader.Find("Universal Render Pipeline/Complex Lit"));
            GenerateShaderHelper(exportFolder, UnityEngine.Shader.Find("Universal Render Pipeline/Simple Lit"));
            GenerateShaderHelper(exportFolder, UnityEngine.Shader.Find("Universal Render Pipeline/Unlit"));
            GenerateShaderHelper(exportFolder, UnityEngine.Shader.Find("HDRP/Lit"));
            GenerateShaderHelper(exportFolder, UnityEngine.Shader.Find("HDRP/Unlit"));
            GenerateShaderHelper(exportFolder, UnityEngine.Shader.Find("Mobile/VertexLit"));
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
                    writer.WriteLine($"        public static readonly string ShaderName = \"{shader.name}\";");
                    writer.WriteLine();
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