using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using KoboldOSC.Helpers;
using KoboldOSC.Messages;

namespace KoboldOSC.Structs;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe ref struct StringLink : ILinkItem
{
    internal unsafe StringLink(ref ParamChain next, ReadOnlySpan<char> value)
    {
        Value = value;
        type = OscType.String;
        this.next = (ParamChain*)Unsafe.AsPointer(ref next);
        byteLength = value.GetAlignedLength();
        totalBytes = next.TotalBytes + ByteLength;
        length = next.Length + 1;
    }

    internal StringLink(ReadOnlySpan<char> value)
    {
        Value = value;
        type = OscType.String;
        byteLength = value.GetAlignedLength();
        totalBytes = ByteLength;
        length = Value.Length;
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
    internal readonly ParamChain* next;


    public readonly ReadOnlySpan<char> Value;


    public static implicit operator StringLink(string other) => new(other);
}
