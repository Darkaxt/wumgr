using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using WUApiLib;

namespace wumgr.Wpf
{
    public partial class WuMgrWpfWindow : Window, INotifyPropertyChanged
    {
        private WpfUpdateListKind currentList = WpfUpdateListKind.Pending;
        private bool isAdministrator;
        private bool offlineMode;
        private bool downloadOfflineCab;
        private bool manualMode;
        private bool includeSuperseded;
        private bool registerMicrosoftUpdate;
        private bool skipUacEnabled;
        private string selectedSource;
        private string statusText;
        private string statusLog;
        private int totalPercent;
        private bool isBusyIndeterminate;

        public ObservableCollection<WpfUpdateRow> Updates { get; private set; }
        public ObservableCollection<string> Sources { get; private set; }

        public string VersionText { get { return "v" + Program.mVersion; } }
        public string ElevationText { get { return IsAdministrator ? "Running elevated" : "Read-only launch. Admin actions require elevation."; } }
        public string PendingLabel { get { return string.Format("Windows Update ({0})", Program.Agent.mPendingUpdates.Count); } }
        public string InstalledLabel { get { return string.Format("Installed Updates ({0})", Program.Agent.mInstalledUpdates.Count); } }
        public string HiddenLabel { get { return string.Format("Hidden Updates ({0})", Program.Agent.mHiddenUpdates.Count); } }
        public string HistoryLabel { get { return string.Format("Update History ({0})", Program.Agent.mUpdateHistory.Count); } }

        public bool IsAdministrator
        {
            get { return isAdministrator; }
            private set { SetField(ref isAdministrator, value, "IsAdministrator"); }
        }

        public bool OfflineMode
        {
            get { return offlineMode; }
            set
            {
                if (!SetField(ref offlineMode, value, "OfflineMode"))
                    return;

                SetConfig("Offline", value ? "1" : "0");
                OnPropertyChanged("CanUseOnlineSource");
                NotifyActionStateChanged();
            }
        }

        public bool DownloadOfflineCab
        {
            get { return downloadOfflineCab; }
            set
            {
                if (SetField(ref downloadOfflineCab, value, "DownloadOfflineCab"))
                    SetConfig("Download", value ? "1" : "0");
            }
        }

        public bool ManualMode
        {
            get { return manualMode; }
            set
            {
                if (!SetField(ref manualMode, value, "ManualMode"))
                    return;

                SetConfig("Manual", value ? "1" : "0");
                NotifyActionStateChanged();
            }
        }

        public bool IncludeSuperseded
        {
            get { return includeSuperseded; }
            set
            {
                if (SetField(ref includeSuperseded, value, "IncludeSuperseded"))
                    SetConfig("IncludeOld", value ? "1" : "0");
            }
        }

        public bool RegisterMicrosoftUpdate
        {
            get { return registerMicrosoftUpdate; }
            set
            {
                if (!SetField(ref registerMicrosoftUpdate, value, "RegisterMicrosoftUpdate"))
                    return;

                Program.Agent.EnableService(WuAgent.MsUpdGUID, value);
                LoadSources(SelectedSource);
            }
        }

        public bool SkipUacEnabled
        {
            get { return skipUacEnabled; }
            set
            {
                if (!SetField(ref skipUacEnabled, value, "SkipUacEnabled"))
                    return;

                if (IsAdministrator)
                    Program.SkipUacEnable(value);
            }
        }

        public string SelectedSource
        {
            get { return selectedSource; }
            set
            {
                if (SetField(ref selectedSource, value, "SelectedSource") && !string.IsNullOrEmpty(value))
                    SetConfig("Source", value);
            }
        }

        public string StatusText
        {
            get { return statusText; }
            private set { SetField(ref statusText, value, "StatusText"); }
        }

        public string StatusLog
        {
            get { return statusLog; }
            private set { SetField(ref statusLog, value, "StatusLog"); }
        }

        public int TotalPercent
        {
            get { return totalPercent; }
            private set { SetField(ref totalPercent, value, "TotalPercent"); }
        }

        public bool IsBusyIndeterminate
        {
            get { return isBusyIndeterminate; }
            private set { SetField(ref isBusyIndeterminate, value, "IsBusyIndeterminate"); }
        }

        public bool HasSelection { get { return Updates.Any(update => update.Selected); } }
        public bool CanUseOnlineSource { get { return !OfflineMode; } }
        public bool IsPendingList { get { return currentList == WpfUpdateListKind.Pending; } }
        public bool IsInstalledList { get { return currentList == WpfUpdateListKind.Installed; } }
        public bool IsHiddenList { get { return currentList == WpfUpdateListKind.Hidden; } }
        public bool IsHistoryList { get { return currentList == WpfUpdateListKind.History; } }
        public bool CanSearch { get { return CurrentActionState.CanSearch; } }
        public bool CanDownload { get { return CurrentActionState.CanDownload; } }
        public bool CanInstall { get { return CurrentActionState.CanInstall; } }
        public bool CanUninstall { get { return CurrentActionState.CanUninstall; } }
        public bool CanHide { get { return CurrentActionState.CanHide; } }
        public bool CanGetLinks { get { return CurrentActionState.CanGetLinks; } }
        public bool CanCancel { get { return CurrentActionState.CanCancel; } }
        public string HideButtonText { get { return IsHiddenList ? "Unhide" : "Hide"; } }

        private WpfActionState CurrentActionState
        {
            get
            {
                return WpfActionState.Create(HasSelection, IsAdministrator, Program.Agent.IsActive(), Program.Agent.IsBusy(), Program.Agent.IsValid(), ManualMode, currentList);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public WuMgrWpfWindow()
        {
            Updates = new ObservableCollection<WpfUpdateRow>();
            Sources = new ObservableCollection<string>();
            StatusLog = "";
            StatusText = "";

            InitializeComponent();
            DataContext = this;

            IsAdministrator = MiscFunc.IsAdministrator();
            skipUacEnabled = Program.IsSkipUacRun();
            offlineMode = MiscFunc.parseInt(GetConfig("Offline", "0")) != 0;
            downloadOfflineCab = MiscFunc.parseInt(GetConfig("Download", "1")) != 0;
            manualMode = MiscFunc.parseInt(GetConfig("Manual", "0")) != 0;
            includeSuperseded = MiscFunc.parseInt(GetConfig("IncludeOld", "0")) != 0;
            registerMicrosoftUpdate = Program.Agent.IsActive() && Program.Agent.TestService(WuAgent.MsUpdGUID);

            LoadSources(GetConfig("Source", "Windows Update"));
            AttachAgentEvents();
            Program.ipc.PipeMessage += PipesMessageHandler;
            Program.ipc.Listen();
            LoadList();
            AppendLog("WPF shell loaded. Full update operations are now wired for the core lists.");
            NotifyAllStateChanged();
        }

        private void AttachAgentEvents()
        {
            Program.Agent.Progress += Agent_Progress;
            Program.Agent.UpdatesChaged += Agent_UpdatesChanged;
            Program.Agent.Finished += Agent_Finished;
            Closed += WuMgrWpfWindow_Closed;
        }

        private void WuMgrWpfWindow_Closed(object sender, EventArgs e)
        {
            Program.Agent.Progress -= Agent_Progress;
            Program.Agent.UpdatesChaged -= Agent_UpdatesChanged;
            Program.Agent.Finished -= Agent_Finished;
            Program.ipc.PipeMessage -= PipesMessageHandler;
        }

        private void PipesMessageHandler(PipeIPC.PipeServer pipe, string data)
        {
            if (data.Equals("show", StringComparison.CurrentCultureIgnoreCase))
            {
                Dispatcher.BeginInvoke(new Action(ShowMainWindow));
                pipe.Send("ok");
            }
            else
            {
                pipe.Send("unknown");
            }
        }

        private void ShowMainWindow()
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
            Show();
            Activate();
        }

        private void LoadSources(string preferredSource)
        {
            Sources.Clear();
            foreach (string source in Program.Agent.mServiceList)
                Sources.Add(source);

            if (!string.IsNullOrEmpty(preferredSource) && Sources.Contains(preferredSource))
                SelectedSource = preferredSource;
            else if (Sources.Count > 0)
                SelectedSource = Sources[0];
        }

        private void LoadList()
        {
            Updates.Clear();
            foreach (MsUpdate update in GetCurrentUpdates())
            {
                WpfUpdateRow row = new WpfUpdateRow(update);
                row.PropertyChanged += UpdateRow_PropertyChanged;
                Updates.Add(row);
            }

            NotifyAllStateChanged();
        }

        private IEnumerable<MsUpdate> GetCurrentUpdates()
        {
            switch (currentList)
            {
                case WpfUpdateListKind.Pending: return Program.Agent.mPendingUpdates;
                case WpfUpdateListKind.Installed: return Program.Agent.mInstalledUpdates;
                case WpfUpdateListKind.Hidden: return Program.Agent.mHiddenUpdates;
                case WpfUpdateListKind.History: return Program.Agent.mUpdateHistory;
                default: return Program.Agent.mPendingUpdates;
            }
        }

        private List<MsUpdate> GetSelectedUpdates()
        {
            return Updates.Where(update => update.Selected).Select(update => update.Update).ToList();
        }

        private void UpdateRow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Selected")
                NotifyActionStateChanged();
        }

        private void PendingList_Click(object sender, RoutedEventArgs e)
        {
            SwitchList(WpfUpdateListKind.Pending);
        }

        private void InstalledList_Click(object sender, RoutedEventArgs e)
        {
            SwitchList(WpfUpdateListKind.Installed);
        }

        private void HiddenList_Click(object sender, RoutedEventArgs e)
        {
            SwitchList(WpfUpdateListKind.Hidden);
        }

        private void HistoryList_Click(object sender, RoutedEventArgs e)
        {
            if (Program.Agent.IsActive())
                Program.Agent.UpdateHistory();
            SwitchList(WpfUpdateListKind.History);
        }

        private void SwitchList(WpfUpdateListKind list)
        {
            currentList = list;
            LoadList();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadList();
            AppendLog("Current WPF list refreshed from the agent cache.");
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (!Program.Agent.IsActive() || Program.Agent.IsBusy())
                return;

            WuAgent.RetCodes ret = OfflineMode
                ? Program.Agent.SearchForUpdates(DownloadOfflineCab, IncludeSuperseded)
                : Program.Agent.SearchForUpdates(SelectedSource, IncludeSuperseded);
            ShowImmediateResult(WuAgent.AgentOperation.CheckingUpdates, ret);
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            if (!ManualMode && !IsAdministrator)
            {
                MessageBox.Show(Translate.fmt("msg_admin_dl"), Program.mName);
                return;
            }

            WuAgent.RetCodes ret = ManualMode
                ? Program.Agent.DownloadUpdatesManually(GetSelectedUpdates())
                : Program.Agent.DownloadUpdates(GetSelectedUpdates());
            ShowImmediateResult(WuAgent.AgentOperation.DownloadingUpdates, ret);
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdministrator)
            {
                MessageBox.Show(Translate.fmt("msg_admin_inst"), Program.mName);
                return;
            }

            WuAgent.RetCodes ret = ManualMode
                ? Program.Agent.DownloadUpdatesManually(GetSelectedUpdates(), true)
                : Program.Agent.DownloadUpdates(GetSelectedUpdates(), true);
            ShowImmediateResult(WuAgent.AgentOperation.InstallingUpdates, ret);
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdministrator)
            {
                MessageBox.Show(Translate.fmt("msg_admin_rem"), Program.mName);
                return;
            }

            WuAgent.RetCodes ret = Program.Agent.UnInstallUpdatesManually(GetSelectedUpdates());
            ShowImmediateResult(WuAgent.AgentOperation.RemoveingUpdates, ret);
        }

        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            bool hide = currentList == WpfUpdateListKind.Pending;
            Program.Agent.HideUpdates(GetSelectedUpdates(), hide);
            LoadList();
        }

        private void GetLinks_Click(object sender, RoutedEventArgs e)
        {
            List<string> links = new List<string>();
            foreach (MsUpdate update in GetSelectedUpdates())
            {
                foreach (string download in update.Downloads)
                {
                    if (!string.IsNullOrWhiteSpace(download))
                        links.Add(download);
                }
            }

            if (links.Count == 0)
            {
                AppendLog("No download links are available for the selected updates.");
                return;
            }

            Clipboard.SetText(string.Join(Environment.NewLine, links.ToArray()));
            AppendLog(string.Format("Copied {0} download link(s) to the clipboard.", links.Count));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Program.Agent.CancelOperations();
        }

        private void OpenWinForms_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Close this WPF window and launch WuMgr without -wpf to use the WinForms UI.", Program.mName);
        }

        private void Agent_Progress(object sender, WuAgent.ProgressArgs args)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                StatusText = FormatOperation(Program.Agent.CurOperation());
                if (args.TotalCount == -1)
                {
                    IsBusyIndeterminate = true;
                    StatusText += "...";
                }
                else
                {
                    IsBusyIndeterminate = false;
                    if (args.TotalPercent >= 0 && args.TotalPercent <= 100)
                        TotalPercent = args.TotalPercent;
                    if (args.TotalCount > 1)
                        StatusText += " " + args.CurrentIndex + "/" + args.TotalCount;
                }
                NotifyActionStateChanged();
            }));
        }

        private void Agent_UpdatesChanged(object sender, WuAgent.UpdatesArgs args)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (args.Found)
                {
                    SetConfig("LastCheck", DateTime.Now.ToString());
                    currentList = WpfUpdateListKind.Pending;
                }
                LoadList();
            }));
        }

        private void Agent_Finished(object sender, WuAgent.FinishedArgs args)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                StatusText = "";
                IsBusyIndeterminate = false;
                TotalPercent = 0;
                NotifyActionStateChanged();
                ShowResult(args.Op, args.Ret, args.RebootNeeded);
            }));
        }

        private void ShowImmediateResult(WuAgent.AgentOperation operation, WuAgent.RetCodes ret)
        {
            if (ret != WuAgent.RetCodes.InProgress)
                ShowResult(operation, ret, false);
            NotifyActionStateChanged();
        }

        private void ShowResult(WuAgent.AgentOperation operation, WuAgent.RetCodes ret, bool reboot)
        {
            if (ret == WuAgent.RetCodes.Success || ret == WuAgent.RetCodes.InProgress)
                return;

            if (ret == WuAgent.RetCodes.Abborted)
            {
                AppendLog(FormatOperation(operation) + " aborted.");
                return;
            }

            string message = FormatRetCode(ret);
            AppendLog(message);
            MessageBox.Show(message, Program.mName);

            if (reboot)
                AppendLog("A reboot is required to finish the operation.");
        }

        private static string FormatOperation(WuAgent.AgentOperation operation)
        {
            switch (operation)
            {
                case WuAgent.AgentOperation.CheckingUpdates: return Translate.fmt("op_check");
                case WuAgent.AgentOperation.PreparingCheck: return Translate.fmt("op_prep");
                case WuAgent.AgentOperation.PreparingUpdates:
                case WuAgent.AgentOperation.DownloadingUpdates: return Translate.fmt("op_dl");
                case WuAgent.AgentOperation.InstallingUpdates: return Translate.fmt("op_inst");
                case WuAgent.AgentOperation.RemoveingUpdates: return Translate.fmt("op_rem");
                case WuAgent.AgentOperation.CancelingOperation: return Translate.fmt("op_cancel");
                default: return Translate.fmt("op_unk");
            }
        }

        private static string FormatRetCode(WuAgent.RetCodes ret)
        {
            switch (ret)
            {
                case WuAgent.RetCodes.AccessError: return Translate.fmt("err_admin");
                case WuAgent.RetCodes.Busy: return Translate.fmt("err_busy");
                case WuAgent.RetCodes.DownloadFailed: return Translate.fmt("err_dl");
                case WuAgent.RetCodes.InstallFailed: return Translate.fmt("err_inst");
                case WuAgent.RetCodes.NoUpdated: return Translate.fmt("err_no_sel");
                case WuAgent.RetCodes.InternalError: return Translate.fmt("err_int");
                case WuAgent.RetCodes.FileNotFound: return Translate.fmt("err_file");
                default: return ret.ToString();
            }
        }

        private void AppendLog(string message)
        {
            if (string.IsNullOrEmpty(StatusLog))
                StatusLog = message;
            else
                StatusLog += Environment.NewLine + message;
        }

        private void NotifyAllStateChanged()
        {
            OnPropertyChanged("OfflineMode");
            OnPropertyChanged("DownloadOfflineCab");
            OnPropertyChanged("ManualMode");
            OnPropertyChanged("IncludeSuperseded");
            OnPropertyChanged("RegisterMicrosoftUpdate");
            OnPropertyChanged("SkipUacEnabled");
            OnPropertyChanged("SelectedSource");
            OnPropertyChanged("CanUseOnlineSource");
            OnPropertyChanged("PendingLabel");
            OnPropertyChanged("InstalledLabel");
            OnPropertyChanged("HiddenLabel");
            OnPropertyChanged("HistoryLabel");
            OnPropertyChanged("IsPendingList");
            OnPropertyChanged("IsInstalledList");
            OnPropertyChanged("IsHiddenList");
            OnPropertyChanged("IsHistoryList");
            OnPropertyChanged("HideButtonText");
            NotifyActionStateChanged();
        }

        private void NotifyActionStateChanged()
        {
            OnPropertyChanged("HasSelection");
            OnPropertyChanged("CanSearch");
            OnPropertyChanged("CanDownload");
            OnPropertyChanged("CanInstall");
            OnPropertyChanged("CanUninstall");
            OnPropertyChanged("CanHide");
            OnPropertyChanged("CanGetLinks");
            OnPropertyChanged("CanCancel");
        }

        private bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (object.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private static string GetConfig(string name, string def = "")
        {
            return Program.IniReadValue("Options", name, def);
        }

        private static void SetConfig(string name, string value)
        {
            Program.IniWriteValue("Options", name, value);
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WpfUpdateRow : INotifyPropertyChanged
    {
        private bool selected;

        public event PropertyChangedEventHandler PropertyChanged;

        public MsUpdate Update { get; private set; }

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected == value)
                    return;

                selected = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Selected"));
            }
        }

        public string Title { get; private set; }
        public string Category { get; private set; }
        public string KB { get; private set; }
        public string Date { get; private set; }
        public string Size { get; private set; }
        public string State { get; private set; }

        public WpfUpdateRow(MsUpdate update)
        {
            Update = update;
            Title = update.Title;
            Category = update.Category;
            KB = update.State == MsUpdate.UpdateState.History ? update.ApplicationID : update.KB;
            Date = update.Date == DateTime.MinValue ? "" : update.Date.ToShortDateString();
            Size = FileOps.FormatSize(update.Size);
            State = FormatState(update);
        }

        private static string FormatState(MsUpdate update)
        {
            if (update.State == MsUpdate.UpdateState.History)
            {
                string state = "";
                switch ((OperationResultCode)update.ResultCode)
                {
                    case OperationResultCode.orcNotStarted: state = Translate.fmt("stat_not_start"); break;
                    case OperationResultCode.orcInProgress: state = Translate.fmt("stat_in_prog"); break;
                    case OperationResultCode.orcSucceeded: state = Translate.fmt("stat_success"); break;
                    case OperationResultCode.orcSucceededWithErrors: state = Translate.fmt("stat_success_2"); break;
                    case OperationResultCode.orcFailed: state = Translate.fmt("stat_failed"); break;
                    case OperationResultCode.orcAborted: state = Translate.fmt("stat_abbort"); break;
                }
                return state + " (0x" + string.Format("{0:X8}", update.HResult) + ")";
            }

            if ((update.Attributes & (int)MsUpdate.UpdateAttr.Installed) != 0)
                return Translate.fmt("stat_install");

            if ((update.Attributes & (int)MsUpdate.UpdateAttr.Hidden) != 0)
                return Translate.fmt("stat_block");

            if ((update.Attributes & (int)MsUpdate.UpdateAttr.Downloaded) != 0)
                return Translate.fmt("stat_dl");

            return Translate.fmt("stat_pending");
        }
    }
}
