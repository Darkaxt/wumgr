using System;

namespace wumgr.Wpf
{
    internal static class WpfUpdateFilter
    {
        public static bool Matches(WpfUpdateRow row, string filter)
        {
            if (row == null)
                return false;

            if (string.IsNullOrWhiteSpace(filter))
                return true;

            string[] values = new[]
            {
                row.Title,
                row.Category,
                row.KB,
                row.Date,
                row.Size,
                row.State
            };

            foreach (string value in values)
            {
                if (!string.IsNullOrEmpty(value) && value.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}
