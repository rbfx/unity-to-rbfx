using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork
{
    /// <summary>The rooms of the loaded scenes as <see cref="RoomShape"/>s, for the gizmos and the export checks.</summary>
    public static class VisibilityScene
    {
        private static List<RoomShape> _cache;
        private static int _cachedFrame = -1;

        /// <summary>One room as its shape: the boxes in the world, the priority and the name.</summary>
        public static RoomShape ShapeOf(VisibilityRoom room)
        {
            var shape = new RoomShape { Source = room, Name = room.name, Priority = room.priority };
            room.CollectWorldBoxes(shape.Boxes, out shape.EmptyBoxes);
            return shape;
        }

        /// <summary>The shapes of the enabled rooms among <paramref name="rooms"/>.</summary>
        public static List<RoomShape> ShapesOf(IEnumerable<VisibilityRoom> rooms)
        {
            var result = new List<RoomShape>();
            foreach (var room in rooms)
                if (room != null && room.isActiveAndEnabled) result.Add(ShapeOf(room));
            return result;
        }

        /// <summary>The enabled rooms of all loaded scenes, kept for the frame (every gizmo of a frame asks).</summary>
        public static List<RoomShape> EnabledRooms()
        {
            int frame = Time.frameCount;
            if (_cache != null && _cachedFrame == frame) return _cache;
#if UNITY_2022_2_OR_NEWER
            var rooms = Object.FindObjectsByType<VisibilityRoom>(FindObjectsSortMode.None);
#else
            var rooms = Object.FindObjectsOfType<VisibilityRoom>();
#endif
            _cache = ShapesOf(rooms);
            _cachedFrame = frame;
            return _cache;
        }

        /// <summary>A color of its own for a room, stable between draws.</summary>
        public static Color ColorOf(VisibilityRoom room)
        {
            float hue = ((room.GetInstanceID() & 0xffff) * 0.381966f) % 1f;
            return Color.HSVToRGB(hue, 0.55f, 1f);
        }

        /// <summary>A point inside a room to draw a link to: the middle of its first box.</summary>
        public static Vector3 CenterOf(RoomShape room) => room.Boxes.Count > 0 ? room.Boxes[0].Center : room.Source.transform.position;
    }
}
