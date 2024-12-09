using System.Runtime.InteropServices;
using System.Text;

namespace KoboldOSC.Messages;

/// <summary>
/// Represents a fixed-size string held inline along with its length.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct InlineString512
{
    /// <summary>
    /// The fixed size of this string.
    /// </summary>
    public const int SIZE = 512;

    /// <summary>
    /// The length of this string, including the terminating null byte.
    /// </summary>
    public int Length;

    /// <summary>
    /// Copies the contents of this inline string into a managed string.
    /// </summary>
    public unsafe readonly string? String
    {
        get
        {
            fixed (byte* dataPtr = data)
                return Encoding.UTF8.GetString(dataPtr, Length);
        }
    }

    internal unsafe fixed byte data[SIZE];

    /// <summary>
    /// Copies a managed string into an inline buffer of UTF8 bytes.
    /// </summary>
    /// <param name="str">The string to copy.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input string is longer than 512 UTF8 bytes, including the null terminator.</exception>
    public InlineString512(string str)
    {
        if (Length > SIZE)
            throw new ArgumentOutOfRangeException(nameof(str), $"Input string was over {SIZE} bytes! Input length: {Length}");
        
        unsafe
        {
            fixed (char* charPtr = str)
            fixed (byte* dataPtr = data)
                Length = Encoding.UTF8.GetBytes(charPtr, str.Length, dataPtr, SIZE) + 1;
        }
    }

    public static implicit operator InlineString512(string other) => new(other);
}