namespace wumgr.Wpf
{
    internal static class WpfProgressValue
    {
        public static double NormalizePercent(int percent)
        {
            if (percent <= 0)
                return 0.0;
            if (percent >= 100)
                return 1.0;
            return percent / 100.0;
        }
    }
}
