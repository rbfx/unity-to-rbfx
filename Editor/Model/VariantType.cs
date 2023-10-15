namespace UnityToRebelFork.Editor
{
    public enum VariantType
    {
        VarNone = 0,
        VarInt = 1,
        VarBool = 2,
        VarFloat = 3,
        VarVector2 = 4,
        VarVector3 = 5,
        VarVector4 = 6,
        VarQuaternion = 7,
        VarColor = 8,
        VarString = 9,
        VarBuffer = 10, // 0x0000000A
        VarVoidPtr = 11, // 0x0000000B
        VarResourceRef = 12, // 0x0000000C
        VarResourceRefList = 13, // 0x0000000D
        VarVariantList = 14, // 0x0000000E
        VarVariantMap = 15, // 0x0000000F
        VarIntRect = 16, // 0x00000010
        VarIntVector2 = 17, // 0x00000011
        VarPtr = 18, // 0x00000012
        VarMatrix3 = 19, // 0x00000013
        VarMatrix3x4 = 20, // 0x00000014
        VarMatrix4 = 21, // 0x00000015
        VarDouble = 22, // 0x00000016
        VarStringList = 23, // 0x00000017
        VarRect = 24, // 0x00000018
        VarIntVector3 = 25, // 0x00000019
        VarInt64 = 26, // 0x0000001A
        VarCustom = 27, // 0x0000001B
        VarVariantcurve = 28, // 0x0000001C
        VarStringvariantmap = 29, // 0x0000001D
        MaxVarTypes = 30, // 0x0000001E
        MaxVarMask = 63, // 0x0000003F
    }
}