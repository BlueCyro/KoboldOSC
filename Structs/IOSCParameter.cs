using System.Runtime.CompilerServices;
using System.Text;

namespace KoboldOSC.Structs;

[Obsolete]
public interface IOSCParameter
{
    public byte ID { get; }
    public int AlignedLength { get; }
    public int Length { get; }
    public void CopyTo(Span<byte> bytes);
}


public static class KOscValueHelpers
{
    public static readonly DateTime Base = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo(this int value, Span<byte> bytes)
    {
        bytes[0] = (byte)(value >> 24);
        bytes[1] = (byte)(value >> 16);
        bytes[2] = (byte)(value >>  8);
        bytes[3] = (byte) value;
    }

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo(this float value, Span<byte> bytes)
    {
        ref uint valUint = ref Unsafe.As<float, uint>(ref value);
        bytes[0] = (byte)(valUint >> 24);
        bytes[1] = (byte)(valUint >> 16);
        bytes[2] = (byte)(valUint >>  8);
        bytes[3] = (byte) valUint;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo(this ulong value, Span<byte> bytes)
    {
        bytes[0] = (byte)(value >> 56);
        bytes[1] = (byte)(value >> 48);
        bytes[2] = (byte)(value >> 40);
        bytes[3] = (byte)(value >> 32);
        bytes[4] = (byte)(value >> 24);
        bytes[5] = (byte)(value >> 16);
        bytes[6] = (byte)(value >>  8);
        bytes[7] = (byte) value;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo(this string value, Span<byte> bytes)
    {
        Encoding.UTF8.GetBytes(value, bytes);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetAlignedLength(this string value)
    {
        return (Encoding.UTF8.GetByteCount(value) + 1).Ensure4Byte();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetAlignedLength(this Span<byte> value)
    {
        return value.Length.Ensure4Byte();
    }



    public static ulong ToNtp(this DateTime time)
    {
        TimeSpan span = time.Subtract(Base);

        double seconds = span.TotalSeconds;
        uint uintSeconds = (uint)seconds;

        double milliseconds = span.TotalMilliseconds - ((double)uintSeconds * 1000);
        double fraction = (milliseconds / 1000) * ((double)uint.MaxValue);

        return (((ulong)uintSeconds & 0xFFFFFFFF) << 32) | ((ulong)fraction & 0xFFFFFFFF);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Ensure4Byte(this int num) => ((num + 3) >> 2) << 2;
}