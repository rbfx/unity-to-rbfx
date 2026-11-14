using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// Turns humanoid animation clips (muscle curves, retargetable) into rbfx animations of one character's bones by
    /// sampling the clip on an instance of the character, frame by frame, and recording the local transform of every
    /// bone of its skinned meshes. The result plays on that character's exported skeleton.
    /// </summary>
    public static class HumanoidClipBaker
    {
        /// <summary>
        /// Animations/&lt;rig&gt;/&lt;clip&gt;.ani. Clips from "Model@Clip.fbx" files are named after the part after '@'.
        /// </summary>
        public static string EvaluateResourcePath(AnimationClip clip, GameObject rig)
        {
            return $"Animations/{ExporterBase.SafeFileName(rig.name)}/{ExporterBase.SafeFileName(ClipName(clip))}.ani";
        }

        public static string ClipName(AnimationClip clip)
        {
            var path = AssetDatabase.GetAssetPath(clip);
            if (!string.IsNullOrEmpty(path))
            {
                var file = Path.GetFileNameWithoutExtension(path);
                int at = file.IndexOf('@');
                if (at >= 0 && at + 1 < file.Length)
                {
                    // A file with several takes keeps the take names.
                    var clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                        .Count(c => !c.name.StartsWith("__preview__"));
                    return clips <= 1 ? file.Substring(at + 1) : file.Substring(at + 1) + "_" + clip.name;
                }
            }
            return clip.name;
        }

        /// <param name="generic">
        /// The clip is not humanoid (the rig has no human avatar): its curves are sampled on the rig as they are. The top-level bones are then recorded
        /// in the space of the skinned mesh, which is what the exported model's bones are in, so what lies above them (the armature of a Blender
        /// file, turned to make Z up) is part of them.
        /// </param>
        public static void Bake(AnimationClip clip, GameObject rig, ExportSettings settings, string resourcePath, bool generic = false)
        {
            var instance = (GameObject)UnityEngine.Object.Instantiate(rig);
            instance.hideFlags = HideFlags.HideAndDontSave;
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;
            try
            {
                var animator = instance.GetComponentInChildren<Animator>();
                if (!generic && (animator == null || animator.avatar == null || !animator.avatar.isHuman))
                    throw new System.InvalidOperationException($"{rig.name} has no Animator with a humanoid avatar to bake {clip.name} on.");
                if (animator != null) animator.applyRootMotion = false;
                var sampleTarget = animator != null ? animator.gameObject : instance;
                var meshSpace = generic ? (instance.GetComponentInChildren<SkinnedMeshRenderer>(true)?.transform.worldToLocalMatrix ?? Matrix4x4.identity) : Matrix4x4.identity;

                var bones = CollectBones(instance);
                float frameRate = clip.frameRate > 0f ? clip.frameRate : 30f;
                int frames = Mathf.Max(1, Mathf.CeilToInt(clip.length * frameRate - 1e-3f));
                var times = new List<float>();
                for (int i = 0; i < frames; i++) times.Add(i / frameRate);
                times.Add(clip.length);

                var samples = bones.ToDictionary(b => b, _ => new List<(float, Vector3, Quaternion, Vector3)>());
                // Humanoid root motion moves the instance (the bones' parents), not the bones. The top-level bones are
                // recorded relative to where their parent was at the first frame, so the motion stays in their tracks;
                // clips without root motion come out as plain local transforms.
                var topLevel = bones.Where(b => b.parent == null || !bones.Contains(b.parent)).ToList();
                var reference = new Dictionary<Transform, Matrix4x4>();
                AnimationMode.StartAnimationMode();
                try
                {
                    foreach (var t in times)
                    {
                        AnimationMode.BeginSampling();
                        AnimationMode.SampleAnimationClip(sampleTarget, clip, t);
                        AnimationMode.EndSampling();
                        if (reference.Count == 0)
                        {
                            foreach (var bone in topLevel)
                                reference[bone] = generic ? meshSpace : bone.parent != null ? bone.parent.worldToLocalMatrix : Matrix4x4.identity;
                        }
                        foreach (var bone in bones)
                        {
                            if (reference.TryGetValue(bone, out var toStart) && (generic || bone.parent != null))
                            {
                                var local = toStart * bone.localToWorldMatrix;
                                samples[bone].Add((t, local.GetColumn(3), local.rotation, generic ? local.lossyScale : bone.localScale));
                            }
                            else
                            {
                                samples[bone].Add((t, bone.localPosition, bone.localRotation, bone.localScale));
                            }
                        }
                    }
                }
                finally
                {
                    AnimationMode.StopAnimationMode();
                }

                using (var stream = settings.CreateFile(resourcePath))
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(new byte[] { 0x55, 0x41, 0x4e, 0x49 });
                    writer.WriteStringSZ(ClipName(clip));
                    writer.Write(clip.length);
                    writer.Write(bones.Count);
                    foreach (var bone in bones)
                    {
                        var list = samples[bone];
                        writer.WriteStringSZ(System.Uri.EscapeDataString(bone.name)); // As the bone is named in the model (see ExporterBase.DecorateName).
                        writer.Write((byte)7); // Position, rotation and scale.
                        writer.Write(list.Count);
                        foreach (var (time, position, rotation, scale) in list)
                        {
                            writer.Write(time);
                            writer.Write(position);
                            writer.Write(rotation);
                            writer.Write(scale);
                        }
                    }
                }

                WriteSidecar(clip, settings, resourcePath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        /// <summary>The bones of every skinned mesh under the rig, parents first, without duplicates.</summary>
        private static List<Transform> CollectBones(GameObject instance)
        {
            var result = new List<Transform>();
            var seen = new HashSet<Transform>();
            foreach (var skin in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                foreach (var bone in skin.bones)
                {
                    if (bone != null && seen.Add(bone)) result.Add(bone);
                }
            }
            result.Sort((a, b) => Depth(a).CompareTo(Depth(b)));
            return result;
        }

        private static int Depth(Transform t)
        {
            int depth = 0;
            for (var p = t.parent; p != null; p = p.parent) depth++;
            return depth;
        }

        private static void WriteSidecar(AnimationClip clip, ExportSettings settings, string resourcePath)
        {
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\"?>");
            xml.AppendLine("<animation>");
            var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
            xml.AppendLine($"\t<metadata name=\"Looped\" type=\"Bool\" value=\"{(clipSettings.loopTime ? "true" : "false")}\" />");
            xml.AppendLine($"\t<metadata name=\"FrameRate\" type=\"Float\" value=\"{clip.frameRate.ToString(CultureInfo.InvariantCulture)}\" />");
            var velocity = clip.averageSpeed;
            xml.AppendLine($"\t<metadata name=\"Velocity\" type=\"Vector3\" value=\"{F(velocity.x)} {F(velocity.y)} {F(velocity.z)}\" />");
            foreach (var e in AnimationUtility.GetAnimationEvents(clip))
            {
                var value = string.IsNullOrEmpty(e.stringParameter) ? e.functionName : e.functionName + ":" + e.stringParameter;
                xml.AppendLine($"\t<trigger time=\"{F(e.time)}\" type=\"String\" value=\"{System.Security.SecurityElement.Escape(value)}\" />");
            }
            xml.AppendLine("</animation>");

            var sidecar = Path.ChangeExtension(resourcePath, ".xml");
            using (var stream = settings.CreateFile(sidecar))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(xml.ToString());
        }

        private static string F(float v) => v.ToString(CultureInfo.InvariantCulture);
    }
}
