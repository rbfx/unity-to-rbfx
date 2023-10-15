using System;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace UnityToRebelFork.Editor
{
    public class RbfxAttribute
    {
        private readonly string _name;
        private readonly VariantType _type;
        private readonly Variant _defaultValue;

        public string Name => _name;
        public VariantType Type => _type;
        public Variant DefaultValue => _defaultValue;

        public RbfxAttribute(string name, VariantType type, Variant defaultValue)
        {
            _name = name;
            _type = type;
            _defaultValue = defaultValue;
        }

        public virtual Variant Get(RefCounted instance)
        {
            return new Variant();
        }

        public override string ToString()
        {
            return _name ?? base.ToString();
        }

        public JObject ToPrefabJObject(RefCounted instance)
        {
            var value = Get(instance);
            if (value != _defaultValue)
            {
                return new JObject()
                {
                    new JProperty("name", _name),
                    new JProperty("type", _type.GetName()),
                    new JProperty("value", value.ToJson()),
                };
            }

            return null;
        }

        public XElement ToPrefabXElement(RefCounted instance)
        {
            var value = Get(instance);
            if (value != _defaultValue)
            {
                return new XElement(PrefabSceneSerializer.XNames.attribute,
                    new XAttribute(PrefabSceneSerializer.XNames.name, _name),
                    new XAttribute(PrefabSceneSerializer.XNames.type, _type.GetName()),
                    new XAttribute(PrefabSceneSerializer.XNames.value, value.ToString()));
            }

            return null;
        }
    }

    public class RbfxAttribute<T> : RbfxAttribute
    {
        private readonly Func<RefCounted, T> _getter;
        private readonly Action<RefCounted, T> _setter;

        public RbfxAttribute(string name, VariantType type, Variant defaultValue, Func<RefCounted, T> getter, Action<RefCounted, T> setter) : base(name, type, defaultValue)
        {
            _getter = getter;
            _setter = setter;
        }

        public override Variant Get(RefCounted instance)
        {
            return new Variant(Type, _getter(instance));
        }

    }
}