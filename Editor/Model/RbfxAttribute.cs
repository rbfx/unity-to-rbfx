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
                var attribute = new XElement(PrefabSceneSerializer.XNames.attribute,
                    new XAttribute(PrefabSceneSerializer.XNames.name, _name),
                    new XAttribute(PrefabSceneSerializer.XNames.type, _type.GetName()));
                WriteValue(attribute, value);
                return attribute;
            }

            return null;
        }

        /// <summary>
        /// A simple value is the <c>value</c> attribute; a list of variants is <c>&lt;value&gt;&lt;element type="..." value="..."/&gt;...&lt;/value&gt;</c>
        /// (an element that is a list holds its own <c>value</c> element), as rbfx writes a scene.
        /// </summary>
        public static void WriteValue(XElement holder, Variant value)
        {
            if (value.Type != VariantType.VarVariantList)
            {
                holder.Add(new XAttribute(PrefabSceneSerializer.XNames.value, value.ToString()));
                return;
            }

            var items = new XElement(PrefabSceneSerializer.XNames.value);
            var list = value.VariantList;
            if (list != null)
            {
                foreach (var item in list)
                {
                    var element = new XElement(XName.Get("element"), new XAttribute(PrefabSceneSerializer.XNames.type, item.Type.GetName()));
                    WriteValue(element, item);
                    items.Add(element);
                }
            }
            holder.Add(items);
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