using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using KoboldOSC.Helpers;
using KoboldOSC.Structs;

[assembly: InternalsVisibleTo("KoboldOSC.Tests")]

namespace KoboldOSC.Messages;


/// <summary>
/// Represents an OSC message.
/// </summary>
[Obsolete("Soon to be replaced by zero-allocation message types.")]
public class KOscMessage : IDisposable, IKOscPacket
{
    /// <inheritdoc />
    public int ByteLength => Path.GetAlignedLength() + IdLengthAligned + valuesLength;

    /// <summary>
    /// The path of this OSC message.
    /// </summary>
    public readonly string Path;

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
        idTable[0] = 44; // The "," character
        idLength++;
    }


    /// <inheritdoc />
    public void Serialize(Span<byte> destination)
    {
        int pathSizeAligned = Path.GetAlignedLength();
        Span<byte> pathSlice = destination[..pathSizeAligned];
        Span<byte> idSlice = destination.Slice(pathSizeAligned, IdLengthAligned);
        Span<byte> valueSlice = destination.Slice(pathSizeAligned + IdLengthAligned, valuesLength);

        Path.CopyBytesTo(pathSlice);
        ((Span<byte>)idTable)[..idLength].CopyTo(idSlice);
        ((Span<byte>)values)[..valuesLength].CopyTo(valueSlice);
    }



    public void WriteInt(params Span<int> value)
    {
        int width = Unsafe.SizeOf<int>();
        WriteID(in value[0], value.Length);

        Span<byte> space = AllocateValue(width * value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            value[i].CopyBytesTo(space);
            space = space[width..];
        }
    }



    public void WriteFloat(params Span<float> value)
    {
        int width = Unsafe.SizeOf<int>();
        WriteID(in value[0], value.Length);

        Span<byte> space = AllocateValue(width * value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            value[i].CopyBytesTo(space);
            space = space[width..];
        }
    }



    public void WriteTime(params Span<ulong> value)
    {
        int width = Unsafe.SizeOf<ulong>();
        WriteID(in value[0], value.Length);

        Span<byte> space = AllocateValue(width * value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            value[i].CopyBytesTo(space);
            space = space[width..];
        }
    }



    public void WriteString(params Span<string> str)
    {
        WriteID(in str[0], str.Length);
        int width = 0;

        for (int i = 0; i < str.Length; i++)
            width += str[i].GetAlignedLength();

        Span<byte> space = AllocateValue(width);

        for (int i = 0; i < str.Length; i++)
        {
            str[i].CopyBytesTo(space);
            space = space[str[i].GetAlignedLength()..];
        }
    }



    private Span<byte> AllocateValue(int requiredBytes)
    {
        int newLength = valuesLength + requiredBytes;
        byte[] temp = ArrayPool<byte>.Shared.Rent(newLength);
        Array.Clear(temp);

        Span<byte> valueSpan = values;
        Span<byte> tempSpan = temp;
        valueSpan.CopyTo(tempSpan);

        ArrayPool<byte>.Shared.Return(values);

        Span<byte> valueSection = tempSpan.Slice(valuesLength, requiredBytes);
        values = temp;
        valuesLength = newLength;

        return valueSection;
    }



    private void WriteID<T>(in T value, int count = 1)
    {
        byte[] temp = ArrayPool<byte>.Shared.Rent(idLength + count);
        Array.Clear(temp);

        Span<byte> idTableSpan = idTable;
        Span<byte> tempSpan = temp;
        idTableSpan.CopyTo(tempSpan);

        ArrayPool<byte>.Shared.Return(idTable);

        for (int i = 0; i < count; i++)
            temp[idLength++] = value.GetOSCIdentifier();

        idTable = temp;
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);

        ArrayPool<byte>.Shared.Return(values);
        ArrayPool<byte>.Shared.Return(idTable);
    }
}


public enum OscType : int
{
    Unknown =  0,
    Int     = 'i',
    Float   = 'f',
    String  = 's',
    Binary  = 'b',
    TimeTag = 't',
}
