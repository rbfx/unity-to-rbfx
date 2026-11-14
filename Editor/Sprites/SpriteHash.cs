using System.Text;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// The 30-bit hashes of the Sprite Library of the 2D Animation package: <c>Animator.StringToHash</c> (a CRC32 of the UTF-8 text) with the top two bits cleared.
    /// An animation that switches the sprite of a SpriteResolver keys the float <c>m_SpriteKey</c>, and the bits of that float are the hash of <c>"Category_Label"</c>.
    /// Implemented here (no Unity API, no dependency on the package) so that the clips can be read with or without the 2D Animation package installed.
    /// </summary>
    public static class SpriteHash
    {
        private static readonly uint[] Table = MakeTable();

        private static uint[] MakeTable()
        {
            var table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint c = i;
                for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
                table[i] = c;
            }
            return table;
        }

        /// <summary>The CRC32 of the UTF-8 bytes of <paramref name="text"/>, as a signed integer.</summary>
        public static int Crc32(string text)
        {
            uint crc = 0xFFFFFFFFu;
            foreach (byte b in Encoding.UTF8.GetBytes(text ?? string.Empty)) crc = Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
            return unchecked((int)~crc);
        }

        /// <summary>The hash a Sprite Library uses for a name: the CRC32 without its two top bits.</summary>
        public static int Hash30(string text) => Crc32(text) & 0x3FFFFFFF;

        /// <summary>The key of a sprite in a Sprite Library: the hash of <c>Category_Label</c>.</summary>
        public static int Key(string category, string label) => Hash30(category + "_" + label);

        /// <summary>The key that an animation curve holds in a float (the bits of the float are the integer key).</summary>
        public static int KeyFromCurveValue(float value) => System.BitConverter.ToInt32(System.BitConverter.GetBytes(value), 0);
    }
}
