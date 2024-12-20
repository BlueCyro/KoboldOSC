using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using KoboldOSC.Helpers;

namespace KoboldOSC.Structs;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

/// <summary>
/// Represents a bundle of OSC messages.
/// </summary>
public readonly ref struct KOscBundleS
{
    private static readonly byte[] bundleHeader = Encoding.UTF8.GetBytes("#bundle");


    public unsafe KOscBundleS(params Span<OscMessageRef> msgs) // Evil ref params input.
    {
        int count = 0;
        int i = msgs.Length;
        while (i > 0)
            count += 4 + msgs[--i].msg->ByteLength; // +4 bytes for the length of each OSC parameter

        ByteLength = 16 + count; // 16 = #bundle/0 + timetag
        MessageCount = msgs.Length;

        // Super mega dummy stupid evil nonsense.
        messages = new Span<OscMessageRef>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(msgs)), msgs.Length);
    }


    public readonly int ByteLength;
    public readonly int MessageCount;



    readonly unsafe Span<OscMessageRef> messages;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe void Serialize(Span<byte> destination)
    {
        int i = MessageCount;

        int destinationOffset = ByteLength;

        bundleHeader.AsSpan().CopyTo(destination);
        DateTime.Now.Ticks2Ntp().CopyBytesTo(destination, 8);
        KOscMessageS* curMsg;
        int curLength;
        Span<byte> curDestPos;
        while (i > 0)
        {
            curMsg = messages[--i].msg;
            curLength = curMsg->ByteLength;
            destinationOffset -= 4 + curLength;
            curDestPos = destination[destinationOffset..];

            curLength.CopyBytesTo(curDestPos);
            curMsg->Serialize(destination[(destinationOffset + 4)..]);
        }
    }
}



/*
 * This is probably one of the more heinous crimes in this library. This is so wildly unsafe that I 
 * would probably ask you to not get any ideas from this one. You're gonna shoot yourself in the foot 
 * unless you REALLY know what you're doing. This is a nasty nasty hack to do variable-length function
 * arguments that are passed by reference instead of by value.
 * 
 * The only reason this works is because KOscMessage is a ref struct. Ref structs can only exist on the stack
 * and thus you can get a pointer to them without worrying about it moving around.
 *
 * When an OSC message is converted to this type, it's passed by 'in', which is summarily ripped up and interpreted
 * as a read/write reference, and it's pointer is encapsulated in this struct. It can then be passed by value - or
 * this use-case - passed by value as part of a variable-parameter input to the OSC bundle constructor.
 *
 * Really, please don't do this because it can break in so many ways. If the struct leaks into the heap or the type
 * you're getting a pointer to isn't guaranteed to either be pinned or located on the stack at all times, you're gonna
 * have a lot of fun swiss-cheesing your memory.
*/

public readonly struct OscMessageRef
{
    internal readonly unsafe KOscMessageS* msg;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe implicit operator OscMessageRef(in KOscMessageS other)
    {
        nint ptr = (nint)Unsafe.AsPointer(ref Unsafe.AsRef(in other));
        return Unsafe.As<nint, OscMessageRef>(ref ptr);
    }
}

#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type