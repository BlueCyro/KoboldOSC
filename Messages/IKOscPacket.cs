namespace KoboldOSC.Messages;

public interface IKOscPacket : IDisposable
{
    int ByteLength { get; }

    void Serialize(Span<byte> destination);
}