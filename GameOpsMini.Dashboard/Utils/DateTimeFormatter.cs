namespace GameOpsMini.Dashboard.Utils;

public static class DateTimeFormatter
{
    private static readonly TimeZoneInfo KoreaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");

    public static string ToKoreaTimeString(DateTime value)
    {
        if (value == DateTime.MinValue)
        {
            return "-";
        }

        var utcValue = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            DateTimeKind.Unspecified =>
                DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => value
        };

        var koreaTime = TimeZoneInfo.ConvertTimeFromUtc(
            utcValue,
            KoreaTimeZone);

        return koreaTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
}