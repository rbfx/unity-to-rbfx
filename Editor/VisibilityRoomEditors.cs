using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    /// <summary>Menu items that make rooms, room boxes and portals (Toe's portal culling; see <see cref="VisibilityRoom"/>).</summary>
    public static class VisibilityMenu
    {
        [MenuItem("GameObject/Toe/Visibility Room", false, 20)]
        private static void CreateRoom(MenuCommand command) => Create(command, "Room", go => go.AddComponent<VisibilityRoom>());

        [MenuItem("GameObject/Toe/Visibility Room Box", false, 21)]
        private static void CreateRoomBox(MenuCommand command)
        {
            var parent = command.context as GameObject;
            if (parent == null || parent.GetComponentInParent<VisibilityRoom>(true) == null)
                Debug.LogWarning("[Rooms] A Visibility Room Box belongs to the Visibility Room above it: make it a child of a room.");
            Create(command, "Room Box", go => go.AddComponent<VisibilityRoomBox>());
        }

        [MenuItem("GameObject/Toe/Visibility Portal", false, 22)]
        private static void CreatePortal(MenuCommand command) => Create(command, "Portal", go => go.AddComponent<VisibilityPortal>());

        private static void Create(MenuCommand command, string name, System.Action<GameObject> addComponent)
        {
            var go = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);
            addComponent(go);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            Selection.activeObject = go;
        }
    }

    /// <summary>Resizes a room's own box in the scene view.</summary>
    [CustomEditor(typeof(VisibilityRoom))]
    public sealed class VisibilityRoomEditor : UnityEditor.Editor
    {
        private readonly BoxBoundsHandle _handle = new BoxBoundsHandle();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var room = (VisibilityRoom)target;

            if (GUILayout.Button("Fit to child renderers")) Fit(room);

            var boxes = new List<OrientedBox>();
            room.CollectWorldBoxes(boxes, out int empty);
            var text = $"{boxes.Count} box(es), {(boxes.Count > 0 ? Volume(boxes).ToString("0.#") : "0")} m3.";
            if (empty > 0) text += $" {empty} box(es) of no size are left out.";
            EditorGUILayout.HelpBox(text + " Rooms must be closed (walls, floor, roof): what the camera sees through a gap is dropped.", empty > 0 || boxes.Count == 0 ? MessageType.Warning : MessageType.None);
        }

        private static float Volume(List<OrientedBox> boxes)
        {
            float volume = 0f;
            foreach (var box in boxes) volume += box.Volume;
            return volume;
        }

        /// <summary>Sets the room's own box to the bounds of the renderers below it, in the room's space.</summary>
        public static void Fit(VisibilityRoom room)
        {
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            bool any = false;
            foreach (var renderer in room.GetComponentsInChildren<UnityEngine.Renderer>(false))
            {
                if (renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer) continue;
                var b = renderer.bounds;
                for (int i = 0; i < 8; i++)
                {
                    var corner = new Vector3((i & 1) != 0 ? b.max.x : b.min.x, (i & 2) != 0 ? b.max.y : b.min.y, (i & 4) != 0 ? b.max.z : b.min.z);
                    var local = room.transform.InverseTransformPoint(corner);
                    min = Vector3.Min(min, local);
                    max = Vector3.Max(max, local);
                    any = true;
                }
            }

            if (!any)
            {
                Debug.LogWarning($"[Rooms] '{room.name}' has no renderers below it to fit to.", room);
                return;
            }
            Undo.RecordObject(room, "Fit Visibility Room");
            room.center = (min + max) * 0.5f;
            room.size = max - min;
        }

        private void OnSceneGUI()
        {
            var room = (VisibilityRoom)target;
            if (room.size == Vector3.zero) return;
            using (new Handles.DrawingScope(room.transform.localToWorldMatrix))
            {
                _handle.center = room.center;
                _handle.size = room.size;
                EditorGUI.BeginChangeCheck();
                _handle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(room, "Resize Visibility Room");
                    room.center = _handle.center;
                    room.size = _handle.size;
                }
            }
        }
    }

    /// <summary>Resizes a room box in the scene view and warns when it has no room above it.</summary>
    [CustomEditor(typeof(VisibilityRoomBox))]
    public sealed class VisibilityRoomBoxEditor : UnityEditor.Editor
    {
        private readonly BoxBoundsHandle _handle = new BoxBoundsHandle();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var room = ((VisibilityRoomBox)target).Room;
            if (room == null) EditorGUILayout.HelpBox("No Visibility Room above this object: the box belongs to no room.", MessageType.Warning);
            else EditorGUILayout.HelpBox($"A box of room '{room.name}'.", MessageType.None);
        }

        private void OnSceneGUI()
        {
            var box = (VisibilityRoomBox)target;
            using (new Handles.DrawingScope(box.transform.localToWorldMatrix))
            {
                _handle.center = box.center;
                _handle.size = box.size;
                EditorGUI.BeginChangeCheck();
                _handle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(box, "Resize Visibility Room Box");
                    box.center = _handle.center;
                    box.size = _handle.size;
                }
            }
        }
    }

    /// <summary>Resizes a portal's rectangle in the scene view and says which rooms it finds on its two sides.</summary>
    [CustomEditor(typeof(VisibilityPortal))]
    public sealed class VisibilityPortalEditor : UnityEditor.Editor
    {
        private readonly BoxBoundsHandle _handle = new BoxBoundsHandle { axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y };

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var portal = (VisibilityPortal)target;
            var sides = VisibilityMath.SidesOf(VisibilityFrame.Of(portal.transform));
            VisibilityMath.RoomsOf(VisibilityScene.EnabledRooms(), sides, out var behind, out var front);
            string Name(RoomShape room) => room == null ? "outside" : $"'{room.Name}'";
            bool broken = (behind == null && front == null) || ReferenceEquals(behind, front);
            EditorGUILayout.HelpBox($"Behind (-Z): {Name(behind)}. In front (+Z): {Name(front)}."
                + (broken ? " A portal needs two different sides: it is ignored." : string.Empty), broken ? MessageType.Warning : MessageType.None);
        }

        private void OnSceneGUI()
        {
            var portal = (VisibilityPortal)target;
            using (new Handles.DrawingScope(portal.transform.localToWorldMatrix))
            {
                _handle.center = Vector3.zero;
                _handle.size = new Vector3(portal.size.x, portal.size.y, 0f);
                EditorGUI.BeginChangeCheck();
                _handle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(portal, "Resize Visibility Portal");
                    portal.size = new Vector2(_handle.size.x, _handle.size.y);
                }
            }
        }
    }
}
