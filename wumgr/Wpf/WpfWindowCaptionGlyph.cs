namespace wumgr.Wpf
{
    internal static class WpfWindowCaptionGlyph
    {
        public const string Minimize = "\uE921";
        public const string Maximize = "\uE922";
        public const string Restore = "\uE923";
        public const string Close = "\uE8BB";

        public static string GetMaximizeRestoreGlyph(bool isMaximized)
        {
            return isMaximized ? Restore : Maximize;
        }
    }
}
