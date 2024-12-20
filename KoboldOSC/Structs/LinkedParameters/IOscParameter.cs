using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using KoboldOSC.Messages;

namespace KoboldOSC.Parameters;



/*
 * So, the interface is kind of a just a lie in this library. I've mostly used it
 * as a way to keep the fields standard, but it's actually not used anywhere. Instead,
 * the 'OscParam' struct is used as a sort of interface by sharing the same parameters
 * up to a certain point. This means that any other strongly-typed parameter struct can be
 * represented by this one ""generically"" when it's reinterpreted.
*/



/// <summary>
/// An interface to interact with a given OSC parameter.
/// </summary>
public interface IOscParameter
{
    /// <summary>
    /// The type that this parameter represents.
    /// </summary>
    OscType Type { get; }

    /// <summary>
    /// The length of this parameter, in bytes.
    /// </summary>
    int ByteLength { get; }
}


/*
 * This struct can represent any other typed OSC parameter due to being the same size, and
 * having filler types to signify that data is present so it doesn't get ereased from by-value copies.
*/


/// <summary>
/// Represents an arbitary OSC parameter.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 32, Pack = 64)]
public readonly struct OscParam : IOscParameter
{
    /// <inheritdoc />
    public readonly OscType Type { get; }

    /// <inheritdoc />
    public readonly int ByteLength { get; }
    

    /*
     * This function pointer looks weird, but it's got a good reason. If this were an action, each
     * call to it would be virtualized (looked up, checked to see if it actually exists, etc.). In 
     * most cases, this wouldn't really matter at all.
     *
     * In this library though, we're going for raw speed.
     *
     * Calling a pointer to a static function ends up being a little faster than calling a delegate
     * and only marginally slower than calling the method directly. Adding up the benchmarks means
     * that this shaves a few nanoseconds off on-average.
     * 
     * This also means that there doesn't have to be any branching logic to determine the type of
     * message currently being serialized, this pointer can just be called and the static method
     * will handle the rest.
    */

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly unsafe delegate* <ref OscParam, Span<byte>, int, void> copyToDangerous;


    /*
     * This feels pretty grody, but when instances of this struct are occasionally passed around
     * by value, the compiler needs to know that there's actually space here being used by something.
     *
     * This means that whatever payload exists here - be it a simple integer or a managed reference - the
     * data will remain and be passed intact as long as it isn't touched.
     *
     * It also happens to neatly pad the struct with actual data up to 32 bytes as well, so that's cool.
    */
    private readonly ulong filler8bytes1;
    private readonly ulong filler8bytes2;


    /// <summary>
    /// Copies an OSC parameter to a destination buffer with an optional offset.
    /// </summary>
    /// <param name="dest">The destination buffer to copy the parameter's bytes to.</param>
    /// <param name="offset">An optional offset within the destination buffer to write the parameter's bytes at.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe void CopyTo(Span<byte> dest, int offset = 0) => copyToDangerous(ref Unsafe.AsRef(in this), dest, offset);




    /*
     * These allow implicit conversions from any supported OSC type into a generalized parameter container.
     * This is specifically why the ulong padding defined above is important, because even though these are
     * inlined by the JIT, the compiler otherwise wouldn't know that the returned struct has any extra data
     * associated with it. (Or at least, I didn't want to take the chance that it might not know.)
     *
     * This allows a variety of types to be represented by a single one generically without
     * even touching generics and thus virtual dispatch and boxing. This is what allows you to
     * use the convenient syntax of:
     *
     *   new KOscParam("/path/to/osc/endpoint", [12, 99f, "cool string", DateTime.Now])
     * 
     * - to create your messages. It even keeps it all on the stack too.
     *
     * Nifty, right?
    */

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator OscParam(int other)
    {
        IntParam param = new(other);
        return Unsafe.As<IntParam, OscParam>(ref param);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator OscParam(float other)
    {
        FloatParam param = new(other);
        return Unsafe.As<FloatParam, OscParam>(ref param);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator OscParam(DateTime other)
    {
        TimeParam param = new(other);
        return Unsafe.As<TimeParam, OscParam>(ref param);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator OscParam(ReadOnlySpan<char> other)
    {
        StringParam param = new(other);
        return Unsafe.As<StringParam, OscParam>(ref param);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator OscParam(string other)
    {
        StringParam param = new(other);
        return Unsafe.As<StringParam, OscParam>(ref param);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator OscParam(ReadOnlySpan<byte> other)
    {
        BinaryParam param = new(other);
        return Unsafe.As<BinaryParam, OscParam>(ref param);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator OscParam(byte[] other)
    {
        BinaryParam param = new(other);
        return Unsafe.As<BinaryParam, OscParam>(ref param);
    }
}