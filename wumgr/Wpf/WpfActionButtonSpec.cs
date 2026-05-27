namespace wumgr.Wpf
{
    internal enum WpfActionButtonKind
    {
        Refresh,
        Search,
        Download,
        Install,
        Uninstall,
        Hide,
        GetLinks,
        Cancel
    }

    internal struct WpfActionButtonSpec
    {
        public const string IconFontFamily = "Segoe Fluent Icons, Segoe MDL2 Assets";

        public WpfActionButtonKind Kind;
        public string Glyph;

        public WpfActionButtonSpec(WpfActionButtonKind kind, string glyph)
        {
            Kind = kind;
            Glyph = glyph;
        }

        public static WpfActionButtonSpec[] CreateDefault()
        {
            return new[]
            {
                new WpfActionButtonSpec(WpfActionButtonKind.Refresh, "\uE72C"),
                new WpfActionButtonSpec(WpfActionButtonKind.Search, "\uE721"),
                new WpfActionButtonSpec(WpfActionButtonKind.Download, "\uE896"),
                new WpfActionButtonSpec(WpfActionButtonKind.Install, "\uE7B8"),
                new WpfActionButtonSpec(WpfActionButtonKind.Uninstall, "\uE74D"),
                new WpfActionButtonSpec(WpfActionButtonKind.Hide, "\uE8C5"),
                new WpfActionButtonSpec(WpfActionButtonKind.GetLinks, "\uE71B"),
                new WpfActionButtonSpec(WpfActionButtonKind.Cancel, "\uE711")
            };
        }
    }
}
