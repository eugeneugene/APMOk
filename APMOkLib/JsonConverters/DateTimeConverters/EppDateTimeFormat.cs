namespace APMOkLib.JsonConverters.DateTimeConverters;

public sealed class EppDateTimeFormat : IDateTimeFormat
{
    public string Format { get => "yyyy-MM-dd\\THH:mm:sszzz"; }
}
