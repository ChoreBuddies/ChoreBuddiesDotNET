namespace ChoreBuddies.Frontend.Utilities;

public static class DateTimeExtensions
{
    public static DateTime AtEndOfDay(this DateTime dt)
    {
        return new DateTime(
            dt.Year,
            dt.Month,
            dt.Day,
            23, 59, 59,
            dt.Kind
        );
    }
}
