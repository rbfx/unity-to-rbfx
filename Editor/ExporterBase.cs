using System;
using System.Collections;
using System.Text;
using UnityEditor;

namespace UnityToRebelFork.Editor
{
    public abstract class ExporterBase
    {
        private static readonly char[] InvalidFileNameChars =
        {
            '\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07', '\x08', '\x09', '\x0A',
            '\x0B', '\x0C', '\x0D', '\x0E', '\x0F', '\x10', '\x11', '\x12', '\x13', '\x14', '\x15',
            '\x16', '\x17', '\x18', '\x19', '\x1A', '\x1B', '\x1C', '\x1D', '\x1E', '\x1F', '\x22',
            '\x3C', '\x3E', '\x7C', ':', '*', '?', '\\', '/'
        };

        protected NameCollisionResolver nameCollisionResolver;
        protected readonly ExportContext context;
        protected readonly ExportSettings _settings;

        public ExporterBase(NameCollisionResolver nameCollisionResolver, ExportContext context, ExportSettings settings)
        {
            this.nameCollisionResolver = nameCollisionResolver;
            this.context = context;
            this._settings = settings;
        }

        /// <summary>
        /// Export settings.
        /// </summary>
        public ExportSettings Settings => _settings;

        /// <summary>
        /// Get safe file name from string with all special characters replaced with "_".
        /// </summary>
        /// <param name="name">Original name.</param>
        /// <returns>Safe file name.</returns>
        public static string SafeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            foreach (var invalidFileNameChar in InvalidFileNameChars)
                name = name.Replace(invalidFileNameChar, '_');

            return name;
        }

        public string GetUniqueName(UnityEngine.Object asset)
        {
            if (!string.IsNullOrWhiteSpace(asset.name))
            {
                return nameCollisionResolver.GetUniqueName(asset);
            }

            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var guid, out long localId))
                return guid + "_" + localId;

            return asset.GetInstanceID().ToString();
            //return asset.GetInstanceID().ToString();
        }

        public string EvaluateResourcePath(UnityEngine.Object asset, string fileExtension)
        {
            if (asset == null)
                return null;

            // Get asset path in the asset database
            var assetPath = AssetDatabase.GetAssetPath(asset);

            // If the asset isn't in asset database then maybe it's an instance of prefab?
            // Then we need to graph path to the prefab.
            if (string.IsNullOrWhiteSpace(assetPath))
                assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(asset);

            // Still nothing? Let's put it in a temporal folder related to the current scene.
            if (string.IsNullOrWhiteSpace(assetPath))
                assetPath = CombinePath(context.TempFolder, asset.name + ".ext");

            if (System.IO.File.Exists(assetPath) && AssetDatabase.LoadAllAssetsAtPath(assetPath).Length == 1)
                return ReplaceExtension(GetRelPathFromAssetPath(assetPath), fileExtension);
            var newExt = "/" + SafeFileName(DecorateName(GetUniqueName(asset))) + fileExtension;
            return ReplaceExtension(GetRelPathFromAssetPath(assetPath), newExt);
        }

        /// <summary>
        /// Combine path segments into one string.
        /// </summary>
        /// <param name="segments">Resource path segments.</param>
        /// <returns>Resource path.</returns>
        public static string CombinePath(params string[] segments)
        {
            var path = new StringBuilder();
            var separator = "";
            foreach (var segment in segments)
            {
                if (string.IsNullOrWhiteSpace(segment)) continue;

                path.Append(separator);
                separator = "/";

                path.Append(segment);

                if (segment.EndsWith("/"))
                    separator = "";
            }

            return path.ToString();
        }

        public string DecorateName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;
            //if (!Options.ASCIIOnly)
            //    return name;

            return Uri.EscapeDataString(name);
        }

        /// <summary>
        /// Replace extension in the asset path.
        /// </summary>
        /// <param name="resourcePath">Path to resource.</param>
        /// <param name="newExt">New extension.</param>
        /// <returns>Path with replaced extension.</returns>
        public static string ReplaceExtension(string resourcePath, string newExt)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
                return null;
            var lastDot = resourcePath.LastIndexOf('.');
            var lastSlash = resourcePath.LastIndexOf('/');
            if (lastDot > lastSlash) return resourcePath.Substring(0, lastDot) + newExt;

            return resourcePath + newExt;
        }

        /// <summary>
        /// Get relative path from asset path.
        /// </summary>
        /// <param name="assetPath">Asset path in Unity.</param>
        /// <returns>Corresponding resource path in RebelFork.</returns>
        public string GetRelPathFromAssetPath(string assetPath)
        {
            var result = assetPath.Replace('\\', '/');

            if (result.StartsWith("Assets/", StringComparison.InvariantCultureIgnoreCase))
                result = result.Substring("Assets/".Length);

            return result;
        }
    }

    public abstract class ExporterBase<T> : ExporterBase, IExporter
    {
        /// <summary>
        /// Create base exporter.
        /// </summary>
        /// <param name="settings">Export settings.</param>
        public ExporterBase(NameCollisionResolver nameCollisionResolver, ExportContext context, ExportSettings settings) : base(nameCollisionResolver, context, settings)
        {
        }

        /// <inheritdoc/>
        bool IExporter.CanExport(object asset)
        {
            return asset is T;
        }

        /// <inheritdoc/>
        string IExporter.EvaluateResourcePath(object asset)
        {
            if (asset is T instance)
            {
                return EvaluateResourcePath(instance);
            }

            return null;
        }

        /// <inheritdoc/>
        IEnumerable IExporter.Export(object asset)
        {
            if (asset is T instance)
            {
                return Export(instance);
            }

            return Array.Empty<string>();
        }

        /// <summary>
        /// Evaluate resource path in RebelFork.
        /// </summary>
        /// <param name="asset">Asset to export.</param>
        /// <returns>Corresponding resource path in RebelFork or null if can't evaluate.</returns>
        public abstract string EvaluateResourcePath(T asset);

        /// <summary>
        /// Export coroutine.
        /// </summary>
        /// <param name="asset">Asset to export.</param>
        /// <returns>Unity coroutine.</returns>
        public abstract IEnumerable Export(T asset);

        /// <summary>
        /// Get relative path from asset.
        /// </summary>
        /// <param name="asset">Exported asset.</param>
        /// <returns>Corresponding resource path in RebelFork or null if can't evaluate.</returns>
        public string GetRelPathFromAsset(T asset)
        {
            if (asset is UnityEngine.Object unityObject)
            {
                var path = AssetDatabase.GetAssetPath(unityObject);
                return GetRelPathFromAssetPath(path);
            }
            else if (asset is UnityEngine.SceneManagement.Scene scene)
            {
                return GetRelPathFromAssetPath(scene.path);
            }

            return null;
        }
    }
}