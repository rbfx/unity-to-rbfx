using UnityEngine;

namespace UnityToRebelFork
{
    /// <summary>Body type of a <see cref="ExportBody"/>.</summary>
    public enum ExportBodyType
    {
        /// <summary>Static without a Rigidbody, dynamic or kinematic with one.</summary>
        Auto,
        Static,
        Kinematic,
        Dynamic,
    }

    /// <summary>
    /// Overrides the rigid body exported for this object's colliders: its type and collision layer and mask. Without it, colliders without a
    /// Rigidbody export as a static body on layer <c>1</c>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ExportBody : MonoBehaviour
    {
        public ExportBodyType bodyType = ExportBodyType.Auto;

        [Tooltip("Collision layer bits (0 keeps the default, 1).")]
        public int layer;

        [Tooltip("Collision mask bits (0 keeps the default, all).")]
        public int mask;
    }
}
