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

        public static double GetFillWidth(double trackWidth, int percent, bool isIndeterminate)
        {
            if (trackWidth <= 0)
                return 0.0;
            if (isIndeterminate)
                return trackWidth;
            return trackWidth * NormalizePercent(percent);
        }
    }
}
