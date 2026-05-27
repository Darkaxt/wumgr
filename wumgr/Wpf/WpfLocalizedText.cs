using System.Collections.Generic;
using System.Text;

namespace wumgr.Wpf
{
    public class WpfLocalizedText
    {
        public string OptionsTab { get { return Translate.fmt("lbl_opt"); } }
        public string AutoUpdateTab { get { return Translate.fmt("lbl_au"); } }
        public string RunInBackground { get { return Translate.fmt("lbl_auto"); } }
        public string UpdateSource { get { return Translate.fmt("wpf_update_source"); } }
        public string OfflineMode { get { return Translate.fmt("lbl_off"); } }
        public string DownloadOfflineCab { get { return Translate.fmt("lbl_dl"); } }
        public string ManualDownloadInstall { get { return Translate.fmt("lbl_man"); } }
        public string IncludeSuperseded { get { return Translate.fmt("lbl_old"); } }
        public string RegisterMicrosoftUpdate { get { return Translate.fmt("lbl_ms"); } }
        public string AlwaysRunAsAdministrator { get { return Translate.fmt("lbl_uac"); } }
        public string BlockMicrosoftServers { get { return Translate.fmt("lbl_block_ms"); } }
        public string DisableAutomaticUpdate { get { return Translate.fmt("lbl_au_off"); } }
        public string DisableUpdateFacilitators { get { return Translate.fmt("lbl_au_dissable"); } }
        public string NotificationOnly { get { return Translate.fmt("lbl_au_notify"); } }
        public string DownloadOnly { get { return Translate.fmt("lbl_au_dl"); } }
        public string ScheduledInstallation { get { return Translate.fmt("lbl_au_time"); } }
        public string AutomaticUpdateDefault { get { return Translate.fmt("lbl_au_def"); } }
        public string HideWindowsUpdateSettings { get { return Translate.fmt("lbl_hide"); } }
        public string DisableStoreAutoUpdate { get { return Translate.fmt("lbl_store"); } }
        public string IncludeDrivers { get { return Translate.fmt("lbl_drv"); } }
        public string SearchButton { get { return Translate.fmt("tip_search"); } }
        public string DownloadButton { get { return Translate.fmt("tip_dl"); } }
        public string InstallButton { get { return Translate.fmt("tip_inst"); } }
        public string UninstallButton { get { return Translate.fmt("tip_rem"); } }
        public string HideButton { get { return Translate.fmt("tip_hide"); } }
        public string UnhideButton { get { return Translate.fmt("wpf_unhide"); } }
        public string OpenMenu { get { return Translate.fmt("wpf_open"); } }
        public string ExitMenu { get { return StripWinFormsMnemonic(Translate.fmt("menu_exit")); } }
        public string GetLinksButton { get { return Translate.fmt("tip_lnk"); } }
        public string CancelButton { get { return Translate.fmt("tip_cancel"); } }
        public string RefreshButton { get { return StripWinFormsMnemonic(Translate.fmt("menu_refresh")); } }
        public string OpenWinFormsButton { get { return Translate.fmt("wpf_open_winforms"); } }
        public string OpenWinFormsHint { get { return Translate.fmt("wpf_open_winforms_hint"); } }
        public string WindowsUpdateHeading { get { return Translate.fmt("wpf_updates_heading"); } }
        public string StatusHeader { get { return Translate.fmt("wpf_status"); } }
        public string ElevatedStatus { get { return Translate.fmt("wpf_elevated"); } }
        public string ReadOnlyStatus { get { return Translate.fmt("wpf_readonly"); } }
        public string InitializingAgent { get { return Translate.fmt("wpf_initializing_agent"); } }
        public string TitleColumn { get { return Translate.fmt("col_title"); } }
        public string CategoryColumn { get { return Translate.fmt("col_cat"); } }
        public string KbColumn { get { return Translate.fmt("col_kb"); } }
        public string DateColumn { get { return Translate.fmt("col_date"); } }
        public string SizeColumn { get { return Translate.fmt("col_site"); } }
        public string StateColumn { get { return Translate.fmt("col_stat"); } }

        public IReadOnlyList<string> AutoUpdateOptions
        {
            get
            {
                return new[]
                {
                    Translate.fmt("lbl_ac_no"),
                    Translate.fmt("lbl_ac_day"),
                    Translate.fmt("lbl_ac_week"),
                    Translate.fmt("lbl_ac_month")
                };
            }
        }

        public IReadOnlyList<string> ScheduleDays
        {
            get
            {
                return new[]
                {
                    Translate.fmt("wpf_daily"),
                    Translate.fmt("wpf_sunday"),
                    Translate.fmt("wpf_monday"),
                    Translate.fmt("wpf_tuesday"),
                    Translate.fmt("wpf_wednesday"),
                    Translate.fmt("wpf_thursday"),
                    Translate.fmt("wpf_friday"),
                    Translate.fmt("wpf_saturday")
                };
            }
        }

        internal static string StripWinFormsMnemonic(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            StringBuilder builder = new StringBuilder(text.Length);
            for (int index = 0; index < text.Length; index++)
            {
                if (text[index] != '&')
                {
                    builder.Append(text[index]);
                    continue;
                }

                if (index + 1 < text.Length && text[index + 1] == '&')
                {
                    builder.Append('&');
                    index++;
                }
            }

            return builder.ToString();
        }
    }
}
