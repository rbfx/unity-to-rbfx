using UnityEditor;
using UnityEngine.Scripting;

namespace UnityToRebelFork.Editor
{
    [Preserve]
    public class ExportWindowEditor : UnityEditor.EditorWindow
    {
        static ExportSettings _settings;
        [MenuItem("Tools/RebelFork/Export assets")]
        public static void ExportAssets()
        {
            GetWindow<ExportWindowEditor>("Export to RebelFork", true);
        }

        public ExportSettingsEditor Editor { get; set; }

        public void OnGUI()
        {
            if (_settings == null)
            {
                var path = "Assets/RebelForkExportSettings.asset";
                if (!System.IO.File.Exists(path))
                {
                    _settings = new ExportSettings();
                    UnityEditor.AssetDatabase.CreateAsset(_settings, path);
                }
                else
                {
                    _settings = AssetDatabase.LoadAssetAtPath<ExportSettings>(path);
                }
            }

            if (Editor == null)
            {
                Editor = UnityEditor.Editor.CreateEditor(_settings) as ExportSettingsEditor;
                if (Editor != null)
                {
                    Editor.InspectorMode = false;
                }
            }

            Editor?.OnInspectorGUI();
            //ExportSettingsEditor.RenderGui(_settings, true);
        }
    }
}