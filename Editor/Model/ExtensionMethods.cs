using System;
using System.IO;
using System.Text;

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
            var a = new UTF8Encoding(false).GetBytes(boneName + '\0');
            writer.Write(a);
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
    }
}