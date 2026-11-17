using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork
{
    /// <summary>
    /// An opening between two <see cref="VisibilityRoom"/>s, or between a room and the outside, for the Toe engine's portal culling: a flat rectangle in
    /// this object's XY plane (<see cref="size"/>, scaled by the object) whose normal is the object's +Z (blue axis). The rooms are not assigned, they are
    /// found: the room that holds the point 5 cm behind the center and the one that holds the point 5 cm in front of it (no room: outside); the gizmo
    /// shows what was found, in red when both sides are the same room or none. Size it to the open part of a doorway, not the whole door frame.
    /// With the Toe export target it becomes a <c>Portal</c> component with a stable id, so game code can find the door and set <c>IsOpen</c> (a disabled
    /// component exports as closed). A closed portal shows nothing behind it: close it only when its door is fully shut and hides everything.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Toe/Visibility Portal")]
    public sealed class VisibilityPortal : MonoBehaviour
    {
        [Tooltip("Width and height of the opening, in this object's space (before its scale).")]
        public Vector2 size = new Vector2(1.5f, 2.2f);

        [Tooltip("Whether the portal starts open. Game code can change it at run time.")]
        public bool isOpen = true;

        /// <summary>The corners in the world, counter-clockwise seen from the front.</summary>
        public void GetWorldCorners(Vector3[] corners)
        {
            float hx = size.x * 0.5f, hy = size.y * 0.5f;
            corners[0] = transform.TransformPoint(new Vector3(-hx, -hy, 0f));
            corners[1] = transform.TransformPoint(new Vector3(hx, -hy, 0f));
            corners[2] = transform.TransformPoint(new Vector3(hx, hy, 0f));
            corners[3] = transform.TransformPoint(new Vector3(-hx, hy, 0f));
        }

        private void OnDrawGizmos() => Draw(0.6f);

        private void OnDrawGizmosSelected() => Draw(1f);

        private void Draw(float alpha)
        {
            var rooms = VisibilityScene.EnabledRooms();
            var sides = VisibilityMath.SidesOf(VisibilityFrame.Of(transform));
            VisibilityMath.RoomsOf(rooms, sides, out var behind, out var front);
            bool broken = (behind == null && front == null) || ReferenceEquals(behind, front);
            var color = broken ? Color.red : enabled && isOpen ? Color.green : Color.gray;
            color.a = alpha;
            Gizmos.color = color;

            var corners = new Vector3[4];
            GetWorldCorners(corners);
            for (int i = 0; i < 4; i++) Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
            Gizmos.DrawLine(sides.Center, sides.Center + sides.Normal * 0.6f);
            Gizmos.DrawLine(sides.Center + sides.Normal * 0.6f, sides.Center + sides.Normal * 0.45f + transform.up * 0.08f);
            Gizmos.DrawLine(sides.Center + sides.Normal * 0.6f, sides.Center + sides.Normal * 0.45f - transform.up * 0.08f);

            // A line to the room on each side (the outside has no center).
            var link = new Color(color.r, color.g, color.b, alpha * 0.5f);
            Gizmos.color = link;
            if (behind != null) Gizmos.DrawLine(sides.Center, VisibilityScene.CenterOf(behind));
            if (front != null) Gizmos.DrawLine(sides.Center, VisibilityScene.CenterOf(front));
        }
    }
}
