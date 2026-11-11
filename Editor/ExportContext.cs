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

        /// <summary>
        /// A character prefab with a humanoid Animator. When set, humanoid clips are baked onto its skeleton by sampling
        /// (see <see cref="HumanoidClipBaker"/>) instead of exported from their curves, which hold muscle values.
        /// </summary>
        public GameObject HumanoidRig { get; set; }
    }
}