using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
    public static readonly long Base = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;



    /// <summary>
    /// Copies an integer's composite bytes to a span, big endian. Does NOT check the span size.
    /// </summary>
    /// <param name="value">The int to copy bytes from.</param>
    /// <param name="bytes">The span to copy bytes to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyBytesTo(this int value, Span<byte> bytes, int offset = 0) => unchecked((uint)value).CopyBytesTo(bytes, offset);



    /// <summary>
    /// Copies a float's composite bytes to a span, big endian. Does NOT check the span size.
    /// </summary>
    /// <param name="value">The float to copy bytes from.</param>
    /// <param name="bytes">The span to copy bytes to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyBytesTo(this float value, Span<byte> bytes, int offset = 0) => Unsafe.As<float, uint>(ref value).CopyBytesTo(bytes, offset);


    /// <summary>
    /// Copies a uint's composite bytes to a span, big endian. Does NOT check the span size.
    /// </summary>
    /// <param name="value">The uint to copy bytes from.</param>
    /// <param name="bytes">The span to copy bytes to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyBytesTo(this uint value, Span<byte> bytes, int offset = 0)
    {
        ref uint dest = ref Unsafe.As<byte, uint>(ref MemoryMarshal.GetReference(bytes[offset..]));
        if (BitConverter.IsLittleEndian)
            dest = BinaryPrimitives.ReverseEndianness(value);
        else
            dest = value;
    }


    /// <summary>
    /// Copies a ulong's composite bytes to a span, big endian. Does NOT check the span size.
    /// </summary>
    /// <param name="value">The ulong to copy bytes from.</param>
    /// <param name="bytes">The span to copy bytes to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyBytesTo(this ulong value, Span<byte> bytes, int offset = 0)
    {
        ref ulong dest = ref Unsafe.As<byte, ulong>(ref MemoryMarshal.GetReference(bytes[offset..]));
        if (BitConverter.IsLittleEndian)
            dest = BinaryPrimitives.ReverseEndianness(value);
        else
            dest = value;
    }



    /// <summary>
    /// Copies a string's composite UTF8 bytes to a span, big endian. Does NOT check the span size.
    /// </summary>
    /// <param name="value">The ulong to copy bytes from.</param>
    /// <param name="bytes">The span to copy bytes to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void CopyBytesTo(this string value, Span<byte> bytes, int offset = 0) => Encoding.UTF8.GetBytes(value, bytes[offset..]);


    /// <summary>
    /// Copies a string's composite UTF8 bytes to a span, big endian. Does NOT check the span size.
    /// </summary>
    /// <param name="value">The ulong to copy bytes from.</param>
    /// <param name="bytes">The span to copy bytes to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyBytesTo(this ReadOnlySpan<char> value, Span<byte> bytes, int offset = 0) => Encoding.UTF8.GetBytes(value, bytes[offset..]);


    /// <summary>
    /// Copies a string's composite UTF8 bytes to a span, big endian. Does NOT check the span size.
    /// </summary>
    /// <param name="value">The ulong to copy bytes from.</param>
    /// <param name="bytes">The span to copy bytes to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void CopyBytesTo(this ReadOnlySpan<byte> value, Span<byte> bytes, int offset = 0)
    {
        value.CopyTo(bytes[offset..]);
    }

    /// <summary>
    /// Gets the 4-byte-aligned, UTF8 byte count of a string. Rounds up to the next alignment.
    /// </summary>
    /// <param name="value">The string to get the aligned length of.</param>
    /// <returns>A 4-byte-aligned, rounded-up UTF8 byte count of the string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetAlignedLength(this string value) => GetAlignedLength(value.AsSpan());



    /// <inheritdoc cref="GetAlignedLength(string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetAlignedLength(this ReadOnlySpan<char> value) => (Encoding.UTF8.GetByteCount(value) + 1).Ensure4Byte();


    /// <summary>
    /// Gets the 4-byte-aligned length of a span of bytes.
    /// </summary>
    /// <param name="value">The span to get the aligned length of.</param>
    /// <returns>The length, rounded up to the nearest multiple of 4.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetAlignedLength(this ReadOnlySpan<byte> value) => value.Length.Ensure4Byte();


    /// <summary>
    /// Converts a DateTime into a raw NTP timestamp.
    /// </summary>
    /// <param name="time">The time to convert.</param>
    /// <returns>Raw NTP timestamp.</returns>
    [Obsolete($"This method is slower than {nameof(Ticks2Ntp)}.")]
    public static ulong ToNtp(this DateTime time)
    {
        TimeSpan span = TimeSpan.FromTicks(time.Ticks - Base);

        double seconds = span.TotalSeconds;
        uint uintSeconds = (uint)seconds;

        double milliseconds = span.TotalMilliseconds - ((double)uintSeconds * 1000);
        double fraction = (milliseconds / 1000) * ((double)uint.MaxValue);

        return (((ulong)uintSeconds & 0xFFFFFFFF) << 32) | ((ulong)fraction & 0xFFFFFFFF);
    }



    /// <summary>
    /// Converts DateTime into a Network Time Protocol (NTP) timestamp.
    /// <para>
    /// Modified version of Cameron's code on stackoverflow: https://stackoverflow.com/a/23160246
    /// </para>
    /// </summary>
    /// <param name="time">The time to convert.</param>
    /// <returns>Raw NTP timestamp.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Ticks2Ntp(this DateTime time)
    {
        double b = time.Ticks - Base;
        b = b / 10000000L * (1UL << 32);
        return (ulong)b;
    }


    /// <summary>
    /// Ensures a number is aligned to the next multiple of 4.
    /// </summary>
    /// <param name="num">The number to round.</param>
    /// <returns>The next multiple of 4.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Ensure4Byte(this int num) => ((num + 3) >> 2) << 2;
}