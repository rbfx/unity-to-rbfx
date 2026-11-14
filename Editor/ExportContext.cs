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

        /// <summary>
        /// With <see cref="HumanoidRig"/>: clips that are not humanoid (a generic rig) are sampled on the rig too, instead of exported from their curves.
        /// The curves leave out what is above the model's root bone (the armature node of a Blender file, turned to make Z up); sampling puts it into the root bone.
        /// </summary>
        public bool BakeClipsOnRig { get; set; }

        /// <summary>What was written for each particle system, and what could not be converted (see <see cref="ParticleSystemExporter"/>).</summary>
        public System.Collections.Generic.List<EffectReport> EffectReports { get; } = new System.Collections.Generic.List<EffectReport>();
    }
}