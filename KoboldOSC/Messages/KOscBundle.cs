using System.Buffers;
using System.Runtime.CompilerServices;
using KoboldOSC.Helpers;
using KoboldOSC.Structs;

namespace KoboldOSC.Messages;

public class KOscBundle : IDisposable, IKOscPacket
{
    public const int BUNDLE_HEADER_LENGTH = 8;
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

    static readonly byte[] bundleHeader = [0x23, 0x62, 0x75, 0x6E, 0x64, 0x6C, 0x65, 0x00];
    private KOscMessage[] bundled;
    private int bundledLength;

    public KOscBundle(params Span<KOscMessage> messages)
    {
        bundledLength = messages.Length;
        bundled = ArrayPool<KOscMessage>.Shared.Rent(bundledLength);
        Array.Clear(bundled);

        for (int i = 0; i < messages.Length; i++)
            bundled[i] = messages[i];
    }


    public void Serialize(Span<byte> destination)
    {
        if (destination.Length < ByteLength)
            throw new ArgumentOutOfRangeException(nameof(destination), "Span destination isn't large enough to hold this message.");

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
