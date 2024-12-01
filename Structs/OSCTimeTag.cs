using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KoboldOSC.Structs;

[StructLayout(LayoutKind.Sequential)]
public readonly struct OSCTimeTag : IOSCParameter
{
    public byte ID => Value.GetOSCIdentifier();
    public int AlignedLength => Length; // Implicitly aligned
    public int Length => Unsafe.SizeOf<ulong>();
    public readonly ulong Value;

    public OSCTimeTag(DateTime time)
    {
        Value = new NtpTime(time);
    }


    public readonly void CopyTo(Span<byte> bytes)
    {
        bytes[0] = (byte)(Value >> 56);
        bytes[1] = (byte)(Value >> 48);
        bytes[2] = (byte)(Value >> 40);
        bytes[3] = (byte)(Value >> 32);
        bytes[4] = (byte)(Value >> 24);
        bytes[5] = (byte)(Value >> 16);
        bytes[6] = (byte)(Value >>  8);
        bytes[7] = (byte) Value;
    }
}
