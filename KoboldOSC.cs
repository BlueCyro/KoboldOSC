using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using KoboldOSC.Structs;

namespace KoboldOSC;


public static class KoboldOSC
{
    public const byte MARKER = (byte)',';


    public static byte GetOSCIdentifier<T>(this T val) where T : unmanaged
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Ensure4Byte(this int num) => ((num + 3) >> 2) << 2;


    public static int GetOscPacketLength(ref OSCString path, params Span<IOSCParameter> parameters)
    {
        int pathBytes = path.AlignedLength;
        int idTableBytes = (parameters.Length + 1).Ensure4Byte();
        int count = pathBytes + idTableBytes;

        int paramBytes = 0;
        for (int i = 0; i < parameters.Length; i++)
            paramBytes += parameters[i].Length;

        return count + paramBytes.Ensure4Byte();
    }


    public static void SerializeOscPacket(ref OSCString path, Span<byte> destination, params Span<IOSCParameter> parameters)
    {
        int pathBytes = path.AlignedLength;
        int idTableBytes = (parameters.Length + 1).Ensure4Byte();

        // Slice the first part of the OSC packet and copy the path to it
        path.CopyTo(destination[..pathBytes]);

        int realIndex = pathBytes;

        Span<byte> idSlice = destination.Slice(realIndex, idTableBytes);
        realIndex += idTableBytes;

        idSlice[0] = MARKER;

        for (int i = 0; i < parameters.Length; i++)
            idSlice[i + 1] = parameters[i].ID;


        Span<byte> paramSlice = destination[realIndex..];

        int curIndex = 0;
        for (int i = 0; i < parameters.Length; i++)
        {
            var curParam = parameters[i];
            curParam.CopyTo(paramSlice[curIndex..]);

            curIndex += curParam.AlignedLength;
        }
    }
}


public class KOscSender : IDisposable
{
    private Socket socket;

    public KOscSender(IPEndPoint ep)
    {
        socket = new(ep.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
    }





    public void Dispose()
    {
        GC.SuppressFinalize(this);

        socket.Dispose();
    }
}


public class KOscMessage : IDisposable
{
    public int ByteLength => Path.AlignedLength + IdLengthAligned + valuesLength;


    public readonly OSCString Path;

    private byte[] idTable = [];
    private byte[] values = [];

    private int IdLengthAligned => (idLength + 1).Ensure4Byte();
    private int idLength;

    private int valuesLength; // Implicitly aligned


    


    public KOscMessage(string path)
    {
        Path = path;
        idTable = ArrayPool<byte>.Shared.Rent(1);
        Array.Clear(idTable);
        values = ArrayPool<byte>.Shared.Rent(0);
        Array.Clear(values);
        idTable[0] = (byte)',';
        idLength++;
    }


    public void Serialize(Span<byte> destination)
    {
        if (destination.Length < ByteLength)
            throw new ArgumentOutOfRangeException(nameof(destination), "Span destination isn't large enough to hold this message.");
        
        int pathSizeAligned = Path.AlignedLength;
        Span<byte> pathSlice = destination[..pathSizeAligned];
        Span<byte> idSlice = destination.Slice(pathSizeAligned, IdLengthAligned);
        Span<byte> valueSlice = destination.Slice(pathSizeAligned + IdLengthAligned, valuesLength);

        Path.CopyTo(pathSlice);
        ((Span<byte>)idTable)[..idLength].CopyTo(idSlice);
        ((Span<byte>)values)[..valuesLength].CopyTo(valueSlice);
    }



    public void WriteInt(params Span<OSCInt> value)
    {
        int width = value[0].AlignedLength;
        WriteID(in value[0], value.Length);

        Span<byte> space = AllocateValue(width * value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            value[i].CopyTo(space);
            space = space[width..];
        }
    }

    public void WriteFloat(params Span<OSCFloat> value)
    {
        int width = value[0].AlignedLength;
        WriteID(in value[0], value.Length);

        Span<byte> space = AllocateValue(width * value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            value[i].CopyTo(space);
            space = space[width..];
        }
    }

    public void WriteTime(params Span<OSCTimeTag> value)
    {
        int width = value[0].AlignedLength;
        WriteID(in value[0], value.Length);

        Span<byte> space = AllocateValue(width * value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            value[i].CopyTo(space);
            space = space[width..];
        }
    }

    public void WriteString(params Span<OSCString> str)
    {
        WriteID(in str[0], str.Length);
        int width = 0;

        for (int i = 0; i < str.Length; i++)
            width += str[i].AlignedLength;

        Span<byte> space = AllocateValue(width);

        for (int i = 0; i < str.Length; i++)
        {
            str[i].CopyTo(space);
            space = space[str[i].AlignedLength..];
        }
    }


    private Span<byte> AllocateValue(int requiredSpace)
    {
        int newLength = valuesLength + requiredSpace;
        byte[] temp = ArrayPool<byte>.Shared.Rent(newLength);
        Array.Clear(temp);

        Span<byte> valueSpan = values;
        Span<byte> tempSpan = temp;
        valueSpan.CopyTo(tempSpan);

        ArrayPool<byte>.Shared.Return(values);

        Span<byte> valueSection = tempSpan.Slice(valuesLength, requiredSpace);
        values = temp;
        valuesLength = newLength;

        return valueSection;
    }



    private void WriteID<T>(in T value, int count = 1) where T : unmanaged, IOSCParameter
    {
        byte[] temp = ArrayPool<byte>.Shared.Rent(idLength + count);
        Array.Clear(temp);

        Span<byte> idTableSpan = idTable;
        Span<byte> tempSpan = temp;
        idTableSpan.CopyTo(tempSpan);

        ArrayPool<byte>.Shared.Return(idTable);

        for (int i = 0; i < count; i++)
            temp[idLength++] = value.ID;

        idTable = temp;
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);

        ArrayPool<byte>.Shared.Return(values);
        ArrayPool<byte>.Shared.Return(idTable);
    }
}



public class KOscBundle : IDisposable
{
    public int ByteLength
    {
        get
        {
            int count = bundleHeader.AlignedLength + Unsafe.SizeOf<ulong>();
            for (int i = 0; i < bundledLength; i++)
                count += 4 + bundled[i].ByteLength;
            
            return count;
        }
    }

    private static OSCString bundleHeader = "#bundle";
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

        int curIndex = bundleHeader.AlignedLength;
        Span<byte> bundleHeaderSlice = destination[..curIndex];
        bundleHeader.CopyTo(bundleHeaderSlice);

        Span<byte> timeTagSlice = destination.Slice(curIndex, Unsafe.SizeOf<ulong>());
        OSCTimeTag curTime = new(DateTime.Now);
        curTime.CopyTo(timeTagSlice);
        curIndex += curTime.AlignedLength;

        for (int i = 0; i < bundledLength; i++)
        {
            KOscMessage curMsg = bundled[i];
            Span<byte> curSizeSlice = destination.Slice(curIndex, Unsafe.SizeOf<int>());
            ((OSCInt)curMsg.ByteLength).CopyTo(curSizeSlice);
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
