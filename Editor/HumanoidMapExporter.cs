using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// The humanoid bones of a character with a humanoid avatar, by the names its bones have in the exported model. A game that retargets
    /// animations between characters needs to know which bone is the left upper arm; Unity's avatar knows, whatever the rig calls it.
    /// The names of the keys are those of <see cref="HumanBodyBones"/>.
    /// </summary>
    public static class HumanoidMapExporter
    {
        /// <summary>HumanBodyBones name to bone name, or null if the character has no humanoid avatar.</summary>
        public static Dictionary<string, string> Build(GameObject character)
        {
            var instance = (GameObject)UnityEngine.Object.Instantiate(character);
            instance.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                var animator = instance.GetComponentInChildren<Animator>();
                if (animator == null || animator.avatar == null || !animator.avatar.isHuman) return null;

                var map = new Dictionary<string, string>();
                foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones)))
                {
                    if (bone == HumanBodyBones.LastBone) continue;
                    var transform = animator.GetBoneTransform(bone);
                    if (transform != null) map[bone.ToString()] = Uri.EscapeDataString(transform.name);
                }
                return map;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        /// <summary>The map as a JSON object (the value of "humanBones" in a manifest, and the content of a .humanoid.json file).</summary>
        public static string ToJson(Dictionary<string, string> map, string indent)
        {
            string Q(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            var lines = map.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => $"{indent}  {Q(p.Key)}: {Q(p.Value)}");
            return "{\n" + string.Join(",\n", lines) + "\n" + indent + "}";
        }

        /// <summary>Writes &lt;model&gt;.humanoid.json next to an exported model, where the engine looks for overrides of its own bone mapping.</summary>
        public static void Write(ExportSettings settings, string modelResourcePath, Dictionary<string, string> map)
        {
            if (string.IsNullOrEmpty(modelResourcePath) || map == null) return;
            var path = Path.ChangeExtension(modelResourcePath, ".humanoid.json");
            using (var stream = settings.CreateFile(path))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write("{ \"humanBones\": " + ToJson(map, "") + " }\n");
        }
    }
}
