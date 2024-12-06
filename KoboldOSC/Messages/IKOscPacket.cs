namespace KoboldOSC.Messages;


/// <summary>
/// Common interface for a given OSC packet.
/// </summary>
public interface IKOscPacket : IDisposable
{
    /// <summary>
    /// The number of bytes this OSC packet is expected to take up upon serialization. Will always be 4-byte aligned, rounded up.
    /// </summary>
    int ByteLength { get; }

    /// <summary>
    /// Serializes this OSC packet to a byte span of equal or greater length.
    /// </summary>
    /// <param name="destination">The byte span to write this OSC packet to.</param>
    void Serialize(Span<byte> destination);
}