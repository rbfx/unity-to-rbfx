using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    /// <summary>Menu items that make a Jiggle Chain and a Jiggle Collider (Toe.JiggleBones; see <see cref="JiggleChain"/>) and set up a whole character.</summary>
    public static class JiggleMenu
    {
        [MenuItem("GameObject/Toe/Jiggle Chain", false, 30)]
        private static void CreateChain(MenuCommand command) => Add<JiggleChain>(command, "Jiggle Chain");

        [MenuItem("GameObject/Toe/Jiggle Collider", false, 31)]
        private static void CreateCollider(MenuCommand command) => Add<JiggleCollider>(command, "Jiggle Collider");

        // These belong on a bone: the component goes on the selected object itself (a new object would be no bone).
        private static void Add<T>(MenuCommand command, string undoName) where T : UnityEngine.Component
        {
            var target = command.context as GameObject;
            if (target == null)
            {
                Debug.LogWarning($"[Jiggle] A {typeof(T).Name} goes on a bone: select the bone in the hierarchy first.");
                return;
            }
            if (target.GetComponent<T>() != null)
            {
                Debug.LogWarning($"[Jiggle] '{target.name}' has a {typeof(T).Name} already.");
                return;
            }
            Undo.AddComponent<T>(target);
            Selection.activeObject = target;
        }

        [MenuItem("Tools/Toe/Jiggle/Add to selected character")]
        private static void SetUpSelected()
        {
            var root = Selection.activeGameObject;
            if (root == null)
            {
                Debug.LogWarning("[Jiggle] Select the character (the root of its prefab, or the prefab itself opened in the prefab editor).");
                return;
            }
            var options = JiggleSetupOptions.Default();
            int chains = JiggleSetup.AddChains(root, options);
            int colliders = JiggleSetup.FitColliders(root, options);
            Debug.Log($"[Jiggle] {root.name}: {chains} chains and {colliders} colliders added. Tune them on the bones; the gizmos show them.");
        }
    }

    /// <summary>How <see cref="JiggleSetup.AddChains"/> finds the chains of a character: by the beginning of the name of their root bone.</summary>
    public sealed class JiggleChainRule
    {
        /// <summary>The root bones are the bones whose name starts with this (ignoring case) and whose parent's does not (so the next bones of a chain are not roots).</summary>
        public string prefix;

        public float stiffness = 0.3f;
        public float damping = 0.15f;
        public float gravity = 1f;
        public float radius = 0.03f;
        public float maxAngle = 60f;
        public float tailLength;
        public JiggleTailDirection tailDirection = JiggleTailDirection.Extend;
    }

    public sealed class JiggleSetupOptions
    {
        public List<JiggleChainRule> chains = new List<JiggleChainRule>();

        /// <summary>Colliders are fitted to the skinned meshes whose name contains one of these words, separated by commas (ignoring case); when none does, to all of them.</summary>
        public string bodyMeshFilter = "body,head";

        /// <summary>Which share of the vertices of a bone the collider covers (the largest distances are left out: a shirt hem, a finger).</summary>
        public float percentile = 0.85f;

        /// <summary>The fitted radius is made this much larger (the chains keep a little off the skin).</summary>
        public float margin = 1.05f;

        /// <summary>A skirt, breasts and hair, the rules of a girl character; a start to tune by hand.</summary>
        public static JiggleSetupOptions Default() => new JiggleSetupOptions
        {
            chains =
            {
                new JiggleChainRule { prefix = "skirt_", stiffness = 0.35f, damping = 0.15f, radius = 0.04f, maxAngle = 50f, tailDirection = JiggleTailDirection.Extend },
                new JiggleChainRule { prefix = "breast_", stiffness = 0.8f, damping = 0.3f, radius = 0.015f, maxAngle = 12f, tailLength = 0.06f, tailDirection = JiggleTailDirection.AwayFromParent },
                new JiggleChainRule { prefix = "harir_", stiffness = 0.5f, damping = 0.2f, radius = 0.02f, maxAngle = 35f, tailLength = 0.12f, tailDirection = JiggleTailDirection.Down },
                new JiggleChainRule { prefix = "hair_", stiffness = 0.5f, damping = 0.2f, radius = 0.02f, maxAngle = 35f, tailLength = 0.12f, tailDirection = JiggleTailDirection.Down },
            },
        };
    }

    /// <summary>Puts <see cref="JiggleChain"/>s on the jiggle bones of a character and <see cref="JiggleCollider"/>s on the bones of its body.</summary>
    public static class JiggleSetup
    {
        /// <summary>Adds a chain to every root bone the rules find (one that has none yet). Returns how many it added.</summary>
        public static int AddChains(GameObject root, JiggleSetupOptions options)
        {
            int added = 0;
            foreach (var bone in root.GetComponentsInChildren<Transform>(true))
            {
                if (bone == root.transform || bone.GetComponent<JiggleChain>() != null) continue;
                foreach (var rule in options.chains)
                {
                    if (!bone.name.StartsWith(rule.prefix, StringComparison.OrdinalIgnoreCase)) continue;
                    if (bone.parent != null && bone.parent.name.StartsWith(rule.prefix, StringComparison.OrdinalIgnoreCase)) break; // Inside a chain.
                    var chain = Undo.AddComponent<JiggleChain>(bone.gameObject);
                    chain.stiffness = rule.stiffness;
                    chain.damping = rule.damping;
                    chain.gravity = rule.gravity;
                    chain.radius = rule.radius;
                    chain.maxAngle = rule.maxAngle;
                    chain.tailLength = rule.tailLength;
                    chain.tailDirection = rule.tailDirection;
                    added++;
                    break;
                }
            }
            return added;
        }

        // The bones that get a collider, and the bone each limb or torso capsule reaches to (null: a sphere on the centre of the vertices).
        private static readonly (HumanBodyBones Bone, HumanBodyBones? To)[] BodyParts =
        {
            (HumanBodyBones.Hips, null),
            (HumanBodyBones.Spine, HumanBodyBones.Chest),
            (HumanBodyBones.Chest, HumanBodyBones.UpperChest),
            (HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg),
            (HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg),
            (HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot),
            (HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot),
            (HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm),
            (HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm),
            (HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand),
            (HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand),
            (HumanBodyBones.Head, null),
        };

        /// <summary>
        /// Fits a collider to each part of the body of a humanoid character: a capsule from a bone to the next for the limbs and the torso, a sphere for the hips and the head. The size is
        /// taken from the vertices of the body meshes that the bone moves most (in the bind pose of the mesh). Returns how many it added.
        /// </summary>
        public static int FitColliders(GameObject root, JiggleSetupOptions options)
        {
            var animator = root.GetComponentInChildren<Animator>(true);
            if (animator == null || animator.avatar == null || !animator.avatar.isHuman)
            {
                Debug.LogWarning($"[Jiggle] '{root.name}' has no humanoid Animator: the body colliders are not fitted (add Jiggle Colliders by hand).");
                return 0;
            }

            var points = CollectBonePoints(root, options);
            int added = 0;
            foreach (var (humanBone, to) in BodyParts)
            {
                var bone = animator.GetBoneTransform(humanBone);
                if (bone == null || bone.GetComponent<JiggleCollider>() != null) continue;
                if (!points.TryGetValue(bone, out var list) || list.Count < 20)
                {
                    Debug.LogWarning($"[Jiggle] No collider on {humanBone}: too few vertices of the body are moved by it.");
                    continue;
                }

                var next = to.HasValue ? animator.GetBoneTransform(to.Value) : null;
                var collider = Undo.AddComponent<JiggleCollider>(bone.gameObject);
                if (next == null)
                {
                    var centre = Vector3.zero;
                    foreach (var p in list) centre += p;
                    centre /= list.Count;
                    collider.shape = JiggleColliderShape.Sphere;
                    collider.center = centre;
                    collider.radius = Percentile(list.Select(p => (p - centre).magnitude), options.percentile) * options.margin;
                }
                else
                {
                    var end = bone.InverseTransformPoint(next.position);
                    var direction = end.normalized;
                    collider.shape = JiggleColliderShape.Capsule;
                    collider.axis = Mathf.Abs(direction.x) >= Mathf.Abs(direction.y) && Mathf.Abs(direction.x) >= Mathf.Abs(direction.z) ? JiggleColliderAxis.X
                        : Mathf.Abs(direction.y) >= Mathf.Abs(direction.z) ? JiggleColliderAxis.Y : JiggleColliderAxis.Z;
                    collider.center = end * 0.5f;
                    collider.radius = Percentile(list.Select(p => DistanceToSegment(p, Vector3.zero, end)), options.percentile) * options.margin;
                    collider.height = end.magnitude + collider.radius;
                }
                added++;
            }
            return added;
        }

        /// <summary>The vertices of the body meshes by the bone that moves them most (at least half), in the space of that bone in the bind pose of the mesh.</summary>
        private static Dictionary<Transform, List<Vector3>> CollectBonePoints(GameObject root, JiggleSetupOptions options)
        {
            var renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(r => r.sharedMesh != null).ToList();
            var words = (options.bodyMeshFilter ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim()).Where(w => w.Length > 0).ToList();
            var body = words.Count == 0 ? new List<SkinnedMeshRenderer>() : renderers.Where(r => words.Any(w => r.name.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
            if (body.Count > 0) renderers = body;

            var result = new Dictionary<Transform, List<Vector3>>();
            foreach (var renderer in renderers)
            {
                var mesh = renderer.sharedMesh;
                var vertices = mesh.vertices;
                var weights = mesh.boneWeights;
                var bindposes = mesh.bindposes;
                var bones = renderer.bones;
                for (int i = 0; i < vertices.Length && i < weights.Length; i++)
                {
                    var w = weights[i];
                    AddPoint(result, bones, bindposes, w.boneIndex0, w.weight0, vertices[i]);
                    AddPoint(result, bones, bindposes, w.boneIndex1, w.weight1, vertices[i]);
                }
            }
            return result;
        }

        private static void AddPoint(Dictionary<Transform, List<Vector3>> result, Transform[] bones, Matrix4x4[] bindposes, int index, float weight, Vector3 vertex)
        {
            if (weight < 0.5f || index < 0 || index >= bones.Length || index >= bindposes.Length || bones[index] == null) return;
            if (!result.TryGetValue(bones[index], out var list)) result[bones[index]] = list = new List<Vector3>();
            list.Add(bindposes[index].MultiplyPoint3x4(vertex));
        }

        private static float DistanceToSegment(Vector3 point, Vector3 a, Vector3 b)
        {
            var axis = b - a;
            float lengthSquared = axis.sqrMagnitude;
            float t = lengthSquared < 1e-12f ? 0f : Mathf.Clamp01(Vector3.Dot(point - a, axis) / lengthSquared);
            return Vector3.Distance(point, a + axis * t);
        }

        private static float Percentile(IEnumerable<float> values, float share)
        {
            var sorted = values.OrderBy(v => v).ToList();
            if (sorted.Count == 0) return 0f;
            return sorted[Mathf.Clamp((int)(sorted.Count * Mathf.Clamp01(share)), 0, sorted.Count - 1)];
        }
    }
}
