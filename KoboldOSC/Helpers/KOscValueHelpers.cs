using System.Runtime.CompilerServices;
using System.Text;

namespace KoboldOSC.Helpers;

/// <summary>
/// Helpers to aid in serialization of OSC packets.
/// </summary>
public static class KOscValueHelpers
{
    /// <summary>
    /// This particular timestamp is used as a base to calculate an NTP timestamp from a DateTime.
    /// </summary>
    public static readonly DateTime Base = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);



    /// <summary>
    /// Copies an integer's composite bytes to a span, big endian. Does NOT check the span size.
    /// </summary>
    /// <param name="value">The int to copy bytes from.</param>
    /// <param name="bytes">The span to copy bytes to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo(this int value, Span<byte> bytes)
    {
        bytes[0] = (byte)(value >> 24);
        bytes[1] = (byte)(value >> 16);
        bytes[2] = (byte)(value >>  8);
        bytes[3] = (byte) value;
    }



    /// <summary>
    /// Copies a float's composite bytes to a span, big endian. Does NOT check the span size.
    /// </summary>
    /// <param name="value">The float to copy bytes from.</param>
    /// <param name="bytes">The span to copy bytes to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo(this float value, Span<byte> bytes)
    {
        ref uint valUint = ref Unsafe.As<float, uint>(ref value);
        bytes[0] = (byte)(valUint >> 24);
        bytes[1] = (byte)(valUint >> 16);
        bytes[2] = (byte)(valUint >>  8);
        bytes[3] = (byte) valUint;
    }


    /// <summary>
    /// Copies a ulong's composite bytes to a span, big endian. Does NOT check the span size.
    /// </summary>
    /// <param name="value">The ulong to copy bytes from.</param>
    /// <param name="bytes">The span to copy bytes to.</param>
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



    /// <summary>
    /// Copies a string's composite UTF8 bytes to a span, big endian. Does NOT check the span size.
    /// </summary>
    /// <param name="value">The ulong to copy bytes from.</param>
    /// <param name="bytes">The span to copy bytes to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo(this string value, Span<byte> bytes) => Encoding.UTF8.GetBytes(value, bytes);


    /// <summary>
    /// Gets the 4-byte-aligned, UTF8 byte count of a string. Rounds up to the next alignment.
    /// </summary>
    /// <param name="value">The string to get the aligned length of.</param>
    /// <returns>A 4-byte-aligned, rounded-up UTF8 byte count of the string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetAlignedLength(this string value) => (Encoding.UTF8.GetByteCount(value) + 1).Ensure4Byte();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetAlignedLength(this Span<byte> value) => value.Length.Ensure4Byte();


    /// <summary>
    /// Converts a DateTime into a raw NTP timestamp.
    /// </summary>
    /// <param name="time">The time to convert.</param>
    /// <returns>Raw NTP timestamp.</returns>
    public static ulong ToNtp(this DateTime time)
    {
        TimeSpan span = time.Subtract(Base);

        double seconds = span.TotalSeconds;
        uint uintSeconds = (uint)seconds;

        double milliseconds = span.TotalMilliseconds - ((double)uintSeconds * 1000);
        double fraction = (milliseconds / 1000) * ((double)uint.MaxValue);

        return (((ulong)uintSeconds & 0xFFFFFFFF) << 32) | ((ulong)fraction & 0xFFFFFFFF);
    }


    /// <summary>
    /// Ensures a number is aligned to the next multiple of 4.
    /// </summary>
    /// <param name="num">The number to round.</param>
    /// <returns>The next multiple of 4.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Ensure4Byte(this int num) => ((num + 3) >> 2) << 2;
}