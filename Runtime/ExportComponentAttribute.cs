using System;
using UnityEngine;

namespace UnityToRebelFork
{
    /// <summary>
    /// Exports a MonoBehaviour as a scene component of the given type: each public (or [SerializeField]) field becomes an attribute of the
    /// same name, nicified ("openAngle" is "Open Angle") unless <see cref="ExportNameAttribute"/> names it. Fields of type bool, int, uint,
    /// float, string, enum (written as its name), Vector2/3/4, Quaternion and Color are exported; others are skipped.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ExportComponentAttribute : Attribute
    {
        public ExportComponentAttribute(string typeName)
        {
            TypeName = typeName;
        }

        /// <summary>The component's type name in the scene (as registered in the engine).</summary>
        public string TypeName { get; }

        /// <summary>
        /// Whether the node also gets a "StableId" component, with an id derived from the object's GlobalObjectId: the same on every export,
        /// different for every object (prefab instances included).
        /// </summary>
        public bool StableId { get; set; }
    }

    /// <summary>The attribute name a field is exported under.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ExportNameAttribute : Attribute
    {
        public ExportNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    /// <summary>A field of a <see cref="ExportComponentAttribute"/> behaviour that is not exported.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ExportIgnoreAttribute : Attribute
    {
    }
}
