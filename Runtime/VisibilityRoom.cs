using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork
{
    /// <summary>
    /// A room for the Toe engine's portal culling: the union of this object's own box and the <see cref="VisibilityRoomBox"/>es below it (so an
    /// L-shaped hall is one room). With the Toe export target it becomes a <c>VisibilityRoom</c> component: from a camera in it, the engine draws only
    /// the rooms it sees through open <see cref="VisibilityPortal"/>s, and drops the outdoors. <b>Rooms must be closed</b> (walls, floor and roof):
    /// whatever the camera can see over or through a gap in a wall is dropped as well. Where rooms overlap, the higher <see cref="priority"/> holds a
    /// point, then the smaller room. The boxes are in this object's space and follow its scale and rotation. Ignored by the Rbfx target.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Toe/Visibility Room")]
    public sealed class VisibilityRoom : MonoBehaviour
    {
        [Tooltip("Center of the room's own box, in this object's space.")]
        public Vector3 center = new Vector3(0f, 1.5f, 0f);

        [Tooltip("Size of the room's own box, in this object's space. All zero: no own box (the room is made of its child Visibility Room Boxes).")]
        public Vector3 size = new Vector3(4f, 3f, 4f);

        [Tooltip("Where rooms overlap, the room with the higher priority holds a point (then the smaller one).")]
        public int priority;

        /// <summary>The boxes of this object's children that belong to this room (their nearest room parent), in the order of the hierarchy.</summary>
        public void CollectChildBoxes(List<VisibilityRoomBox> result)
        {
            var found = GetComponentsInChildren<VisibilityRoomBox>(true);
            foreach (var box in found)
            {
                if (box != null && box.enabled && ReferenceEquals(box.Room, this)) result.Add(box);
            }
        }

        private static bool IsEmpty(Vector3 size) => size.x <= 0f || size.y <= 0f || size.z <= 0f;

        /// <summary>
        /// The room's boxes in the world, the way the engine will hold them. <paramref name="empty"/> counts the boxes of no size, which are left out
        /// (an own box of size zero in every axis is simply no own box).
        /// </summary>
        public void CollectWorldBoxes(List<OrientedBox> result, out int empty)
        {
            empty = 0;
            if (size != Vector3.zero)
            {
                if (IsEmpty(size)) empty++;
                else result.Add(VisibilityMath.WorldBox(VisibilityFrame.Of(transform), center, size));
            }

            var children = new List<VisibilityRoomBox>();
            CollectChildBoxes(children);
            foreach (var box in children)
            {
                if (IsEmpty(box.size)) empty++;
                else result.Add(VisibilityMath.WorldBox(VisibilityFrame.Of(box.transform), box.center, box.size));
            }
        }

        /// <summary>
        /// The room's boxes as the engine stores them, in this object's space before its scale (what the exporter writes). <paramref name="inexact"/>
        /// counts the boxes that the engine cannot hold exactly: a turned box under a room scaled differently along its axes.
        /// </summary>
        public void CollectExportBoxes(List<RoomBoxData> result, out int empty, out int inexact)
        {
            empty = 0;
            inexact = 0;
            if (size != Vector3.zero)
            {
                if (IsEmpty(size)) empty++;
                else result.Add(new RoomBoxData(center, size, Quaternion.identity));
            }

            var room = VisibilityFrame.Of(transform);
            var children = new List<VisibilityRoomBox>();
            CollectChildBoxes(children);
            foreach (var box in children)
            {
                if (IsEmpty(box.size))
                {
                    empty++;
                    continue;
                }
                if (!VisibilityMath.ToRoomSpace(room, VisibilityFrame.Of(box.transform), box.center, box.size, out var data)) inexact++;
                result.Add(data);
            }
        }

        private static readonly Color Dim = new Color(1f, 1f, 1f, 0.35f);

        private void OnDrawGizmos() => DrawBoxes(Dim);

        private void OnDrawGizmosSelected() => DrawBoxes(Color.white);

        private void DrawBoxes(Color tint)
        {
            var boxes = new List<OrientedBox>();
            CollectWorldBoxes(boxes, out _);
            var color = VisibilityScene.ColorOf(this) * tint;
            var saved = Gizmos.matrix;
            Gizmos.color = color;
            foreach (var box in boxes)
            {
                Gizmos.matrix = Matrix4x4.TRS(box.Center, Quaternion.LookRotation(box.AxisZ, box.AxisY), Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, box.HalfSize * 2f);
            }
            Gizmos.matrix = saved;
        }
    }
}
