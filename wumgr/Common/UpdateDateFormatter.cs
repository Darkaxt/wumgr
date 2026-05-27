using System;
using System.Globalization;

internal static class UpdateDateFormatter
{
    public static string FormatForDisplay(DateTime date)
    {
        return FormatForDisplay(date, CultureInfo.CurrentUICulture);
    }

    public static string FormatForDisplay(DateTime date, CultureInfo culture)
    {
        if (date == DateTime.MinValue)
            return "";

        if (culture == null)
            culture = CultureInfo.CurrentUICulture;

        return date.ToString(culture.DateTimeFormat.ShortDatePattern, culture);
    }

    public static string SerializeForCache(DateTime date)
    {
        if (date == DateTime.MinValue)
            return "";

        return date.ToString("o", CultureInfo.InvariantCulture);
    }

    public static bool TryDeserializeFromCache(string value, CultureInfo legacyCulture, out DateTime date)
    {
        date = DateTime.MinValue;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (DateTime.TryParseExact(value, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date))
            return true;

        if (legacyCulture == null)
            legacyCulture = CultureInfo.CurrentUICulture;

        if (DateTime.TryParse(value, legacyCulture, DateTimeStyles.AssumeLocal, out date))
            return true;

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out date);
    }
}
