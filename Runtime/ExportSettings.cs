using UnityEngine;
using System.IO;

namespace UnityToRebelFork.Editor
{
    [CreateAssetMenu(fileName = "RebelForkExportSettings", menuName = "RebelFork/ExportSettings", order = 1)]
    public class ExportSettings : ScriptableObject
    {
        [Tooltip("Data folder RebelFork project path")]
        public string path;

        [Tooltip("Force using only ASCII symbols for names")]
        public bool ASCIIOnly = false;

        [Tooltip("Export vertex colors.")]
        public bool VertexColors = true;

        [Tooltip("Export normal map as packed.")]
        public bool PackNormals = false;

        [Tooltip("Keep empty nodes.")]
        public bool EmptyNodes = true;

        [Tooltip("Write prefab instances in a scene as nodes of their own instead of references to exported prefabs (levels: baked collider scale, stable ids per instance).")]
        public bool InlinePrefabs = false;

        [Tooltip("Give scenes rbfx's default sky and zone (CoreData's DefaultSkybox). Off for games that light their levels themselves.")]
        public bool SceneEnvironment = true;

        public string ResolveResourceFileName(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return null;
            }

            var fullPath = Path.Combine(path, resourcePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            return fullPath;
        }

        /// <summary>
        /// Create output file if possible.
        /// </summary>
        /// <param name="resourcePath">Relative resource path.</param>
        /// <returns>Full file path or null if export is not necessary.</returns>
        public Stream CreateFile(string resourcePath)
        {
            var fullPath = ResolveResourceFileName(resourcePath);
            if (fullPath == null)
                return null;
            return System.IO.File.Open(fullPath, FileMode.Create, FileAccess.Write, FileShare.Read);
        }

    }
}