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
        public WpfActionButtonKind Kind;
        public string ResourceName;

        public WpfActionButtonSpec(WpfActionButtonKind kind, string resourceName)
        {
            Kind = kind;
            ResourceName = resourceName;
        }

        public static WpfActionButtonSpec[] CreateDefault()
        {
            return new[]
            {
                new WpfActionButtonSpec(WpfActionButtonKind.Refresh, "icons8_refresh_32"),
                new WpfActionButtonSpec(WpfActionButtonKind.Search, "icons8_available_updates_32"),
                new WpfActionButtonSpec(WpfActionButtonKind.Download, "icons8_downloading_updates_32"),
                new WpfActionButtonSpec(WpfActionButtonKind.Install, "icons8_software_installer_32"),
                new WpfActionButtonSpec(WpfActionButtonKind.Uninstall, "icons8_trash_32"),
                new WpfActionButtonSpec(WpfActionButtonKind.Hide, "icons8_hide_32"),
                new WpfActionButtonSpec(WpfActionButtonKind.GetLinks, "icons8_link_32"),
                new WpfActionButtonSpec(WpfActionButtonKind.Cancel, "icons8_cancel_32")
            };
        }
    }
}
