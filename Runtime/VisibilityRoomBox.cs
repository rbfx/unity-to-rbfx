using UnityEngine;

namespace UnityToRebelFork
{
    /// <summary>
    /// An extra box of the <see cref="VisibilityRoom"/> above this object in the hierarchy (the nearest one): put it on a child object, shape it
    /// with this object's transform, and the room becomes the union of its boxes. Exports nothing by itself.
    /// </summary>
    [AddComponentMenu("Toe/Visibility Room Box")]
    public sealed class VisibilityRoomBox : MonoBehaviour
    {
        [Tooltip("Center of the box, in this object's space.")]
        public Vector3 center;

        [Tooltip("Size of the box, in this object's space.")]
        public Vector3 size = new Vector3(2f, 2f, 2f);

        /// <summary>The room this box belongs to: the nearest <see cref="VisibilityRoom"/> on this object or above it; null if there is none.</summary>
        public VisibilityRoom Room => GetComponentInParent<VisibilityRoom>(true);

        private void OnDrawGizmos() => Draw(0.35f);

        private void OnDrawGizmosSelected() => Draw(1f);

        private void Draw(float alpha)
        {
            var room = Room;
            var color = room != null ? VisibilityScene.ColorOf(room) : Color.red;
            color.a *= alpha;
            var saved = Gizmos.matrix;
            Gizmos.color = color;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(center, size);
            Gizmos.matrix = saved;
        }
    }
}
