using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public readonly struct Variant : IEquatable<Variant>
    {
        public bool Equals(Variant other)
        {
            if (_type != other._type)
                return false;

            if (_value == null && other._value == null)
                return true;

            if (object.ReferenceEquals(_value, other._value))
                return true;

            switch (_type)
            {
                case VariantType.VarNone: return true;
                case VariantType.VarBool: return Bool == other.Bool;
                case VariantType.VarInt: return Int == other.Int;
                case VariantType.VarString: return (this.String??"") == (other.String ?? "");
                case VariantType.VarFloat: return Float == other.Float;
                case VariantType.VarDouble: return this.Double == other.Double;
                case VariantType.VarVector2: return this.Vector2 == other.Vector2;
                case VariantType.VarVector3: return this.Vector3 == other.Vector3;
                case VariantType.VarVector4: return this.Vector4 == other.Vector4;
                case VariantType.VarQuaternion: return this.Quaternion == other.Quaternion;
                case VariantType.VarResourceRef: return this.ResourceRef == other.ResourceRef;
                case VariantType.VarResourceRefList: return this.ResourceRefList == other.ResourceRefList;
                case VariantType.VarStringList: return this.Compare(this.StringList, other.StringList);
                case VariantType.VarStringvariantmap: return this.Compare(this.Stringvariantmap, other.Stringvariantmap);
                case VariantType.VarColor: return this.Color == other.Color;
                case VariantType.VarVariantList: return this.VariantList == other.VariantList;
                case VariantType.VarBuffer: return true; // Buffer is not supported.
                default: throw new NotImplementedException(_type.GetName());
            }
        }

        private bool Compare(Dictionary<string, Variant> stringList, Dictionary<string, Variant> otherStringList)
        {
            return true;
        }

        private bool Compare(IList<string> stringList, IList<string> otherStringList)
        {
            var empty = stringList == null || stringList.Count == 0;
            var otherEmpty = otherStringList == null || otherStringList.Count == 0;
            if (empty && otherEmpty)
                return true;
            if (empty || otherEmpty)
                return false;
            if (stringList.Count != otherStringList.Count) return false;
            for (var index = 0; index < stringList.Count; index++)
            {
                if (stringList[index] != otherStringList[index]) return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is Variant other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)_type, _value);
        }

        public static bool operator ==(Variant left, Variant right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Variant left, Variant right)
        {
            return !left.Equals(right);
        }

        private readonly VariantType _type;
        private readonly object _value;

        public static implicit operator int(Variant value)
        {
            return value.Int;
        }
        public static implicit operator Variant(int value)
        {
            return new Variant(value);
        }
        public static implicit operator float(Variant value)
        {
            return value.Float;
        }
        public static implicit operator Variant(float value)
        {
            return new Variant(value);
        }
        public static implicit operator Vector4(Variant value)
        {
            return value.Vector4;
        }
        public static implicit operator Variant(Vector3 value)
        {
            return new Variant(value);
        }
        public static implicit operator Variant(Vector4 value)
        {
            return new Variant(value);
        }
        public static implicit operator Variant(Color value)
        {
            return new Variant(value);
        }
        public static implicit operator Variant(Color32 value)
        {
            return new Variant((Color)value);
        }

        public Variant(VariantType type, object value)
        {
            _value = value;
            _type = type;
        }
        public Variant(bool value)
        {
            _value = value;
            _type = VariantType.VarBool;
        }
        public Variant(int value)
        {
            _value = value;
            _type = VariantType.VarInt;
        }
        public Variant(float value)
        {
            _value = value;
            _type = VariantType.VarFloat;
        }
        public Variant(string value)
        {
            _value = value;
            _type = VariantType.VarString;
        }
        public Variant(IList<string> value)
        {
            _value = value;
            _type = VariantType.VarStringList;
        }
        public Variant(Vector2 value)
        {
            _value = value;
            _type = VariantType.VarVector2;
        }
        public Variant(Vector3 value)
        {
            _value = value;
            _type = VariantType.VarVector3;
        }
        public Variant(Vector4 value)
        {
            _value = value;
            _type = VariantType.VarVector4;
        }
        public Variant(IntVector2 value)
        {
            _value = value;
            _type = VariantType.VarIntVector2;
        }
        public Variant(IntVector3 value)
        {
            _value = value;
            _type = VariantType.VarIntVector3;
        }
        public Variant(Quaternion value)
        {
            _value = value;
            _type = VariantType.VarQuaternion;
        }
        public Variant(Buffer value)
        {
            _value = value;
            _type = VariantType.VarBuffer;
        }
        public Variant(Color value)
        {
            _value = value;
            _type = VariantType.VarColor;
        }
        public Variant(Rect value)
        {
            _value = value;
            _type = VariantType.VarRect;
        }
        public Variant(IntRect value)
        {
            _value = value;
            _type = VariantType.VarIntRect;
        }
        public Variant(ResourceRef value)
        {
            _value = value;
            _type = VariantType.VarResourceRef;
        }
        public Variant(ResourceRefList value)
        {
            _value = value;
            _type = VariantType.VarResourceRefList;
        }
        public Variant(VariantList value)
        {
            _value = value;
            _type = VariantType.VarVariantList;
        }
        public Variant(VariantMap value)
        {
            _value = value;
            _type = VariantType.VarVariantMap;
        }
        public Variant(Dictionary<string, Variant> value)
        {
            _value = value;
            _type = VariantType.VarStringvariantmap;
        }

        public bool Bool => _type == VariantType.VarBool ? (bool)_value : default;
        public int Int => _type == VariantType.VarInt ? (int)_value : default;
        public long Int64 => _type == VariantType.VarInt64 ? (long)_value : default;
        public float Float => _type == VariantType.VarFloat ? (float)_value : default;
        public double Double => _type == VariantType.VarDouble ? (double)_value : default;
        public string String => _type == VariantType.VarString ? (string)_value : default;
        public IList<string> StringList => _type == VariantType.VarStringList ? (IList<string>)_value : default;
        public Vector2 Vector2 => _type == VariantType.VarVector2 ? (Vector2)_value : default;
        public Vector3 Vector3 => _type == VariantType.VarVector3 ? (Vector3)_value : default;
        public Vector4 Vector4 => _type == VariantType.VarVector4 ? (Vector4)_value : default;
        public IntVector2 IntVector2 => _type == VariantType.VarIntVector2 ? (IntVector2)_value : default;
        public IntVector3 IntVector3 => _type == VariantType.VarIntVector3 ? (IntVector3)_value : default;
        public Quaternion Quaternion => _type == VariantType.VarQuaternion ? (Quaternion)_value : default;
        public Buffer Buffer => _type == VariantType.VarBuffer ? (Buffer)_value : default;
        public Color Color => _type == VariantType.VarColor ? (Color)_value : default;
        public Rect Rect => _type == VariantType.VarRect ? (Rect)_value : default;
        public IntRect IntRect => _type == VariantType.VarIntRect ? (IntRect)_value : default;
        public ResourceRef ResourceRef => _type == VariantType.VarResourceRef ? (ResourceRef)_value : default;
        public ResourceRefList ResourceRefList => _type == VariantType.VarResourceRefList ? (ResourceRefList)_value : default;
        public VariantList VariantList => _type == VariantType.VarVariantList ? (VariantList)_value : default;
        public VariantMap VariantMap => _type == VariantType.VarVariantMap ? (VariantMap)_value : default;
        public Dictionary<string, Variant> Stringvariantmap => _type == VariantType.VarStringvariantmap ? (Dictionary<string, Variant>)_value : default;
        public VariantType Type => _type;

        public override string ToString()
        {
            switch (_type)
            {
                case VariantType.VarBool: return Bool.ToString();
                case VariantType.VarInt: return Int.ToString();
                case VariantType.VarFloat: return Float.ToString();
                case VariantType.VarString: return String.ToString();
                case VariantType.VarStringList: return StringList.ToString();
                case VariantType.VarVector2: return $"{Vector2.x} {Vector2.y}";
                case VariantType.VarVector3: return $"{Vector3.x} {Vector3.y} {Vector3.z}";
                case VariantType.VarVector4: return $"{Vector4.x} {Vector4.y} {Vector4.z} {Vector4.w}";
                case VariantType.VarIntVector2: return IntVector2.ToString();
                case VariantType.VarIntVector3: return IntVector3.ToString();
                case VariantType.VarQuaternion: return $"{Quaternion.w} {Quaternion.x} {Quaternion.y} {Quaternion.z}";
                case VariantType.VarBuffer: return Buffer.ToString();
                case VariantType.VarColor: return $"{Color.r} {Color.g} {Color.b} {Color.a}";
                case VariantType.VarRect: return Rect.ToString();
                case VariantType.VarIntRect: return IntRect.ToString();
                case VariantType.VarResourceRef: return ResourceRef.ToString();
                case VariantType.VarResourceRefList: return ResourceRefList.ToString();
                case VariantType.VarVariantList: return VariantList.ToString();
                case VariantType.VarVariantMap: return VariantMap.ToString();
                case VariantType.VarStringvariantmap: return Stringvariantmap.ToString();
                default:
                    throw new NotImplementedException();
            }
        }

        public object ToJson()
        {
            switch (_type)
            {
                case VariantType.VarBool: return this.Bool;
                case VariantType.VarFloat: return this.Float;
                case VariantType.VarDouble: return this.Double;
                case VariantType.VarInt: return this.Int;
                case VariantType.VarInt64: return this.Int64;
            }

            return ToString();
        }
    }
}