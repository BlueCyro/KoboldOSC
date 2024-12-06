using System.Buffers;
using System.Runtime.CompilerServices;
using KoboldOSC.Helpers;
using KoboldOSC.Structs;

namespace KoboldOSC.Messages;

/// <summary>
/// Represents a bundle of OSC messages.
/// </summary>
public class KOscBundle : IDisposable, IKOscPacket
{
    /// <summary>
    /// The constant length of a null-terminated bundle header: #bundle\0x00
    /// </summary>
    public const int BUNDLE_HEADER_LENGTH = 8;

    // The header "#bundle" terminated by a null byte.
    static readonly byte[] bundleHeader = [0x23, 0x62, 0x75, 0x6E, 0x64, 0x6C, 0x65, 0x00];

    /// <inheritdoc/>
    public int ByteLength
    {
        get
        {
            int count = BUNDLE_HEADER_LENGTH + Unsafe.SizeOf<ulong>();
            for (int i = 0; i < bundledLength; i++)
                count += 4 + bundled[i].ByteLength;

            return count;
        }
    }

    private KOscMessage[] bundled;
    private int bundledLength;

    /// <summary>
    /// Creates a new bundle of OSC messages.
    /// </summary>
    /// <param name="messages">The messages to include in this bundle.</param>
    public KOscBundle(params Span<KOscMessage> messages)
    {
        bundledLength = messages.Length;
        bundled = ArrayPool<KOscMessage>.Shared.Rent(bundledLength);
        Array.Clear(bundled);

        for (int i = 0; i < messages.Length; i++)
            bundled[i] = messages[i];
    }


    /// <inheritdoc/>
    public void Serialize(Span<byte> destination)
    {
        int curIndex = bundleHeader.Length;
        Span<byte> bundleHeaderSlice = destination[..curIndex];
        bundleHeader.CopyTo(bundleHeaderSlice);

        Span<byte> timeTagSlice = destination.Slice(curIndex, Unsafe.SizeOf<ulong>());
        ulong curTime = DateTime.Now.ToNtp();
        curTime.CopyTo(timeTagSlice);
        curIndex += Unsafe.SizeOf<ulong>();

        for (int i = 0; i < bundledLength; i++)
        {
            KOscMessage curMsg = bundled[i];
            Span<byte> curSizeSlice = destination.Slice(curIndex, Unsafe.SizeOf<int>());
            curMsg.ByteLength.CopyTo(curSizeSlice);
            curIndex += Unsafe.SizeOf<int>();


            Span<byte> curMessageSlice = destination.Slice(curIndex, curMsg.ByteLength);
            bundled[i].Serialize(curMessageSlice);
            curIndex += curMsg.ByteLength;
        }
    }


    /// <summary>
    /// Disposes of this OSC packet and frees the array pool.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        ArrayPool<KOscMessage>.Shared.Return(bundled);
        for (int i = bundledLength - 1; i > 0; i--)
        {
            bundled[i].Dispose();
        }
    }
}
