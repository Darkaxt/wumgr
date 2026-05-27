namespace wumgr.Wpf
{
    internal static class WpfProgressValue
    {
        private const double MarqueeWidthRatio = 0.30;
        private const double MinimumMarqueeWidth = 48.0;
        private const double MaximumMarqueeWidth = 160.0;

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

        public static bool ShouldShowProgress(bool isBusy, int percent, bool isIndeterminate)
        {
            return isBusy;
        }

        public static string GetDisplayText(int percent, bool isIndeterminate)
        {
            if (isIndeterminate)
                return "Working...";
            if (percent <= 0)
                return "";
            return ((int)(NormalizePercent(percent) * 100)).ToString() + "%";
        }

        public static double GetMarqueeWidth(double trackWidth)
        {
            if (trackWidth <= 0)
                return 0.0;

            double width = trackWidth * MarqueeWidthRatio;
            if (width < MinimumMarqueeWidth)
                width = MinimumMarqueeWidth;
            if (width > MaximumMarqueeWidth)
                width = MaximumMarqueeWidth;
            if (width > trackWidth)
                width = trackWidth;
            return width;
        }

        public static double GetMarqueeLeft(double trackWidth, double marqueeWidth, double phase)
        {
            if (trackWidth <= 0 || marqueeWidth <= 0 || marqueeWidth >= trackWidth)
                return 0.0;

            if (phase <= 0)
                return 0.0;
            if (phase >= 1)
                return trackWidth - marqueeWidth;

            return (trackWidth - marqueeWidth) * phase;
        }
    }
}
