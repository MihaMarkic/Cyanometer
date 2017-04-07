using System.Diagnostics;

namespace Cyanometer.Core.Core
{

    [DebuggerDisplay("{R}.{G}.{B}")]
    public struct Color
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public static Color FromRgb(byte r, byte g, byte b)
        {
            return new Color { R = r, G = g, B = b };
        }

        public static Color FromHex(int value)
        {
            return new Color { R = (byte)((value & 0xFF0000) >> 16), G = (byte)((value & 0xFF00) >> 8), B = (byte)(value & 0xFF) };
        }
    }
}
