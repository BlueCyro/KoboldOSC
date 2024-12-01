namespace KoboldOSC.Structs;

public interface IOSCParameter
{
    public byte ID { get; }
    public int AlignedLength { get; }
    public int Length { get; }
    public void CopyTo(Span<byte> bytes);
}
