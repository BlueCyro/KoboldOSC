using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using KoboldOSC.Structs;

namespace KoboldOSC;


public static class KoboldOSC
{
    public const byte MARKER = (byte)',';


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetOSCIdentifier<T>(this T val)
    {
        return val switch
        {
            int     => (byte)'i',
            float   => (byte)'f',
            char    => (byte)'s',
            byte    => (byte)'b',
            ulong   => (byte)'t',
            _ => throw new NotSupportedException($"OSC type unsupported: {typeof(T)}"),
        };
    }


    // public static int GetOscPacketLength(ref OSCString path, params Span<IOSCParameter> parameters)
    // {
    //     int pathBytes = path.AlignedLength;
    //     int idTableBytes = (parameters.Length + 1).Ensure4Byte();
    //     int count = pathBytes + idTableBytes;

    //     int paramBytes = 0;
    //     for (int i = 0; i < parameters.Length; i++)
    //         paramBytes += parameters[i].Length;

    //     return count + paramBytes.Ensure4Byte();
    // }


    // public static void SerializeOscPacket(ref OSCString path, Span<byte> destination, params Span<IOSCParameter> parameters)
    // {
    //     int pathBytes = path.AlignedLength;
    //     int idTableBytes = (parameters.Length + 1).Ensure4Byte();

    //     // Slice the first part of the OSC packet and copy the path to it
    //     path.CopyTo(destination[..pathBytes]);

    //     int realIndex = pathBytes;

    //     Span<byte> idSlice = destination.Slice(realIndex, idTableBytes);
    //     realIndex += idTableBytes;

    //     idSlice[0] = MARKER;

    //     for (int i = 0; i < parameters.Length; i++)
    //         idSlice[i + 1] = parameters[i].ID;


    //     Span<byte> paramSlice = destination[realIndex..];

    //     int curIndex = 0;
    //     for (int i = 0; i < parameters.Length; i++)
    //     {
    //         var curParam = parameters[i];
    //         curParam.CopyTo(paramSlice[curIndex..]);

    //         curIndex += curParam.AlignedLength;
    //     }
    // }
}
