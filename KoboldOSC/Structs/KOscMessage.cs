using System.Runtime.CompilerServices;
using System.Text;
using KoboldOSC.Helpers;
using KoboldOSC.Messages;

namespace KoboldOSC.Structs;

public ref struct KOscMessageS(string path)
{
    public readonly string Path = path;
    public readonly int ByteLength => PathBytes + IdTableBytes + ParameterBytes;

    public readonly int PathBytes => Path.GetAlignedLength();
    public readonly int IdTableBytes => (1 + ItemCount).Ensure4Byte();
    public readonly int ParameterBytes => Chain.TotalBytes;
    public readonly int ItemCount => Chain.Length;

    public unsafe readonly ref ParamChain Chain => ref Unsafe.AsRef<ParamChain>(refChain);
    internal unsafe ParamChain* refChain;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ParamChain Start(out ParamChain discard)
    {
        discard = new();
        return ref Unsafe.AsRef(in discard);
    }


    public readonly void Serialize(Span<byte> buffer)
    {
        Span<byte> pathBuf = buffer;
        Span<byte> idMark = buffer[PathBytes..];
        Encoding.UTF8.GetBytes(Path, pathBuf);
        idMark[0] = 44; // The ',' character.

        Span<byte> idBuf = idMark[1..];
        Span<byte> paramBuf = idMark[IdTableBytes..];

        ref ParamChain chain = ref Chain;


        int curIdIndex = ItemCount;
        int curParamIndex = ParameterBytes;
        while (chain.Length > 0)
        {
            idBuf[--curIdIndex] = (byte)chain.Type;
            curParamIndex -= chain.ByteLength;
            chain.CopyOscData(paramBuf[curParamIndex..]);
            chain = ref chain.GetNext();
        }
    }
}
