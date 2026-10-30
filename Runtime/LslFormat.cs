namespace KaranGrover.Lsl
{
    /// <summary>Channel format of a stream (same values as liblsl's channel_format_t).</summary>
    public enum LslFormat
    {
        Float32 = 1,
        Double64 = 2,
        String = 3,
        Int32 = 4,
        Int16 = 5,
        Int8 = 6,
        Int64 = 7,
    }
}
