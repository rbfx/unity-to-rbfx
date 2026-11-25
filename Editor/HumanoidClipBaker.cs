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
    /// bone of its skinned meshes. The result plays on that character's exported skeleton. The placement of the root node at the first frame (what a humanoid clip
    /// puts the root motion at when it starts) is not baked: frame 0 of the top-level bones is the bind pose, and only the motion after it is kept.
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
                // The space of the skinned mesh before the clip moves anything: the exported model has its bones in it, so whatever lies above the
                // top-level bones (a biped's root dummy, the armature of a Blender file) is part of them, in the clip as in the bind pose.
                var meshSpace = MeshSpace(instance);

                var bones = CollectBones(instance);
                float frameRate = clip.frameRate > 0f ? clip.frameRate : 30f;
                int frames = Mathf.Max(1, Mathf.CeilToInt(clip.length * frameRate - 1e-3f));
                var times = new List<float>();
                for (int i = 0; i < frames; i++) times.Add(i / frameRate);
                times.Add(clip.length);

                var samples = bones.ToDictionary(b => b, _ => new List<(float, Vector3, Quaternion, Vector3)>());
                // Humanoid root motion moves the instance (the bones' parents), not the bones. The top-level bones are
                // recorded in the space the skinned mesh had before the clip, so the motion stays in their tracks and so does what lies above
                // them; clips without root motion come out as the bone's transform in the bind pose's space.
                var topLevel = bones.Where(b => b.parent == null || !bones.Contains(b.parent)).ToList();
                var reference = new Dictionary<Transform, Matrix4x4>();
                // A humanoid clip places the avatar's root node at the root motion of its first frame (the take's own body-centre height, a start offset sideways), even when it does not
                // travel: that start placement is not part of the motion and would carry the whole character off the ground. The top-level bones are therefore recorded as the motion
                // since the first frame applied to the bind pose (frame 0 is the bind pose); a clip that really travels or turns keeps that. Generic clips are sampled as they are.
                var bindWorld = new Dictionary<Transform, Matrix4x4>();
                var firstWorldInverse = new Dictionary<Transform, Matrix4x4>();
                if (!generic)
                    foreach (var bone in topLevel)
                        bindWorld[bone] = bone.localToWorldMatrix;
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
                            {
                                reference[bone] = meshSpace;
                                if (!generic) firstWorldInverse[bone] = bone.localToWorldMatrix.inverse;
                            }
                        }
                        foreach (var bone in bones)
                        {
                            if (reference.TryGetValue(bone, out var toStart))
                            {
                                var world = bone.localToWorldMatrix;
                                if (!generic) world = world * firstWorldInverse[bone] * bindWorld[bone]; // The motion since the first frame, on the bind pose.
                                var local = toStart * world;
                                // The scale is the one of the matrix, not bone.localScale: what lies above the top-level bone (an armature scaled 0.01 over bones in metres) is part of it, as it is of the position.
                                samples[bone].Add((t, local.GetColumn(3), local.rotation, local.lossyScale));
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

        /// <summary>
        /// The space the clip's top-level bones are recorded in: that of the skinned mesh whose transform is nearest to the rig's origin. A character made of parts has one skinned mesh per
        /// part, and a part's object may be placed away from the others (the blouse of the Vampire Girl is 1.07 m below the origin): the first mesh found must not decide, or every clip
        /// would carry that offset (the character would float). The parts' models share one skeleton; the ones at the origin are what the skeleton's bind pose is written in.
        /// </summary>
        private static Matrix4x4 MeshSpace(GameObject instance)
        {
            var best = Matrix4x4.identity;
            float bestDistance = float.MaxValue;
            foreach (var skin in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var space = skin.transform.worldToLocalMatrix;
                // How far the mesh's transform is from the rig's origin: its offset plus how much it is turned.
                float distance = ((Vector3)space.GetColumn(3)).magnitude + Quaternion.Angle(Quaternion.identity, space.rotation) * 0.01f;
                if (distance < bestDistance - 1e-6f)
                {
                    bestDistance = distance;
                    best = space;
                }
            }
            return best;
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
