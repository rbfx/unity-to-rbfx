using System.Collections;

namespace UnityToRebelFork.Editor
{
    public interface IExporter
    {
        /// <summary>
        /// Check if exporter can export the asset.
        /// </summary>
        /// <param name="asset">Asset to export.</param>
        /// <returns>True if can export, false otherwise.</returns>
        bool CanExport(object asset);

        /// <summary>
        /// Evaluate resource path in RebelFork.
        /// </summary>
        /// <param name="asset">Asset to export.</param>
        /// <returns>Corresponding resource path in RebelFork or null if can't evaluate.</returns>
        string EvaluateResourcePath(object asset);

        /// <summary>
        /// Export coroutine.
        /// </summary>
        /// <param name="asset">Asset to export.</param>
        /// <returns>Unity coroutine.</returns>
        IEnumerable Export(object asset);
    }
}