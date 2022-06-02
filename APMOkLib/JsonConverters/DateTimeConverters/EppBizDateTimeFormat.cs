namespace APMOkLib.JsonConverters.DateTimeConverters;

public sealed class EppBizDateTimeFormat : IDateTimeFormat
{
    public string Format { get => "yyyy-MM-dd\\THH:mm:ss.fffzzz"; }
}
