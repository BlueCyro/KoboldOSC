using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using KoboldOSC.Messages;

namespace KoboldOSC.Structs;

[StructLayout(LayoutKind.Sequential)]
public ref struct ParamChain : ILinkItem
{
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


    internal unsafe ParamChain* next;
}



public interface ILinkItem
{
    /// <summary>
    /// The type of OSC data that this link contains.
    /// </summary>
    OscType Type { get; }

    /// <summary>
    /// The total number of bytes that are left in the chain to read (self-inclusive).
    /// </summary>
    int TotalBytes { get; }

    /// <summary>
    /// The byte length of this particular link in the chain.
    /// </summary>
    int ByteLength { get; }

    /// <summary>
    /// The number of links that come after this one.
    /// </summary>
    int Length { get; }

    /// <summary>
    /// The next link in the chain.
    /// </summary>
    ref ParamChain Next { get; }
}