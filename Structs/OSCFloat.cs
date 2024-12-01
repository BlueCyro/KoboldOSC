using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KoboldOSC.Structs;

[StructLayout(LayoutKind.Sequential)]
public readonly struct OSCFloat(float value) : IOSCParameter
{
    public byte ID => Value.GetOSCIdentifier();
    public int AlignedLength => Length; // Implicitly aligned
    public int Length => Unsafe.SizeOf<float>();
    public readonly float Value = value;
    public readonly void CopyTo(Span<byte> bytes)
    {
        ref uint floatBytes = ref Unsafe.As<float, uint>(ref Unsafe.AsRef(in Value));
        bytes[0] = (byte)(floatBytes >> 24);
        bytes[1] = (byte)(floatBytes >> 16);
        bytes[2] = (byte)(floatBytes >>  8);
        bytes[3] = (byte) floatBytes;
    }


    public static implicit operator float(OSCFloat other) => other.Value;
    public static implicit operator OSCFloat(float other) => new(other);
}
