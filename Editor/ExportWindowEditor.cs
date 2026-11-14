using UnityEditor;
using UnityEngine.Scripting;

namespace UnityToRebelFork.Editor
{
    /// <summary>The export window. Each target has its own settings asset: rbfx (Tools/RebelFork) and Toe (Tools/Toe).</summary>
    [Preserve]
    public class ExportWindowEditor : UnityEditor.EditorWindow
    {
        private ExportSettings _settings;
        private ExportTarget _target = ExportTarget.Rbfx;

        [MenuItem("Tools/RebelFork/Export assets")]
        public static void ExportAssets()
        {
            Open(ExportTarget.Rbfx, "Export to RebelFork");
        }

        [MenuItem("Tools/Toe/Export assets")]
        public static void ExportAssetsToToe()
        {
            Open(ExportTarget.Toe, "Export to Toe");
        }

        private static void Open(ExportTarget target, string title)
        {
            var window = GetWindow<ExportWindowEditor>(title, true);
            // Switching between the menus changes the settings the window edits.
            if (window._target != target || window._settings == null)
            {
                window._target = target;
                window._settings = null;
                window.Editor = null;
            }
            window.titleContent = new UnityEngine.GUIContent(title);
        }

        public ExportSettingsEditor Editor { get; set; }

        private static string SettingsPath(ExportTarget target)
            => target == ExportTarget.Toe ? "Assets/ToeExportSettings.asset" : "Assets/RebelForkExportSettings.asset";

        public void OnGUI()
        {
            if (_settings == null)
            {
                var path = SettingsPath(_target);
                if (!System.IO.File.Exists(path))
                {
                    _settings = CreateInstance<ExportSettings>();
                    _settings.Target = _target;
                    UnityEditor.AssetDatabase.CreateAsset(_settings, path);
                }
                else
                {
                    _settings = AssetDatabase.LoadAssetAtPath<ExportSettings>(path);
                }
                // The asset of a menu is always for its target.
                if (_settings != null && _settings.Target != _target)
                {
                    _settings.Target = _target;
                    EditorUtility.SetDirty(_settings);
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
