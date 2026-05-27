namespace wumgr
{
    internal enum WpfUpdateListKind
    {
        Pending,
        Installed,
        Hidden,
        History
    }

    internal struct WpfActionState
    {
        public bool CanSearch;
        public bool CanDownload;
        public bool CanInstall;
        public bool CanUninstall;
        public bool CanHide;
        public bool CanGetLinks;
        public bool CanCancel;

        public static WpfActionState Create(bool hasSelection, bool isAdministrator, bool agentActive, bool agentBusy, bool agentValid, bool manualMode, WpfUpdateListKind currentList)
        {
            bool enable = agentActive && !agentBusy;
            bool validForDownloadOrInstall = agentValid || manualMode;

            WpfActionState state = new WpfActionState();
            state.CanSearch = enable;
            state.CanDownload = hasSelection && enable && validForDownloadOrInstall && currentList == WpfUpdateListKind.Pending;
            state.CanInstall = hasSelection && isAdministrator && enable && validForDownloadOrInstall && currentList == WpfUpdateListKind.Pending;
            state.CanUninstall = hasSelection && isAdministrator && enable && currentList == WpfUpdateListKind.Installed;
            state.CanHide = hasSelection && enable && agentValid && (currentList == WpfUpdateListKind.Pending || currentList == WpfUpdateListKind.Hidden);
            state.CanGetLinks = hasSelection && currentList != WpfUpdateListKind.History;
            state.CanCancel = agentBusy;
            return state;
        }
    }
}
