namespace wumgr
{
    internal struct WpfPolicyOptionState
    {
        public bool CanChangeBlockMicrosoft;
        public bool CanSelectNotification;
        public bool CanSelectDownload;
        public bool CanSelectScheduled;
        public bool CanChangeSchedule;
        public bool CanChangeDisableFacilitators;
        public bool DisableFacilitatorsForcedOn;
        public bool CanChangeHideWindowsUpdatePage;
        public bool HideWindowsUpdatePageForcedOn;
        public bool CanChangeStoreAutoUpdate;
        public bool CanChangeDrivers;

        public static WpfPolicyOptionState Create(bool isAdministrator, GPO.Respect gpoRespect, float windowsVersion, GPO.AUOptions selectedAutoUpdate, bool blockMicrosoftServers, bool disableFacilitators)
        {
            WpfPolicyOptionState state = new WpfPolicyOptionState();
            bool canWritePolicy = isAdministrator;
            bool selectedDisabled = selectedAutoUpdate == GPO.AUOptions.Disabled;
            bool selectedScheduled = selectedAutoUpdate == GPO.AUOptions.Scheduled;
            bool hasStandardGpoPolicy = gpoRespect != GPO.Respect.Partial && gpoRespect != GPO.Respect.None;

            state.CanChangeBlockMicrosoft = canWritePolicy && gpoRespect != GPO.Respect.None;
            state.CanSelectNotification = canWritePolicy && hasStandardGpoPolicy;
            state.CanSelectDownload = canWritePolicy && hasStandardGpoPolicy;
            state.CanSelectScheduled = canWritePolicy && hasStandardGpoPolicy;
            state.CanChangeSchedule = state.CanSelectScheduled && selectedScheduled;

            state.DisableFacilitatorsForcedOn = selectedDisabled
                && (gpoRespect == GPO.Respect.None || (gpoRespect == GPO.Respect.Partial && !blockMicrosoftServers));
            state.CanChangeDisableFacilitators = canWritePolicy
                && selectedDisabled
                && windowsVersion >= 10.0f
                && !state.DisableFacilitatorsForcedOn
                && (hasStandardGpoPolicy || (gpoRespect == GPO.Respect.Partial && blockMicrosoftServers));

            bool effectiveDisableFacilitators = disableFacilitators || state.DisableFacilitatorsForcedOn;
            state.HideWindowsUpdatePageForcedOn = effectiveDisableFacilitators;
            state.CanChangeHideWindowsUpdatePage = canWritePolicy && windowsVersion >= 10.0f && !effectiveDisableFacilitators;
            state.CanChangeStoreAutoUpdate = canWritePolicy && windowsVersion >= 6.2f;
            state.CanChangeDrivers = canWritePolicy;

            return state;
        }
    }
}
