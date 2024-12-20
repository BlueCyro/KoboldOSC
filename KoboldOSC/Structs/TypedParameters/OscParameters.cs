using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using KoboldOSC.Helpers;
using KoboldOSC.Messages;

namespace KoboldOSC.Parameters;



/// <summary>
/// An OSC parameter representing a single-precision floating-point number.
/// <para>
/// <b>NOTE</b>: Any float can be implicitly converted to this type, there is no public constructor.
/// </para>
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 32, Pack = 64)]
public readonly ref struct FloatParam : IOscParameter
{
    internal unsafe FloatParam(float value)
    {
        Type = OscType.Float;
        ByteLength = Unsafe.SizeOf<float>();
        Value = value;
        copyToDangerous = &CopyTo;
    }

    /// <inheritdoc />
    public readonly OscType Type { get; }

    /// <inheritdoc />
    public readonly int ByteLength { get; }



    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly unsafe delegate* <ref FloatParam, Span<byte>, int, void> copyToDangerous;

    /// <summary>
    /// The payload of this parameter.
    /// </summary>
    public readonly float Value;


    public static implicit operator FloatParam(float other) => new(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo(ref FloatParam param, Span<byte> dest, int offset) => param.Value.CopyBytesTo(dest, offset);
}



/// <summary>
/// An OSC parameter representing a 32-bit integer.
/// <para>
/// <b>NOTE</b>: Any 32-bit int can be implicitly converted to this type, there is no public constructor.
/// </para>
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 32, Pack = 64)]
public readonly ref struct IntParam : IOscParameter
{
    internal unsafe IntParam(int value)
    {
        Type = OscType.Int;
        ByteLength = Unsafe.SizeOf<int>();
        Value = value;
        copyToDangerous = &CopyTo;
    }

    /// <inheritdoc />
    public readonly OscType Type { get; }

    /// <inheritdoc />
    public readonly int ByteLength { get; }



    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly unsafe delegate* <ref IntParam, Span<byte>, int, void> copyToDangerous;


    /// <summary>
    /// The payload of this parameter.
    /// </summary>
    public readonly int Value;


    public static implicit operator IntParam(int other) => new(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo(ref IntParam param, Span<byte> dest, int offset) => param.Value.CopyBytesTo(dest, offset);
}



/// <summary>
/// An OSC parameter representing an NTP timestamp.
/// <para>
/// <b>NOTE</b>: Any DateTime value can be implicitly converted to this type, there is no public constructor.
/// </para>
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 32, Pack = 64)]
public readonly ref struct TimeParam : IOscParameter
{
    public unsafe TimeParam(ulong value)
    {
        Type = OscType.TimeTag;
        ByteLength = Unsafe.SizeOf<ulong>();
        Value = value;
        copyToDangerous = &CopyTo;
    }


    internal unsafe TimeParam(DateTime value)
    {
        Type = OscType.TimeTag;
        ByteLength = Unsafe.SizeOf<ulong>();
        Value = value.Ticks2Ntp();
        copyToDangerous = &CopyTo;
    }

    /// <inheritdoc />
    public readonly OscType Type { get; }

    /// <inheritdoc />
    public readonly int ByteLength { get; }



    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly unsafe delegate* <ref TimeParam, Span<byte>, int, void> copyToDangerous;


    /// <summary>
    /// The payload of this parameter.
    /// </summary>
    public readonly ulong Value;


    public static implicit operator TimeParam(DateTime other) => new(other.Ticks2Ntp());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo(ref TimeParam param, Span<byte> dest, int offset) => param.Value.CopyBytesTo(dest, offset);
}



/// <summary>
/// An OSC parameter representing a string.
/// <para>
/// <b>NOTE</b>: Any string can be implicitly converted to this type, there is no public constructor.
/// </para>
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 32, Pack = 64)]
public readonly ref struct StringParam : IOscParameter
{
    internal unsafe StringParam(ReadOnlySpan<char> value)
    {
        Type = OscType.String;
        ByteLength = value.GetAlignedLength();
        Value = value;
        copyToDangerous = &CopyTo;
    }

    /// <inheritdoc />
    public readonly OscType Type { get; }

    /// <inheritdoc />
    public readonly int ByteLength { get; }



    [DebuggerBrowsable(DebuggerBrowsableState.Never)]    
    private readonly unsafe delegate* <ref StringParam, Span<byte>, int, void> copyToDangerous;


    /// <summary>
    /// The payload of this parameter.
    /// </summary>
    public readonly ReadOnlySpan<char> Value;


    public static implicit operator StringParam(string other) => new(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo(ref StringParam param, Span<byte> dest, int offset) => param.Value.CopyBytesTo(dest, offset);
}



/// <summary>
/// An OSC parameter representing an arbitrary buffer of binary data.
/// <para>
/// <b>NOTE</b>: Any span of bytes can be implicitly converted to this type, there is no public constructor.
/// </para>
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 32, Pack = 64)]
public readonly ref struct BinaryParam : IOscParameter
{
    internal unsafe BinaryParam(ReadOnlySpan<byte> value)
    {
        Type = OscType.Binary;
        ByteLength = Unsafe.SizeOf<int>() + value.GetAlignedLength();
        Value = value;
        copyToDangerous = &CopyTo;
    }

    /// <inheritdoc />
    public readonly OscType Type { get; }

    /// <inheritdoc />
    public readonly int ByteLength { get; }



    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly unsafe delegate* <ref BinaryParam, Span<byte>, int, void> copyToDangerous;

    /// <summary>
    /// The payload of this parameter.
    /// </summary>
    public readonly ReadOnlySpan<byte> Value;


    public static implicit operator BinaryParam(ReadOnlySpan<byte> other) => new(other);
    public static implicit operator BinaryParam(byte[] other) => new(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo(ref BinaryParam param, Span<byte> dest, int offset)
    {
        param.Value.Length.CopyBytesTo(dest, offset);
        param.Value.CopyBytesTo(dest, offset + Unsafe.SizeOf<int>());
    }
}