using UnityEngine;

namespace UnityToRebelFork
{
    public enum JiggleColliderShape
    {
        Sphere,
        Capsule,
    }

    public enum JiggleColliderAxis
    {
        X,
        Y,
        Z,
    }

    /// <summary>
    /// A sphere or capsule fixed to a bone that the <see cref="JiggleChain"/>s of the character do not enter (a thigh for a skirt, the head for hair, the chest for breasts). It follows
    /// the bone; the numbers are in the space of the bone. It is no Unity collider and no physics body: with the Toe export target it goes into the <c>jiggle</c> section of the
    /// character manifest, and the Rbfx target leaves it out.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Toe/Jiggle Collider")]
    public sealed class JiggleCollider : MonoBehaviour
    {
        public JiggleColliderShape shape = JiggleColliderShape.Capsule;

        [Tooltip("The middle of the shape, in the space of this bone.")]
        public Vector3 center;

        [Tooltip("The radius in metres (before the scale of the bone).")]
        public float radius = 0.05f;

        [Tooltip("The height of a capsule from tip to tip, with its two caps (before the scale of the bone).")]
        public float height = 0.3f;

        [Tooltip("The axis of a capsule, in the space of this bone.")]
        public JiggleColliderAxis axis = JiggleColliderAxis.Y;

        /// <summary>The axis of a capsule as a vector in the space of the bone.</summary>
        public static Vector3 AxisVector(JiggleColliderAxis axis)
        {
            switch (axis)
            {
                case JiggleColliderAxis.X: return Vector3.right;
                case JiggleColliderAxis.Z: return Vector3.forward;
                default: return Vector3.up;
            }
        }

        private static readonly Color Dim = new Color(0.2f, 1f, 0.3f, 0.35f);

        private void OnDrawGizmos() => Draw(Dim);

        private void OnDrawGizmosSelected() => Draw(new Color(0.2f, 1f, 0.3f, 1f));

        private void Draw(Color color)
        {
            Gizmos.color = color;
            var world = transform.localToWorldMatrix;
            if (shape == JiggleColliderShape.Sphere)
            {
                Gizmos.DrawWireSphere(world.MultiplyPoint3x4(center), radius * MaxScale(world));
                return;
            }

            var axisLocal = AxisVector(axis);
            float half = Mathf.Max(height * 0.5f - radius, 0f);
            var a = world.MultiplyPoint3x4(center - axisLocal * half);
            var b = world.MultiplyPoint3x4(center + axisLocal * half);
            float r = radius * MaxScale(world);
            Gizmos.DrawWireSphere(a, r);
            Gizmos.DrawWireSphere(b, r);
            var side = Vector3.Cross((b - a).normalized, Vector3.up);
            if (side.sqrMagnitude < 1e-6f) side = Vector3.right;
            side = side.normalized * r;
            var other = Vector3.Cross(side, (b - a).normalized).normalized * r;
            Gizmos.DrawLine(a + side, b + side);
            Gizmos.DrawLine(a - side, b - side);
            Gizmos.DrawLine(a + other, b + other);
            Gizmos.DrawLine(a - other, b - other);
        }

        private static float MaxScale(Matrix4x4 m)
            => Mathf.Max(m.GetColumn(0).magnitude, Mathf.Max(m.GetColumn(1).magnitude, m.GetColumn(2).magnitude));
    }
}
