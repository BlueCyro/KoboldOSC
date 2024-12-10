using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using KoboldOSC.Helpers;
using KoboldOSC.Messages;
using KoboldOSC.Structs;

namespace KoboldOSC.Helpers;

public static class KOSCMessageHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ref ParamChain End(this ref ParamChain prev, ref KOscMessageS message)
    {
        message.refChain = (ParamChain*)Unsafe.AsPointer(ref prev);
        return ref prev;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ParamChain WriteInt(this ref ParamChain prev, in IntLink value)
    {
        return ref Unsafe.As<IntLink, ParamChain>(ref Unsafe.AsRef(in value)).UnsafeUpdateLink(ref prev);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ParamChain WriteFloat(this ref ParamChain prev, in FloatLink value)
    {
        return ref Unsafe.As<FloatLink, ParamChain>(ref Unsafe.AsRef(in value)).UnsafeUpdateLink(ref prev);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ParamChain WriteTimeTag(this ref ParamChain prev, in TimeLink value)
    {
        return ref Unsafe.As<TimeLink, ParamChain>(ref Unsafe.AsRef(in value)).UnsafeUpdateLink(ref prev);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ParamChain WriteString(this ref ParamChain prev, in StringLink value)
    {
        return ref Unsafe.As<StringLink, ParamChain>(ref Unsafe.AsRef(in value)).UnsafeUpdateLink(ref prev);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe ref ParamChain UnsafeUpdateLink(this ref ParamChain chain, ref ParamChain next)
    {
        chain.next = (ParamChain*)Unsafe.AsPointer(ref next);
        Unsafe.AsRef(in chain.totalBytes)    = next.TotalBytes + chain.ByteLength;
        Unsafe.AsRef(in chain.length)        = next.Length + 1;
        return ref chain;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyOscData(this ref ParamChain chain, Span<byte> destination)
    {
        OscType asType = chain.Type;
        switch (asType)
        {
            case OscType.Float:
            case OscType.Int:
                Unsafe.As<ParamChain, IntLink>(ref chain).Value.CopyTo(destination);
                break;

            case OscType.TimeTag:
                Unsafe.As<ParamChain, TimeLink>(ref chain).Value.CopyTo(destination);
                break;

            case OscType.Binary:
            case OscType.String:
                Encoding.UTF8.GetBytes(Unsafe.As<ParamChain, StringLink>(ref chain).Value, destination);
                break;

            default:
                throw new NotImplementedException($"Got unsupported OSC type: {asType}");
        }
    }
}
