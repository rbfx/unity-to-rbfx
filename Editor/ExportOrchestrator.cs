using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;

namespace UnityToRebelFork.Editor
{
    public class ExportOrchestrator
    {
        /// <summary>
        /// Available exporters.
        /// </summary>
        private readonly IExporter[] exporters;

        /// <summary>
        /// Visited assets that are already scheduled for export.
        /// </summary>
        private readonly Dictionary<object, string> visitedAssets = new Dictionary<object, string>();
        
        /// <summary>
        /// Gate object to lock access to queue.
        /// </summary>
        private readonly object gate = new object();

        /// <summary>
        /// Queue of enumeration factories.
        /// </summary>
        private readonly Queue<Func<IEnumerable>> exportQueue = new Queue<Func<IEnumerable>>();

        /// <summary>
        /// Number of processed assets to display correct progress bar.
        /// </summary>
        private int exportCounter;

        /// <summary>
        /// Current processed enumeration.
        /// </summary>
        private IEnumerator enumeration;

        /// <summary>
        /// A stopwatch to measure update time.
        /// This allows to process several resources during a single frame update but not to block the update completely.
        /// </summary>
        private readonly Stopwatch foregroundStepStopwatch = new Stopwatch();

        /// <summary>
        /// Queue processing delegate instance.
        /// </summary>
        private readonly EditorApplication.CallbackFunction callback;


        public ExportOrchestrator(IEnumerable<IExporter> exporters)
        {
            this.exporters = exporters.ToArray();
            callback = ProcessQueue;
        }

        public string ScheduleExport(object asset)
        {
            if (asset == null)
            {
                return null;
            }

            if (visitedAssets.TryGetValue(asset, out var path))
            {
                return path;
            }

            foreach (var exporter in exporters)
            {
                if (exporter.CanExport(asset))
                {
                    path = ScheduleExport(asset, exporter);
                    visitedAssets[asset] = path;
                    return path;
                }
            }

            return null;
        }

        public string ScheduleExport(object asset, IExporter exporter)
        {
            ScheduleForegroundTask(() => exporter.Export(asset));
            return exporter.EvaluateResourcePath(asset);
        }

        public void ScheduleForegroundTask(Func<IEnumerable> task)
        {
            lock (gate)
            {
                if (exportQueue.Count == 0)
                {
                    exportCounter = 0;
                    EditorApplication.update =
                        Delegate.Combine(EditorApplication.update, callback) as EditorApplication.CallbackFunction;
                }

                exportQueue.Enqueue(task);
            }
        }

        private void ProcessQueue()
        {
            string progressBarMessage = null;
            foregroundStepStopwatch.Restart();
            while (foregroundStepStopwatch.Elapsed.TotalMilliseconds < 16)
            {
                if (enumeration != null)
                {
                    try
                    {
                        if (!enumeration.MoveNext())
                        {
                            //enumeration.Dispose();
                            enumeration = null;
                        }
                        else
                        {
                            progressBarMessage = string.Format("{0}", enumeration.Current);
                        }
                    }
                    catch (Exception exception)
                    {
                        progressBarMessage = exception.ToString();
                        UnityEngine.Debug.LogError(exception);
                        //enumeration?.Dispose();
                        enumeration = null;
                    }
                }
                else
                {
                    Func<IEnumerable> task;
                    lock (gate)
                    {
                        if (exportQueue.Count == 0)
                        {
                            EditorUtility.ClearProgressBar();
                            EditorApplication.update =
                                Delegate.Remove(EditorApplication.update, callback) as
                                    EditorApplication.CallbackFunction;
                            return;
                        }

                        task = exportQueue.Dequeue();
                        ++exportCounter;
                    }

                    try
                    {
                        enumeration = task().GetEnumerator();
                    }
                    catch (Exception exception)
                    {
                        progressBarMessage = exception.ToString();
                        UnityEngine.Debug.LogError(exception);
                    }
                }
            }

            EditorUtility.DisplayProgressBar("Exporting to RebelFork", progressBarMessage,
                exportCounter / (exportCounter + exportQueue.Count + 1));
        }
    }
}