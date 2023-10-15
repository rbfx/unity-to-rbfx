using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityToRebelFork.Editor
{
    public class ExportContext
    {
        public ExportContext(UnityEngine.SceneManagement.Scene scene)
        {
        }

        public ExportContext(GameObject prefabRoot)
        {
        }

        public string TempFolder { get; set; } = "Tmp/";
    }
}