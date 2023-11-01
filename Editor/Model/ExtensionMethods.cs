using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public static class ExtensionMethods
    {
        public static void Write(this BinaryWriter writer, UnityEngine.Quaternion v)
        {
            writer.Write(v.w);
            writer.Write(v.x);
            writer.Write(v.y);
            writer.Write(v.z);
        }

        public static void Write(this BinaryWriter writer, UnityEngine.Vector3 v)
        {
            writer.Write(v.x);
            writer.Write(v.y);
            writer.Write(v.z);
        }

        public static void WriteStringSZ(this BinaryWriter writer, string boneName)
        {
            if (string.IsNullOrEmpty(boneName))
            {
                writer.Write((byte)0);
            }
            else
            {
                var a = new UTF8Encoding(false).GetBytes(boneName + '\0');
                writer.Write(a);
            }
        }

        public static string GetName(this VariantType valueType)
        {
            switch (valueType)
            {
                case VariantType.VarNone: return "None";
                case VariantType.VarInt: return "Int";
                case VariantType.VarBool: return "Bool";
                case VariantType.VarFloat: return "Float";
                case VariantType.VarVector2: return "Vector2";
                case VariantType.VarVector3: return "Vector3";
                case VariantType.VarVector4: return "Vector4";
                case VariantType.VarQuaternion: return "Quaternion";
                case VariantType.VarColor: return "Color";
                case VariantType.VarString: return "String";
                case VariantType.VarBuffer: return "Buffer";
                case VariantType.VarVoidPtr: return "VoidPtr";
                case VariantType.VarResourceRef: return "ResourceRef";
                case VariantType.VarResourceRefList: return "ResourceRefList";
                case VariantType.VarIntRect: return "IntRect";
                case VariantType.VarIntVector2: return "IntVector2";
                case VariantType.VarPtr: return "Ptr";
                case VariantType.VarMatrix3: return "Matrix3";
                case VariantType.VarMatrix3x4: return "Matrix3x4";
                case VariantType.VarMatrix4: return "Matrix4";
                case VariantType.VarDouble: return "Double";
                case VariantType.VarStringList: return "StringVector";
                case VariantType.VarRect: return "Rect";
                case VariantType.VarIntVector3: return "IntVector3";
                case VariantType.VarInt64: return "Int64";
                case VariantType.VarCustom: return "Custom";
                case VariantType.VarStringvariantmap: return "StringVariantMap";
                case VariantType.VarVariantList: return "VariantList";
                //case VariantType.VaVe: return "VariantVector";
                //case VariantType.VarM: return "VariantMap";
                //case VariantType.VariantCurve: return "VariantCurve";
                default: throw new NotImplementedException();
            }
        }

        public static IEnumerable<Sample<Vector2>> Merge(this IEnumerable<Sample<float>> t1, IEnumerable<Sample<float>> t2)
        {
            return Merge(t1, t2, (s1, s2) => new Vector2(s1, s2));
        }

        public static IEnumerable<Sample<Vector3>> Merge(this IEnumerable<Sample<Vector2>> t1, IEnumerable<Sample<float>> t2)
        {
            return Merge(t1, t2, (s1, s2) => new Vector3(s1.x, s1.y, s2));
        }

        public static IEnumerable<Sample<Vector4>> Merge(this IEnumerable<Sample<Vector3>> t1, IEnumerable<Sample<float>> t2)
        {
            return Merge(t1, t2, (s1, s2) => new Vector4(s1.x, s1.y, s1.z, s2));
        }

        public static IEnumerable<Sample<Res>> Merge<T1, T2, Res>(this IEnumerable<Sample<T1>> t1, IEnumerable<Sample<T2>> t2,
            Func<T1, T2, Res> mergeFunc)
        {
            using var e1 = t1.GetEnumerator();
            using var e2 = t2.GetEnumerator();
            var hasE1 = e1.MoveNext();
            var hasE2 = e2.MoveNext();
            if (!hasE1 || !hasE2)
                yield break;

            var last1 = e1.Current;
            var last2 = e2.Current;
            while (hasE1 && hasE2)
            {
                if (Math.Abs(e1.Current.Time - e2.Current.Time) < 1e-3f)
                {
                    yield return new Sample<Res>(e1.Current.Time, mergeFunc(e1.Current.Value, e2.Current.Value));
                    last1 = e1.Current;
                    last2 = e2.Current;
                    hasE1 = e1.MoveNext();
                    hasE2 = e2.MoveNext();
                }
                else if (e1.Current.Time < e2.Current.Time)
                {
                    yield return new Sample<Res>(e1.Current.Time, mergeFunc(e1.Current.Value, last2.Value));
                    last1 = e1.Current;
                    hasE1 = e1.MoveNext();
                }
                else
                {
                    yield return new Sample<Res>(e2.Current.Time, mergeFunc(last1.Value, e2.Current.Value));
                    last2 = e2.Current;
                    hasE2 = e2.MoveNext();
                }
            }

            while (hasE1)
            {
                yield return new Sample<Res>(e1.Current.Time, mergeFunc(e1.Current.Value, last2.Value));
                hasE1 = e1.MoveNext();
            }

            while (hasE2)
            {
                yield return new Sample<Res>(e2.Current.Time, mergeFunc(last1.Value, e2.Current.Value));
                hasE2 = e2.MoveNext();
            }
        }
    }
}