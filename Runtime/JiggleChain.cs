using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork
{
    /// <summary>Where the tail of a <see cref="JiggleChain"/> points: the last bone has no child, so the Toe engine puts a particle after it.</summary>
    public enum JiggleTailDirection
    {
        /// <summary>Along the last segment of the chain (a chain of one bone: away from its parent).</summary>
        Extend,

        /// <summary>Down in the rest pose (world -Y), however the bone is turned.</summary>
        Down,

        /// <summary>From the head of the bone's parent to the head of the bone (breasts, a bun).</summary>
        AwayFromParent,
    }

    /// <summary>
    /// A chain of bones that jiggles in the Toe engine (Toe.JiggleBones): a skirt, hair, a tail, breasts. Put it on the <b>root bone</b>: the chain is this bone and the bones below it
    /// that follow it one by one (each the only child of the one before; a bone with several children ends the chain, with a warning). The root bone stays where the animation has it;
    /// the others lag behind the body, hang under gravity and swing back, and keep out of the <see cref="JiggleCollider"/>s of the character. With the Toe export target the chain
    /// goes into the <c>jiggle</c> section of the character manifest (<c>Characters/&lt;name&gt;.json</c>, written by the character export); the Rbfx target leaves it out.
    /// Put the components on a prefab (a variant of the character's model prefab) so that they are kept.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Toe/Jiggle Chain")]
    public sealed class JiggleChain : MonoBehaviour
    {
        [Range(0f, 1f)]
        [Tooltip("How hard the bones are pulled back to the pose of the animation: 0 is floppy, 1 is rigid (the share of the way back per 1/120 s).")]
        public float stiffness = 0.3f;

        [Range(0f, 1f)]
        [Tooltip("How much of the speed is lost (per 1/120 s).")]
        public float damping = 0.15f;

        [Tooltip("How strongly gravity pulls, as a multiple of the gravity of the character's Jiggle Animator.")]
        public float gravity = 1f;

        [Tooltip("The thickness of the chain in metres: the bones keep this far from the colliders.")]
        public float radius = 0.03f;

        [Range(0f, 180f)]
        [Tooltip("The most a segment may turn away from the pose of the animation, in degrees.")]
        public float maxAngle = 60f;

        [Tooltip("The length of the tail after the last bone in metres. 0: the length of the last segment (0.1 m for a chain of one bone).")]
        public float tailLength;

        [Tooltip("Where the tail points.")]
        public JiggleTailDirection tailDirection = JiggleTailDirection.Extend;

        /// <summary>
        /// The bones of the chain from this one down: each the only child of the one before. <paramref name="branched"/> is true when a bone has several children (the chain
        /// ends there).
        /// </summary>
        public void CollectBones(List<Transform> result, out bool branched)
        {
            branched = false;
            var bone = transform;
            while (bone != null)
            {
                result.Add(bone);
                if (bone.childCount == 0) return;
                if (bone.childCount > 1)
                {
                    branched = true;
                    return;
                }
                bone = bone.GetChild(0);
            }
        }

        private static readonly Color Dim = new Color(1f, 0.85f, 0.2f, 0.4f);

        private void OnDrawGizmos() => Draw(Dim);

        private void OnDrawGizmosSelected() => Draw(new Color(1f, 0.85f, 0.2f, 1f));

        private void Draw(Color color)
        {
            var bones = new List<Transform>();
            CollectBones(bones, out _);
            Gizmos.color = color;
            for (int i = 0; i < bones.Count; i++)
            {
                Gizmos.DrawWireSphere(bones[i].position, Mathf.Max(radius, 0.002f));
                if (i > 0) Gizmos.DrawLine(bones[i - 1].position, bones[i].position);
            }

            // The tail, as the engine will put it.
            var last = bones[bones.Count - 1];
            Vector3 direction;
            switch (tailDirection)
            {
                case JiggleTailDirection.Down:
                    direction = Vector3.down;
                    break;
                case JiggleTailDirection.AwayFromParent:
                    direction = last.parent != null ? last.position - last.parent.position : Vector3.down;
                    break;
                default:
                    direction = bones.Count > 1 ? last.position - bones[bones.Count - 2].position : (last.parent != null ? last.position - last.parent.position : Vector3.down);
                    break;
            }
            if (direction.sqrMagnitude < 1e-10f) direction = Vector3.down;
            float length = tailLength > 0f ? tailLength : (bones.Count > 1 ? Vector3.Distance(bones[bones.Count - 1].position, bones[bones.Count - 2].position) : 0.1f);
            Gizmos.color = new Color(1f, 0.3f, 0.2f, color.a);
            Gizmos.DrawLine(last.position, last.position + direction.normalized * length);
            Gizmos.DrawWireSphere(last.position + direction.normalized * length, Mathf.Max(radius, 0.002f));
        }
    }
}
