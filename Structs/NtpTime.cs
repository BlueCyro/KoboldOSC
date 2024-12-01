namespace KoboldOSC.Structs;

public readonly struct NtpTime
{
    public static readonly DateTime Base = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public readonly ulong Value;
    public static NtpTime Now => new(DateTime.Now);


    public NtpTime(DateTime time)
    {
        TimeSpan span = time.Subtract(Base);

        double seconds = span.TotalSeconds;
        uint uintSeconds = (uint)seconds;

        double milliseconds = span.TotalMilliseconds - ((double)uintSeconds * 1000);
        double fraction = (milliseconds / 1000) * ((double)uint.MaxValue);

        Value = (((ulong)uintSeconds & 0xFFFFFFFF) << 32) | ((ulong)fraction & 0xFFFFFFFF);
    }


    public static implicit operator ulong(NtpTime other) => other.Value;
    // public static implicit operator NtpTime(ulong other) => new(other);
    
    // public static implicit operator DateTime(NtpTime other) => other.Value;
    public static implicit operator NtpTime(DateTime other) => new(other);
}