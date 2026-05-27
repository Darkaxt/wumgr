using System.Globalization;

namespace wumgr.Wpf
{
    internal struct WpfStatusPaneHeight
    {
        public const double DefaultHeight = 140.0;
        public const double MinHeight = 96.0;
        public const double MaxHeight = 420.0;

        public double Height;

        public static bool TryCreate(string value, out WpfStatusPaneHeight height)
        {
            height = new WpfStatusPaneHeight();

            double parsed;
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                return false;

            if (parsed < MinHeight || parsed > MaxHeight)
                return false;

            height.Height = parsed;
            return true;
        }

        public static double Coerce(double height)
        {
            if (height < MinHeight)
                return MinHeight;

            if (height > MaxHeight)
                return MaxHeight;

            return height;
        }
    }
}
