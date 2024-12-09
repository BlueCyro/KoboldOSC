using System.Buffers;
using System.Runtime.CompilerServices;
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
    public static ref ParamChain WriteInt(this ref ParamChain prev, int value, out ParamLink<int> discard)
    {
        discard = new(ref prev, value);
        return ref Unsafe.As<ParamLink<int>, ParamChain>(ref Unsafe.AsRef(in discard));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ParamChain WriteFloat(this ref ParamChain prev, float value, out ParamLink<float> discard)
    {
        discard = new(ref prev, value);
        return ref Unsafe.As<ParamLink<float>, ParamChain>(ref Unsafe.AsRef(in discard));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ParamChain WriteTimeTag(this ref ParamChain prev, DateTime value, out ParamLink<ulong> discard)
    {
        discard = new(ref prev, value.Ticks2Ntp());
        return ref Unsafe.As<ParamLink<ulong>, ParamChain>(ref Unsafe.AsRef(in discard));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ParamChain WriteString(this ref ParamChain prev, string value, out ParamString discard)
    {
        discard = new(ref prev, value);
        ref ParamChain rChain = ref Unsafe.As<ParamString, ParamChain>(ref Unsafe.AsRef(in discard));
        return ref rChain;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyOscData(this ref ParamChain chain, Span<byte> destination)
    {
        ref byte byteRef = ref chain.GetPayload();
        OscType asType = chain.Type;
        switch (asType)
        {
            case OscType.Float:
            case OscType.Int:
                Unsafe.As<byte, uint>(ref byteRef).CopyTo(destination);
                break;

            case OscType.TimeTag:
                Unsafe.As<byte, ulong>(ref byteRef).CopyTo(destination);
                break;

            case OscType.Binary:
            case OscType.String:
                ref InlineString512 str = ref Unsafe.As<byte, InlineString512>(ref byteRef);

                str.CopyTo(destination);
                break;

            default:
                throw new NotImplementedException($"Got unsupported OSC type: {asType}");
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ref byte GetPayload(this ref ParamChain chain) => ref Unsafe.As<ParamChain, byte>(ref Unsafe.Add(ref chain, 1));
}
