using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using KoboldOSC.Helpers;
using KoboldOSC.Messages;

namespace KoboldOSC.Structs;


[StructLayout(LayoutKind.Sequential)]
public readonly unsafe ref struct FloatLink : ILinkItem
{
    internal unsafe FloatLink(ref ParamChain next, float value = default)
    {
        type = OscType.Float;
        this.next = (ParamChain*)Unsafe.AsPointer(ref next);
        Value = value;
        byteLength = Unsafe.SizeOf<float>();
        totalBytes = next.TotalBytes + ByteLength;
        length = next.Length + 1;
    }

    internal FloatLink(float value = default)
    {
        type = OscType.Float;
        Value = value;
        byteLength = Unsafe.SizeOf<float>();
        totalBytes = ByteLength;
        length = 1;
    }

    /// <inheritdoc />
    public readonly OscType Type => type;
    internal readonly OscType type;

    /// <inheritdoc />
    public readonly int TotalBytes => totalBytes;
    internal readonly int totalBytes;

    /// <inheritdoc />
    public readonly int ByteLength => byteLength;
    internal readonly int byteLength;

    /// <inheritdoc />
    public readonly int Length => length;
    internal readonly int length;

    /// <inheritdoc />
    public readonly unsafe ref ParamChain Next => ref Unsafe.AsRef<ParamChain>(next);
    internal unsafe readonly ParamChain* next;


    /// <summary>
    /// The payload of this link.
    /// </summary>
    public readonly float Value;


    public static implicit operator FloatLink(float other) => new(other);
}


[StructLayout(LayoutKind.Sequential)]
public readonly unsafe ref struct IntLink
{
    internal unsafe IntLink(ref ParamChain next, int value = default)
    {
        type = OscType.Int;
        this.next = (ParamChain*)Unsafe.AsPointer(ref next);
        Value = value;
        byteLength = Unsafe.SizeOf<int>();
        totalBytes = next.TotalBytes + ByteLength;
        length = next.Length + 1;
    }

    internal unsafe IntLink(int value = default)
    {
        type = OscType.Int;
        next = null;
        Value = value;
        byteLength = Unsafe.SizeOf<int>();
        totalBytes = ByteLength;
        length = 1;
    }

    /// <inheritdoc />
    public readonly OscType Type => type;
    internal readonly OscType type;

    /// <inheritdoc />
    public readonly int TotalBytes => totalBytes;
    internal readonly int totalBytes;

    /// <inheritdoc />
    public readonly int ByteLength => byteLength;
    internal readonly int byteLength;

    /// <inheritdoc />
    public readonly int Length => length;
    internal readonly int length;

    /// <inheritdoc />
    public readonly unsafe ref ParamChain Next => ref Unsafe.AsRef<ParamChain>(next);
    internal unsafe readonly ParamChain* next;


    /// <summary>
    /// The payload of this link.
    /// </summary>
    public readonly int Value;


    public static implicit operator IntLink(int other) => new(other);
}



[StructLayout(LayoutKind.Sequential)]
public readonly unsafe ref struct TimeLink
{
    internal unsafe TimeLink(ref ParamChain next, ulong value = default)
    {
        type = OscType.TimeTag;
        this.next = (ParamChain*)Unsafe.AsPointer(ref next);
        Value = value;
        byteLength = Unsafe.SizeOf<ulong>();
        totalBytes = next.TotalBytes + ByteLength;
        length = next.Length + 1;
    }

    internal TimeLink(ulong value = default)
    {
        type = OscType.TimeTag;
        Value = value;
        byteLength = Unsafe.SizeOf<ulong>();
        totalBytes = ByteLength;
        length = 1;
    }

    /// <inheritdoc />
    public readonly OscType Type => type;
    internal readonly OscType type;

    /// <inheritdoc />
    public readonly int TotalBytes => totalBytes;
    internal readonly int totalBytes;

    /// <inheritdoc />
    public readonly int ByteLength => byteLength;
    internal readonly int byteLength;

    /// <inheritdoc />
    public readonly int Length => length;
    internal readonly int length;

    /// <inheritdoc />
    public readonly unsafe ref ParamChain Next => ref Unsafe.AsRef<ParamChain>(next);
    internal unsafe readonly ParamChain* next;


    /// <summary>
    /// The payload of this link.
    /// </summary>
    public readonly ulong Value;


    public static implicit operator TimeLink(DateTime other) => new(other.Ticks2Ntp());
}