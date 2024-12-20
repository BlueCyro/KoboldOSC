using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;
using KoboldOSC.Helpers;
using KoboldOSC.Messages;
using KoboldOSC.Parameters;

namespace KoboldOSC.Structs;

/// <summary>
/// Represents an OSC message.
/// </summary>
public ref struct KOscMessageS
{
    /// <summary>
    /// The path that this OSC message is destined for.
    /// </summary>
    public readonly ReadOnlySpan<char> Path;

    /// <summary>
    /// The number of bytes the path string is expected to take up.
    /// </summary>
    public readonly int PathBytes;


    /// <summary>
    /// The total byte length of this OSC message.
    /// </summary>
    public readonly int ByteLength;

    /// <summary>
    /// How many bytes the ID table takes up (including the ',' delimiter)
    /// </summary>
    public readonly int IdTableBytes;


    /// <summary>
    /// How many bytes this OSC message's parameters are expected to take up.
    /// </summary>
    public readonly int ParamBytes;

    /// <summary>
    /// The number of parameters in this OSC message.
    /// </summary>
    public readonly int ParamCount;

    public readonly Span<OscParam> Parameters;

    /// <param name="path">The path this OSC message is destined for.</param>
    public KOscMessageS(string path, Span<OscParam> oscParams)
    {
        Path = path;
        PathBytes = path.GetAlignedLength();

        Parameters = oscParams;
        ParamCount = oscParams.Length;
        IdTableBytes = (2 + ParamCount).Ensure4Byte(); // +2 because of the ',' character and the trailing null byte

        int i = ParamCount;
        int count = 0;
        while (i > 0)
            count += oscParams[--i].ByteLength;

        ParamBytes = count;
        ByteLength = PathBytes + IdTableBytes + ParamBytes;
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public unsafe void AppendParam(ref OscParam oscParam)
    // {
    //     ParamBytes += oscParam.ByteLength;
    //     oscParam.next = paramList;
    //     paramList = (OscParam*)Unsafe.AsPointer(ref oscParam);
    //     ParamCount++;
    // }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Serialize(Span<byte> buffer)
    {
        // Localize variables to slightly aid cache locality in this circumstance.

        int pathBytes     = PathBytes;          // Get the path bytes.
        int curParamIndex = ByteLength;         // Get the byte length of this OSC message
        int typesOffset   = pathBytes + 1;      // Offset to the type table (after the ',' mark).
        int paramsLength  = Parameters.Length;  // Get the parameter span length.



        Encoding.UTF8.GetBytes(Path, buffer);   // Write the path to the buffer.
        buffer[pathBytes] = 44;                 // Write the ',' character after the path to denote the type table.



        // Write the parameters into the parameter portion of the buffer backwards.
        int i = paramsLength;
        while (i > 0)
        {
            ref OscParam curParam = ref Parameters[--i];          // Decrement I and get a parameter from the span.
            byte curType          = (byte)curParam.Type;          // Get the type as a byte.
            int  curByteLength    = curParam.ByteLength;          // Get the length of the current parameter.


            buffer[typesOffset + i] = curType;                    // Write the type to the type table.
            curParamIndex -= curByteLength;                       // Decrement the param offset by the current parameter's byte length.
            curParam.CopyTo(buffer, curParamIndex);               // Write the parameter to the buffer at the given offset.
        }
    }
}
