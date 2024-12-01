using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace KoboldOSC.Structs;

[StructLayout(LayoutKind.Sequential)]
public readonly struct OSCString(OSCString.InlineUtf8 str) : IOSCParameter
{
    public byte ID => '0'.GetOSCIdentifier();
    public int AlignedLength => Length.Ensure4Byte(); // Strings can be N length, so align properly
    public int Length => Value.Length;
    public readonly InlineUtf8 Value = str;
    public readonly void CopyTo(Span<byte> bytes)
    {
        Value.ToBytes()[..AlignedLength].CopyTo(bytes);
    }

    public static implicit operator OSCString(string other) => new(other);
    public static implicit operator string(OSCString other) => Encoding.UTF8.GetString(other.Value.ToBytes()[..other.Length]);

    [InlineArray(256)]
    public struct InlineUtf8
    {
        public const int SIZE = 256;

        public readonly int Length
        {
            get
            {
                int count;
                for (count = 0; count < SIZE; count++)
                    if (this[count] == 0)
                    {
                        count++;
                        break;
                    }

                return count;
            }
        }

        private byte _element;

        public InlineUtf8(string str)
        {
            Encoding.UTF8.GetBytes(str, this.ToBytes());
        }



        public static implicit operator InlineUtf8(string other) => new(other);
        public static implicit operator string(InlineUtf8 other) => Encoding.UTF8.GetString(MemoryMarshal.Cast<InlineUtf8, byte>(new Span<InlineUtf8>(ref other)));
    }
}



public static class OSCStringHelpers
{
    public static Span<byte> ToBytes(this in OSCString.InlineUtf8 inline) => MemoryMarshal.Cast<OSCString.InlineUtf8, byte>(new Span<OSCString.InlineUtf8>(ref Unsafe.AsRef(in inline)));
}
