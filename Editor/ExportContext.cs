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

        /// <summary>Toe target: the cube map XMLs (resource paths) that were exported and are to get their roughness chain baked afterwards (see <see cref="CubemapExporter"/>).</summary>
        public System.Collections.Generic.List<string> CubemapsToPrefilter { get; } = new System.Collections.Generic.List<string>();

        /// <summary>What was written for each particle system, and what could not be converted (see <see cref="ParticleSystemExporter"/>).</summary>
        public System.Collections.Generic.List<EffectReport> EffectReports { get; } = new System.Collections.Generic.List<EffectReport>();

        /// <summary>
        /// Toe target: whether the sprite and audio exporters (<see cref="SpriteSheetExporter"/>, <see cref="SpriteAnimationExporter"/>, <see cref="AudioClipExporter"/>) take the
        /// assets they know. On for exporting by hand; a batch export turns it on only when it was asked for sprites or audio, so that exporting a character does not change
        /// what happens to a texture that happens to be imported as a sprite.
        /// </summary>
        public bool ClaimSpriteAssets { get; set; } = true;

        /// <summary>
        /// Toe target: pixels per unit forced on the sprites of the textures under a folder (or of one texture), by Unity asset path. A pack is drawn for its own unit (the Hippo
        /// monsters for 16 pixels), a game has one: this makes a 32 pixel sprite one 32 pixel cell. The longest matching path wins; textures without a rule keep what the importer says.
        /// </summary>
        public System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, float>> SpritePixelsPerUnitRules { get; } =
            new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, float>>();

        /// <summary>The pixels per unit a rule forces on the sprites of this texture, or null.</summary>
        public float? SpritePixelsPerUnitFor(string assetPath)
        {
            float? result = null;
            int length = -1;
            foreach (var rule in SpritePixelsPerUnitRules)
            {
                var folder = rule.Key.TrimEnd('/');
                bool matches = string.Equals(assetPath, folder, System.StringComparison.OrdinalIgnoreCase)
                               || assetPath.StartsWith(folder + "/", System.StringComparison.OrdinalIgnoreCase);
                if (matches && folder.Length > length)
                {
                    result = rule.Value;
                    length = folder.Length;
                }
            }
            return result;
        }

        /// <summary>Toe target: the resource paths of exported audio files that are to be encoded to Ogg Vorbis afterwards (see <see cref="AudioClipExporter"/>).</summary>
        public System.Collections.Generic.List<string> AudioToEncode { get; } = new System.Collections.Generic.List<string>();

        /// <summary>The sprite sheets that were written in this run, by the resource path of their JSON file, so that a sheet used by several animation sets is written once.</summary>
        public System.Collections.Generic.Dictionary<string, WrittenSheet> WrittenSheets { get; } = new System.Collections.Generic.Dictionary<string, WrittenSheet>();
    }
}