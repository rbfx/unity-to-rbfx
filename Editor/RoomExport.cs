using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// The Toe components that rooms and portals export as, built from plain values (no Unity objects), so that what is written can be tested
    /// outside the editor. The attribute names and the layout of <c>Boxes</c> are the Toe engine's (BuiltInReflection: VisibilityRoom, Portal).
    /// </summary>
    public static class RoomExport
    {
        /// <summary>Toe's <c>VisibilityRoom</c>: its priority, and its boxes as a variant list: the count, then center, size and rotation of each box.</summary>
        public static GenericComponent RoomComponent(int priority, IReadOnlyList<RoomBoxData> boxes)
        {
            var list = new VariantList { new Variant(boxes.Count) };
            foreach (var box in boxes)
            {
                list.Add(new Variant(box.Center));
                list.Add(new Variant(box.Size));
                list.Add(new Variant(box.Rotation));
            }
            return new GenericComponent("VisibilityRoom")
                .Set("Priority", new Variant(priority))
                .Set("Boxes", new Variant(list));
        }

        /// <summary>Toe's <c>Portal</c>: the opening's width and height, and whether it starts open.</summary>
        public static GenericComponent PortalComponent(Vector2 size, bool isOpen)
            => new GenericComponent("Portal")
                .Set("Size", new Variant(size))
                .Set("Is Open", new Variant(isOpen));

        /// <summary>The "StableId" component that lets game code find a node again.</summary>
        public static GenericComponent StableIdComponent(string id)
            => new GenericComponent("StableId").Set("Id", new Variant(VariantType.VarString, id));
    }
}
