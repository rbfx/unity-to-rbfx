using System.Collections.Generic;

namespace UnityToRebelFork.Editor
{
    /// <summary>A component of any type name with a list of attribute values: what <see cref="ExportComponentAttribute"/> behaviours export as.</summary>
    public class GenericComponent : Component
    {
        private sealed class ValueAttribute : RbfxAttribute
        {
            private readonly Variant _value;

            public ValueAttribute(string name, Variant value) : base(name, value.Type, new Variant())
            {
                _value = value;
            }

            public override Variant Get(RefCounted instance) => _value;
        }

        public GenericComponent(string typeName)
        {
            TypeName = typeName;
        }

        /// <summary>The type name written to the scene.</summary>
        public string TypeName { get; }

        public List<KeyValuePair<string, Variant>> Values { get; } = new List<KeyValuePair<string, Variant>>();

        public GenericComponent Set(string name, Variant value)
        {
            Values.RemoveAll(_ => _.Key == name);
            Values.Add(new KeyValuePair<string, Variant>(name, value));
            return this;
        }

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            foreach (var value in Values) yield return new ValueAttribute(value.Key, value.Value);
        }
    }
}
