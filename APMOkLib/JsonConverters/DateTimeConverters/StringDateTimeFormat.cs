namespace APMOkLib.JsonConverters.DateTimeConverters;

public sealed class StringDateTimeFormat : IDateTimeFormat
{
    public string Format => "yyyy-MM-dd\\THH:mm:ss.fff";
}
