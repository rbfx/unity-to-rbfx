using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork
{
    /// <summary>
    /// A position, rotation and (lossy) scale: what the room code needs of a Transform. Only managed Unity calls are used (no Transform methods,
    /// no Quaternion.Inverse), so the math below can be run and tested outside the editor.
    /// </summary>
    public readonly struct VisibilityFrame
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 Scale;

        public VisibilityFrame(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public static VisibilityFrame Of(Transform transform) => new VisibilityFrame(transform.position, transform.rotation, transform.lossyScale);

        /// <summary>A point of the frame's own space, in the world.</summary>
        public Vector3 TransformPoint(Vector3 local) => Position + Rotation * Vector3.Scale(Scale, local);

        /// <summary>A world point in the frame's own space.</summary>
        public Vector3 InverseTransformPoint(Vector3 world)
        {
            var d = VisibilityMath.Conjugate(Rotation) * (world - Position);
            return new Vector3(VisibilityMath.Divide(d.x, Scale.x), VisibilityMath.Divide(d.y, Scale.y), VisibilityMath.Divide(d.z, Scale.z));
        }
    }

    /// <summary>A box with any orientation: the same shape the Toe engine uses (center, half the size along each of three unit axes).</summary>
    public readonly struct OrientedBox
    {
        public readonly Vector3 Center;
        public readonly Vector3 HalfSize;
        public readonly Vector3 AxisX;
        public readonly Vector3 AxisY;
        public readonly Vector3 AxisZ;

        public OrientedBox(Vector3 center, Vector3 halfSize, Vector3 axisX, Vector3 axisY, Vector3 axisZ)
        {
            Center = center;
            HalfSize = halfSize;
            AxisX = axisX;
            AxisY = axisY;
            AxisZ = axisZ;
        }

        public float Volume => 8f * HalfSize.x * HalfSize.y * HalfSize.z;

        /// <summary>True if the point is inside the box or on its surface (within float rounding, which grows with the coordinates).</summary>
        public bool Contains(Vector3 point)
        {
            var d = point - Center;
            var l = new Vector3(Vector3.Dot(d, AxisX), Vector3.Dot(d, AxisY), Vector3.Dot(d, AxisZ));
            float slack = 2e-6f * (1f + VisibilityMath.MaxAbs(Center) + VisibilityMath.MaxAbs(point));
            return Mathf.Abs(l.x) <= HalfSize.x + slack && Mathf.Abs(l.y) <= HalfSize.y + slack && Mathf.Abs(l.z) <= HalfSize.z + slack;
        }

        /// <summary>The same box a little smaller on every side; rooms that only meet at a wall do not overlap once shrunk.</summary>
        public OrientedBox Shrunk(float amount)
            => new OrientedBox(Center, new Vector3(Mathf.Max(HalfSize.x - amount, 0f), Mathf.Max(HalfSize.y - amount, 0f), Mathf.Max(HalfSize.z - amount, 0f)), AxisX, AxisY, AxisZ);

        /// <summary>True if the boxes share any space (touching counts): the separating axis test over the 15 candidate axes (a port of Toe's OrientedBoundingBox.Overlaps).</summary>
        public bool Overlaps(OrientedBox other)
        {
            const float parallelEpsilon = 1e-6f;
            var axesA = new[] { AxisX, AxisY, AxisZ };
            var axesB = new[] { other.AxisX, other.AxisY, other.AxisZ };
            var a = new[] { HalfSize.x, HalfSize.y, HalfSize.z };
            var b = new[] { other.HalfSize.x, other.HalfSize.y, other.HalfSize.z };
            var r = new float[9];
            var absR = new float[9];
            var t = new float[3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    float d = Vector3.Dot(axesA[i], axesB[j]);
                    r[i * 3 + j] = d;
                    absR[i * 3 + j] = Mathf.Abs(d) + parallelEpsilon;
                }
            }

            var offset = other.Center - Center;
            for (int i = 0; i < 3; i++) t[i] = Vector3.Dot(offset, axesA[i]);

            for (int i = 0; i < 3; i++)
            {
                float rb = b[0] * absR[i * 3] + b[1] * absR[i * 3 + 1] + b[2] * absR[i * 3 + 2];
                if (Mathf.Abs(t[i]) > a[i] + rb) return false;
            }

            for (int j = 0; j < 3; j++)
            {
                float ra = a[0] * absR[j] + a[1] * absR[3 + j] + a[2] * absR[6 + j];
                float distance = t[0] * r[j] + t[1] * r[3 + j] + t[2] * r[6 + j];
                if (Mathf.Abs(distance) > ra + b[j]) return false;
            }

            for (int i = 0; i < 3; i++)
            {
                int i1 = (i + 1) % 3, i2 = (i + 2) % 3;
                for (int j = 0; j < 3; j++)
                {
                    int j1 = (j + 1) % 3, j2 = (j + 2) % 3;
                    float ra = a[i1] * absR[i2 * 3 + j] + a[i2] * absR[i1 * 3 + j];
                    float rb = b[j1] * absR[i * 3 + j2] + b[j2] * absR[i * 3 + j1];
                    float distance = t[i2] * r[i1 * 3 + j] - t[i1] * r[i2 * 3 + j];
                    if (Mathf.Abs(distance) > ra + rb) return false;
                }
            }

            return true;
        }
    }

    /// <summary>One box of a room as the Toe engine stores it: in the room node's space, before the node's scale (center, full size, rotation).</summary>
    public readonly struct RoomBoxData
    {
        public readonly Vector3 Center;
        public readonly Vector3 Size;
        public readonly Quaternion Rotation;

        public RoomBoxData(Vector3 center, Vector3 size, Quaternion rotation)
        {
            Center = center;
            Size = size;
            Rotation = rotation;
        }
    }

    /// <summary>A room as the engine sees it: its boxes in the world, its priority and its total volume.</summary>
    public sealed class RoomShape
    {
        public VisibilityRoom Source;
        public string Name;
        public int Priority;
        public readonly List<OrientedBox> Boxes = new List<OrientedBox>();

        /// <summary>Boxes of no size, which are left out (a room with only those is no room).</summary>
        public int EmptyBoxes;

        public float Volume
        {
            get
            {
                float volume = 0f;
                foreach (var box in Boxes) volume += box.Volume;
                return volume;
            }
        }

        public bool Contains(Vector3 point)
        {
            foreach (var box in Boxes)
                if (box.Contains(point)) return true;
            return false;
        }

        public bool Overlaps(RoomShape other, float shrink)
        {
            foreach (var a in Boxes)
                foreach (var b in other.Boxes)
                    if (a.Shrunk(shrink).Overlaps(b.Shrunk(shrink))) return true;
            return false;
        }
    }

    /// <summary>The two sides of a portal: the points a little behind and in front of its center, along its normal (+Z of its object).</summary>
    public readonly struct PortalSides
    {
        public readonly Vector3 Center;
        public readonly Vector3 Normal;
        public readonly Vector3 Behind;
        public readonly Vector3 Front;

        public PortalSides(Vector3 center, Vector3 normal, Vector3 behind, Vector3 front)
        {
            Center = center;
            Normal = normal;
            Behind = behind;
            Front = front;
        }
    }

    /// <summary>The rules the Toe engine applies to rooms and portals, so that what a designer sees in the editor is what the engine will find.</summary>
    public static class VisibilityMath
    {
        /// <summary>How far from a portal's center, along its normal, the points are that find the rooms on its two sides (Toe's Portal.SideOffset default).</summary>
        public const float DefaultSideOffset = 0.05f;

        /// <summary>Rooms that overlap by less than this at a wall count as touching, not overlapping.</summary>
        public const float WallTolerance = 0.005f;

        internal static float MaxAbs(Vector3 v) => Mathf.Max(Mathf.Abs(v.x), Mathf.Max(Mathf.Abs(v.y), Mathf.Abs(v.z)));

        internal static float Divide(float value, float by) => Mathf.Abs(by) > 1e-6f ? value / by : 0f;

        /// <summary>The inverse of a unit quaternion.</summary>
        internal static Quaternion Conjugate(Quaternion q) => new Quaternion(-q.x, -q.y, -q.z, q.w);

        private static Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

        /// <summary>A box of the frame's space (center and full size) in the world. The frame's scale scales the box along its own axes.</summary>
        public static OrientedBox WorldBox(in VisibilityFrame frame, Vector3 center, Vector3 size)
        {
            var half = Vector3.Scale(size, Abs(frame.Scale)) * 0.5f;
            return new OrientedBox(frame.TransformPoint(center), half, frame.Rotation * Vector3.right, frame.Rotation * Vector3.up, frame.Rotation * Vector3.forward);
        }

        /// <summary>
        /// A box of the <paramref name="box"/> frame as Toe stores a room's box: in the <paramref name="room"/> node's space, before the node's scale
        /// (the engine scales a box by its node). Exact for a uniformly scaled room, and for a box whose axes lie along the room's axes; for another box
        /// under a room scaled differently along its axes the engine cannot hold the shear, so the result is the unrotated bounds and false is returned.
        /// </summary>
        public static bool ToRoomSpace(in VisibilityFrame room, in VisibilityFrame box, Vector3 center, Vector3 size, out RoomBoxData result)
        {
            var relative = Conjugate(room.Rotation) * box.Rotation;
            var localCenter = room.InverseTransformPoint(box.TransformPoint(center));
            var half = Vector3.Scale(size, Abs(box.Scale)) * 0.5f;
            var scale = Abs(room.Scale);

            if (IsUniform(scale))
            {
                result = new RoomBoxData(localCenter, Vector3.Scale(size, Abs(box.Scale)) / Mathf.Max(scale.x, 1e-6f), relative);
                return true;
            }

            var axes = new[] { relative * Vector3.right, relative * Vector3.up, relative * Vector3.forward };
            var local = Vector3.zero;
            bool aligned = true;
            for (int i = 0; i < 3 && aligned; i++)
            {
                int along = -1;
                for (int j = 0; j < 3; j++)
                    if (Mathf.Abs(axes[i][j]) > 0.999f) along = j;
                if (along < 0) aligned = false;
                else local[i] = Divide(2f * half[i], scale[along]);
            }

            if (aligned)
            {
                result = new RoomBoxData(localCenter, local, relative);
                return true;
            }

            // The room-space bounds of the box, then the room's scale taken out per axis.
            var extent = Abs(axes[0]) * half.x + Abs(axes[1]) * half.y + Abs(axes[2]) * half.z;
            result = new RoomBoxData(localCenter, new Vector3(Divide(2f * extent.x, scale.x), Divide(2f * extent.y, scale.y), Divide(2f * extent.z, scale.z)), Quaternion.identity);
            return false;
        }

        private static bool IsUniform(Vector3 scale)
        {
            float max = Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
            float min = Mathf.Min(scale.x, Mathf.Min(scale.y, scale.z));
            return max - min <= 1e-3f * Mathf.Max(max, 1e-6f);
        }

        /// <summary>
        /// The room that holds the point: the higher priority wins, then the smaller room (total volume), then the earlier one in the list; null for none (the
        /// outside). The rule of the engine's RoomGraph.FindRoom.
        /// </summary>
        public static RoomShape FindRoom(IReadOnlyList<RoomShape> rooms, Vector3 point)
        {
            RoomShape best = null;
            float bestVolume = 0f;
            foreach (var room in rooms)
            {
                if (!room.Contains(point)) continue;
                float volume = room.Volume;
                if (best == null || room.Priority > best.Priority || (room.Priority == best.Priority && volume < bestVolume))
                {
                    best = room;
                    bestVolume = volume;
                }
            }
            return best;
        }

        /// <summary>The points on the two sides of a portal object: its position, and its +Z as the normal.</summary>
        public static PortalSides SidesOf(in VisibilityFrame portal, float offset = DefaultSideOffset)
        {
            var normal = (portal.Rotation * Vector3.forward).normalized;
            return new PortalSides(portal.Position, normal, portal.Position - normal * offset, portal.Position + normal * offset);
        }

        /// <summary>The room behind and the room in front of a portal (null for the outside).</summary>
        public static void RoomsOf(IReadOnlyList<RoomShape> rooms, in PortalSides sides, out RoomShape behind, out RoomShape front)
        {
            behind = FindRoom(rooms, sides.Behind);
            front = FindRoom(rooms, sides.Front);
        }

        /// <summary>
        /// What is wrong with the rooms and portals of a scene, as lines of text: the same checks as the engine's <c>rooms check</c> (rooms of the same
        /// priority and size that overlap, a portal with no room on either side or the same room on both, a room with no box of any size).
        /// </summary>
        public static List<string> Check(IReadOnlyList<RoomShape> rooms, IReadOnlyList<KeyValuePair<string, PortalSides>> portals)
        {
            var warnings = new List<string>();
            foreach (var room in rooms)
            {
                if (room.Boxes.Count == 0) warnings.Add($"Room '{room.Name}' has no box of any size.");
                else if (room.EmptyBoxes > 0) warnings.Add($"Room '{room.Name}' has {room.EmptyBoxes} box(es) of no size, which are left out.");
            }

            for (int a = 0; a < rooms.Count; a++)
            {
                for (int b = a + 1; b < rooms.Count; b++)
                {
                    var first = rooms[a];
                    var second = rooms[b];
                    float volume = first.Volume;
                    if (first.Boxes.Count == 0 || second.Boxes.Count == 0 || first.Priority != second.Priority) continue;
                    if (Mathf.Abs(volume - second.Volume) > 1e-3f * Mathf.Max(1f, volume)) continue;
                    if (first.Overlaps(second, WallTolerance))
                        warnings.Add($"Rooms '{first.Name}' and '{second.Name}' overlap with the same priority and size.");
                }
            }

            foreach (var entry in portals)
            {
                RoomsOf(rooms, entry.Value, out var behind, out var front);
                if (behind == null && front == null) warnings.Add($"Portal '{entry.Key}' has no room on either side.");
                else if (ReferenceEquals(behind, front)) warnings.Add($"Portal '{entry.Key}' has the same room ('{behind.Name}') on both sides and is ignored.");
            }
            return warnings;
        }
    }
}
