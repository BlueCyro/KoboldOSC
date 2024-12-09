using System.Buffers;
using System.Runtime.CompilerServices;
using KoboldOSC.Helpers;
using KoboldOSC.Messages;
using KoboldOSC.Structs;

namespace KoboldOSC.Helpers;

public static class KOSCMessageHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ref RefChain End(this ref RefChain prev, ref KOscMessageS message)
    {
        message.refChain = (RefChain*)Unsafe.AsPointer(ref prev);
        return ref prev;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref RefChain WriteInt(this ref RefChain prev, int value, out LinkedRef<int> discard)
    {
        discard = new(ref prev, value);
        return ref Unsafe.As<LinkedRef<int>, RefChain>(ref Unsafe.AsRef(in discard));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref RefChain WriteFloat(this ref RefChain prev, float value, out LinkedRef<float> discard)
    {
        discard = new(ref prev, value);
        return ref Unsafe.As<LinkedRef<float>, RefChain>(ref Unsafe.AsRef(in discard));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref RefChain WriteTimeTag(this ref RefChain prev, DateTime value, out LinkedRef<ulong> discard)
    {
        discard = new(ref prev, value.Ticks2Ntp());
        return ref Unsafe.As<LinkedRef<ulong>, RefChain>(ref Unsafe.AsRef(in discard));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref RefChain WriteString(this ref RefChain prev, string value, out LinkedString discard)
    {
        discard = new(ref prev, value);
        ref RefChain rChain = ref Unsafe.As<LinkedString, RefChain>(ref Unsafe.AsRef(in discard));
        return ref rChain;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyOscData(this ref RefChain chain, Span<byte> destination)
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
    internal static ref byte GetPayload(this ref RefChain chain) => ref Unsafe.As<RefChain, byte>(ref Unsafe.Add(ref chain, 1));
}
