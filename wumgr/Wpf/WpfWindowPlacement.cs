using System;
using System.Globalization;

namespace wumgr
{
    internal struct WpfWindowPlacement
    {
        public double Left;
        public double Top;
        public double Width;
        public double Height;
        public bool Maximized;

        public static bool TryCreate(string leftValue, string topValue, string widthValue, string heightValue, string stateValue, double minWidth, double minHeight, out WpfWindowPlacement placement)
        {
            placement = new WpfWindowPlacement();

            double left;
            double top;
            double width;
            double height;

            if (!TryParse(leftValue, out left) ||
                !TryParse(topValue, out top) ||
                !TryParse(widthValue, out width) ||
                !TryParse(heightValue, out height))
                return false;

            if (width < minWidth || height < minHeight)
                return false;

            placement.Left = left;
            placement.Top = top;
            placement.Width = width;
            placement.Height = height;
            placement.Maximized = stateValue.Equals("Maximized", StringComparison.CurrentCultureIgnoreCase);
            return true;
        }

        private static bool TryParse(string value, out double result)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }
    }
}
