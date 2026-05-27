using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using Controls = System.Windows.Controls;
using Forms = System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
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
        private bool runInBackground;
        private bool? blockMicrosoftServers;
        private bool disableUpdateFacilitators;
        private bool hideWindowsUpdatePage;
        private bool disableStoreAutoUpdate;
        private bool? includeDriversInUpdates;
        private WpfThemeMode themeMode = WpfThemeMode.System;
        private string selectedSource;
        private string statusText;
        private string statusLog;
        private string searchFilter;
        private int totalPercent;
        private bool isBusyIndeterminate;
        private bool allowShowDisplay = true;
        private Forms.NotifyIcon notifyIcon;
        private DispatcherTimer autoUpdateTimer;
        private DispatcherTimer progressAnimationTimer;
        private int selectedAutoUpdateIndex;
        private int idleDelay;
        private double progressAnimationPhase;
        private DateTime lastCheck = DateTime.MinValue;
        private DateTime lastBalloon = DateTime.MinValue;
        private GPO.Respect gpoRespect = GPO.Respect.Unknown;
        private float windowsVersion;
        private GPO.AUOptions selectedPolicyAutoUpdate;
        private int selectedScheduleDay;
        private int selectedScheduleTime;
        private bool suspendPolicyUpdate;
        private bool agentInitializationStarted;
        private bool suppressSelectAllRowsChange;
        private bool? selectAllRowsState;
        private readonly WpfLocalizedText text = new WpfLocalizedText();

        public ObservableCollection<WpfUpdateRow> Updates { get; private set; }
        public ObservableCollection<string> Sources { get; private set; }
        public ObservableCollection<string> AutoUpdateOptions { get; private set; }
        public ObservableCollection<string> ThemeOptions { get; private set; }
        public ObservableCollection<string> ScheduleDays { get; private set; }
        public ObservableCollection<string> ScheduleTimes { get; private set; }

        public WpfLocalizedText Text { get { return text; } }
        public string VersionText { get { return "v" + Program.mVersion; } }
        public string ElevationText { get { return IsAdministrator ? text.ElevatedStatus : text.ReadOnlyStatus; } }
        public string PendingLabel { get { return Translate.fmt("lbl_fnd_upd", Program.Agent.mPendingUpdates.Count); } }
        public string InstalledLabel { get { return Translate.fmt("lbl_inst_upd", Program.Agent.mInstalledUpdates.Count); } }
        public string HiddenLabel { get { return Translate.fmt("lbl_block_upd", Program.Agent.mHiddenUpdates.Count); } }
        public string HistoryLabel { get { return Translate.fmt("lbl_old_upd", Program.Agent.mUpdateHistory.Count); } }

        public bool IsAdministrator
        {
            get { return isAdministrator; }
            private set
            {
                if (SetField(ref isAdministrator, value, "IsAdministrator"))
                    OnPropertyChanged("ElevationText");
            }
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

        public bool RunInBackground
        {
            get { return runInBackground; }
            set
            {
                if (!SetField(ref runInBackground, value, "RunInBackground"))
                    return;

                if (!MiscFunc.IsRunningAsUwp())
                    Program.AutoStart(value);
                UpdateNotifyIcon();
                OnPropertyChanged("CanChangeRunInBackground");
                OnPropertyChanged("SelectedAutoUpdateIndex");
            }
        }

        public int SelectedAutoUpdateIndex
        {
            get { return selectedAutoUpdateIndex; }
            set
            {
                if (value < 0 || value >= AutoUpdateOptions.Count)
                    value = 0;

                if (SetField(ref selectedAutoUpdateIndex, value, "SelectedAutoUpdateIndex"))
                    SetConfig("AutoUpdate", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public int SelectedThemeIndex
        {
            get { return WpfThemeSettings.ToSelectedIndex(themeMode); }
            set
            {
                WpfThemeMode nextMode = WpfThemeSettings.FromSelectedIndex(value);
                if (themeMode == nextMode)
                    return;

                themeMode = nextMode;
                OnPropertyChanged("SelectedThemeIndex");
                SetConfig(WpfThemeSettings.ConfigKey, WpfThemeSettings.ToConfigValue(themeMode));
                ApplyTheme();
            }
        }

        public bool? BlockMicrosoftServers
        {
            get { return blockMicrosoftServers; }
            set
            {
                if (object.Equals(blockMicrosoftServers, value))
                    return;

                bool? previous = blockMicrosoftServers;
                blockMicrosoftServers = value;
                OnPropertyChanged("BlockMicrosoftServers");

                if (!suspendPolicyUpdate && !HandleBlockMicrosoftServersChanged(previous))
                    return;

                CoercePolicyDerivedState();
                NotifyPolicyOptionStateChanged();
            }
        }

        public bool DisableUpdateFacilitators
        {
            get { return disableUpdateFacilitators; }
            set { SetDisableUpdateFacilitators(value, !suspendPolicyUpdate); }
        }

        public bool HideWindowsUpdatePage
        {
            get { return hideWindowsUpdatePage; }
            set { SetHideWindowsUpdatePage(value, !suspendPolicyUpdate); }
        }

        public bool DisableStoreAutoUpdate
        {
            get { return disableStoreAutoUpdate; }
            set
            {
                if (!SetField(ref disableStoreAutoUpdate, value, "DisableStoreAutoUpdate"))
                    return;

                if (!suspendPolicyUpdate && IsAdministrator)
                    GPO.SetStoreAU(value);
            }
        }

        public bool? IncludeDriversInUpdates
        {
            get { return includeDriversInUpdates; }
            set
            {
                if (!SetField(ref includeDriversInUpdates, value, "IncludeDriversInUpdates"))
                    return;

                if (!suspendPolicyUpdate && IsAdministrator)
                    GPO.ConfigDriverAU(ToGpoCheckState(value));
            }
        }

        public bool IsPolicyDefault
        {
            get { return selectedPolicyAutoUpdate == GPO.AUOptions.Default; }
            set { if (value) SetSelectedPolicyAutoUpdate(GPO.AUOptions.Default); }
        }

        public bool IsPolicyDisabled
        {
            get { return selectedPolicyAutoUpdate == GPO.AUOptions.Disabled; }
            set { if (value) SetSelectedPolicyAutoUpdate(GPO.AUOptions.Disabled); }
        }

        public bool IsPolicyNotification
        {
            get { return selectedPolicyAutoUpdate == GPO.AUOptions.Notification; }
            set { if (value) SetSelectedPolicyAutoUpdate(GPO.AUOptions.Notification); }
        }

        public bool IsPolicyDownload
        {
            get { return selectedPolicyAutoUpdate == GPO.AUOptions.Download; }
            set { if (value) SetSelectedPolicyAutoUpdate(GPO.AUOptions.Download); }
        }

        public bool IsPolicyScheduled
        {
            get { return selectedPolicyAutoUpdate == GPO.AUOptions.Scheduled; }
            set { if (value) SetSelectedPolicyAutoUpdate(GPO.AUOptions.Scheduled); }
        }

        public int SelectedScheduleDay
        {
            get { return selectedScheduleDay; }
            set
            {
                value = ClampIndex(value, ScheduleDays.Count);
                if (SetField(ref selectedScheduleDay, value, "SelectedScheduleDay"))
                    ApplyScheduledPolicyIfNeeded();
            }
        }

        public int SelectedScheduleTime
        {
            get { return selectedScheduleTime; }
            set
            {
                value = ClampIndex(value, ScheduleTimes.Count);
                if (SetField(ref selectedScheduleTime, value, "SelectedScheduleTime"))
                    ApplyScheduledPolicyIfNeeded();
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

        public string SearchFilter
        {
            get { return searchFilter; }
            set
            {
                if (!SetField(ref searchFilter, value, "SearchFilter"))
                    return;

                OnPropertyChanged("HasSearchFilter");
                LoadList();
            }
        }

        public int TotalPercent
        {
            get { return totalPercent; }
            private set
            {
                if (SetField(ref totalPercent, value, "TotalPercent"))
                    UpdateProgressFill();
            }
        }

        public bool IsBusyIndeterminate
        {
            get { return isBusyIndeterminate; }
            private set
            {
                if (SetField(ref isBusyIndeterminate, value, "IsBusyIndeterminate"))
                {
                    UpdateProgressAnimationState();
                    UpdateProgressFill();
                }
            }
        }

        public bool HasSelection { get { return Updates.Any(update => update.Selected); } }
        public bool HasSearchFilter { get { return !string.IsNullOrWhiteSpace(SearchFilter); } }
        public bool CanUseOnlineSource { get { return !OfflineMode; } }
        public bool CanChangeRunInBackground { get { return !MiscFunc.IsRunningAsUwp() || !RunInBackground; } }
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
        public string HideButtonText { get { return IsHiddenList ? text.UnhideButton : text.HideButton; } }
        public bool CanChangeBlockMicrosoft { get { return CurrentPolicyOptionState.CanChangeBlockMicrosoft; } }
        public bool CanSelectNotification { get { return CurrentPolicyOptionState.CanSelectNotification; } }
        public bool CanSelectDownload { get { return CurrentPolicyOptionState.CanSelectDownload; } }
        public bool CanSelectScheduled { get { return CurrentPolicyOptionState.CanSelectScheduled; } }
        public bool CanChangeSchedule { get { return CurrentPolicyOptionState.CanChangeSchedule; } }
        public bool CanChangeDisableFacilitators { get { return CurrentPolicyOptionState.CanChangeDisableFacilitators; } }
        public bool CanChangeHideWindowsUpdatePage { get { return CurrentPolicyOptionState.CanChangeHideWindowsUpdatePage; } }
        public bool CanChangeStoreAutoUpdate { get { return CurrentPolicyOptionState.CanChangeStoreAutoUpdate; } }
        public bool CanChangeDrivers { get { return CurrentPolicyOptionState.CanChangeDrivers; } }

        private WpfActionState CurrentActionState
        {
            get
            {
                return WpfActionState.Create(HasSelection, IsAdministrator, Program.Agent.IsActive(), Program.Agent.IsBusy(), Program.Agent.IsValid(), ManualMode, currentList);
            }
        }

        private WpfPolicyOptionState CurrentPolicyOptionState
        {
            get
            {
                return WpfPolicyOptionState.Create(IsAdministrator, gpoRespect, windowsVersion, selectedPolicyAutoUpdate, BlockMicrosoftServers == true, DisableUpdateFacilitators);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public WuMgrWpfWindow()
        {
            Updates = new ObservableCollection<WpfUpdateRow>();
            Sources = new ObservableCollection<string>();
            AutoUpdateOptions = new ObservableCollection<string>();
            ThemeOptions = new ObservableCollection<string>();
            ScheduleDays = new ObservableCollection<string>();
            ScheduleTimes = new ObservableCollection<string>();
            StatusLog = "";
            StatusText = "";

            InitializeComponent();
            CreateProgressAnimationTimer();
            DataContext = this;
            Title = Program.mName;
            ApplyLocalizedText();
            ApplyWindowCaptionButtons();
            LoadThemeOptions();
            themeMode = WpfThemeSettings.Parse(GetConfig(WpfThemeSettings.ConfigKey, "system"));
            OnPropertyChanged("SelectedThemeIndex");
            ApplyTheme();
            ApplyActionButtonIcons();

            IsAdministrator = MiscFunc.IsAdministrator();
            skipUacEnabled = Program.IsSkipUacRun();
            offlineMode = MiscFunc.parseInt(GetConfig("Offline", "0")) != 0;
            downloadOfflineCab = MiscFunc.parseInt(GetConfig("Download", "1")) != 0;
            manualMode = MiscFunc.parseInt(GetConfig("Manual", "0")) != 0;
            includeSuperseded = MiscFunc.parseInt(GetConfig("IncludeOld", "0")) != 0;
            registerMicrosoftUpdate = Program.Agent.IsActive() && Program.Agent.TestService(WuAgent.MsUpdGUID);
            runInBackground = Program.IsAutoStart();
            idleDelay = MiscFunc.parseInt(GetConfig("IdleDelay", "20"));
            selectedAutoUpdateIndex = MiscFunc.parseInt(GetConfig("AutoUpdate", "0"));
            LoadLastCheck();

            LoadScheduleOptions();
            LoadAutoUpdateOptions();
            LoadPolicyOptions();
            LoadSources(GetConfig("Source", "Windows Update"));
            LoadWindowSettings();
            CreateNotifyIcon();
            CreateAutoUpdateTimer();
            AttachAgentEvents();
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            Program.ipc.PipeMessage += PipesMessageHandler;
            Program.ipc.Listen();
            LoadList();
            AppendLog(WpfStatusText.Ready);
            NotifyAllStateChanged();
        }

        private void AttachAgentEvents()
        {
            Program.Agent.Progress += Agent_Progress;
            Program.Agent.UpdatesChaged += Agent_UpdatesChanged;
            Program.Agent.Finished += Agent_Finished;
            ContentRendered += WuMgrWpfWindow_ContentRendered;
            StateChanged += WuMgrWpfWindow_StateChanged;
            Closing += WuMgrWpfWindow_Closing;
            Closed += WuMgrWpfWindow_Closed;
        }

        private void ApplyLocalizedText()
        {
            TitleColumn.Header = text.TitleColumn;
            CategoryColumn.Header = text.CategoryColumn;
            KbColumn.Header = text.KbColumn;
            DateColumn.Header = text.DateColumn;
            SizeColumn.Header = text.SizeColumn;
            StateColumn.Header = text.StateColumn;
        }

        private void LoadThemeOptions()
        {
            ThemeOptions.Clear();
            foreach (string option in text.ThemeOptions)
                ThemeOptions.Add(option);
        }

        private void ApplyWindowCaptionButtons()
        {
            MinimizeWindowButton.Content = WpfWindowCaptionGlyph.Minimize;
            MinimizeWindowButton.ToolTip = "Minimize";
            System.Windows.Automation.AutomationProperties.SetName(MinimizeWindowButton, "Minimize");

            CloseWindowButton.Content = WpfWindowCaptionGlyph.Close;
            CloseWindowButton.ToolTip = "Close";
            System.Windows.Automation.AutomationProperties.SetName(CloseWindowButton, "Close");

            UpdateMaximizeRestoreButton();
        }

        private void UpdateMaximizeRestoreButton()
        {
            bool isMaximized = WindowState == WindowState.Maximized;
            string label = isMaximized ? "Restore" : "Maximize";
            MaximizeWindowButton.Content = WpfWindowCaptionGlyph.GetMaximizeRestoreGlyph(isMaximized);
            MaximizeWindowButton.ToolTip = label;
            System.Windows.Automation.AutomationProperties.SetName(MaximizeWindowButton, label);
        }

        private void ApplyTheme()
        {
            WpfThemeMode effectiveMode = WpfThemeSettings.ResolveEffectiveMode(themeMode, IsSystemUsingLightTheme());
            bool useDarkTheme = effectiveMode == WpfThemeMode.Dark;

            if (useDarkTheme)
            {
                SetThemeBrush("WindowBackgroundBrush", "#111820");
                SetThemeBrush("HeaderBackgroundBrush", "#151e28");
                SetThemeBrush("PanelBackgroundBrush", "#17212d");
                SetThemeBrush("SurfaceBackgroundBrush", "#101821");
                SetThemeBrush("ControlBackgroundBrush", "#1f2b38");
                SetThemeBrush("ControlHoverBrush", "#2a3948");
                SetThemeBrush("ControlPressedBrush", "#314457");
                SetThemeBrush("SelectedBackgroundBrush", "#264a60");
                SetThemeBrush("BorderLineBrush", "#344252");
                SetThemeBrush("TextBrush", "#edf3fa");
                SetThemeBrush("MutedTextBrush", "#a8b6c6");
                SetThemeBrush("DisabledTextBrush", "#6f7f90");
                SetThemeBrush("WarningTextBrush", "#f3c969");
                SetThemeBrush("GridBackgroundBrush", "#101821");
                SetThemeBrush("GridHeaderBackgroundBrush", "#1b2633");
                SetThemeBrush("GridLineBrush", "#2d3b49");
                SetThemeBrush("ProgressTrackBrush", "#253342");
                SetThemeBrush("ProgressFillBrush", "#3fb950");
                SetThemeBrush("ProgressMarqueeBrush", "#58a6ff");
            }
            else
            {
                SetThemeBrush("WindowBackgroundBrush", "#f3f5f7");
                SetThemeBrush("HeaderBackgroundBrush", "#ffffff");
                SetThemeBrush("PanelBackgroundBrush", "#ffffff");
                SetThemeBrush("SurfaceBackgroundBrush", "#ffffff");
                SetThemeBrush("ControlBackgroundBrush", "#ffffff");
                SetThemeBrush("ControlHoverBrush", "#edf4fb");
                SetThemeBrush("ControlPressedBrush", "#dcecf8");
                SetThemeBrush("SelectedBackgroundBrush", "#d9edf8");
                SetThemeBrush("BorderLineBrush", "#d8dee6");
                SetThemeBrush("TextBrush", "#1f2933");
                SetThemeBrush("MutedTextBrush", "#52616f");
                SetThemeBrush("DisabledTextBrush", "#758392");
                SetThemeBrush("WarningTextBrush", "#7a4f00");
                SetThemeBrush("GridBackgroundBrush", "#ffffff");
                SetThemeBrush("GridHeaderBackgroundBrush", "#f4f6f8");
                SetThemeBrush("GridLineBrush", "#dbe3eb");
                SetThemeBrush("ProgressTrackBrush", "#d8dee6");
                SetThemeBrush("ProgressFillBrush", "#1f9d35");
                SetThemeBrush("ProgressMarqueeBrush", "#46a4ff");
            }
        }

        private void SetThemeBrush(string key, string color)
        {
            var brush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
            brush.Freeze();
            Resources[key] = brush;
        }

        private static bool IsSystemUsingLightTheme()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    object value = key == null ? null : key.GetValue("AppsUseLightTheme");
                    if (value is int)
                        return (int)value != 0;

                    int parsed;
                    if (value != null && int.TryParse(value.ToString(), out parsed))
                        return parsed != 0;
                }
            }
            catch
            {
            }

            return true;
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (themeMode != WpfThemeMode.System)
                return;

            if (e.Category == UserPreferenceCategory.Color || e.Category == UserPreferenceCategory.General)
                Dispatcher.BeginInvoke(new Action(ApplyTheme));
        }

        private void ApplyActionButtonIcons()
        {
            foreach (WpfActionButtonSpec spec in WpfActionButtonSpec.CreateDefault())
            {
                Controls.Button button = GetActionButton(spec.Kind);
                button.Content = CreateActionIcon(spec.Glyph);
            }

            RefreshActionButtonTooltips();
        }

        private Controls.Button GetActionButton(WpfActionButtonKind kind)
        {
            switch (kind)
            {
                case WpfActionButtonKind.Refresh: return RefreshActionButton;
                case WpfActionButtonKind.Search: return SearchActionButton;
                case WpfActionButtonKind.Download: return DownloadActionButton;
                case WpfActionButtonKind.Install: return InstallActionButton;
                case WpfActionButtonKind.Uninstall: return UninstallActionButton;
                case WpfActionButtonKind.Hide: return HideActionButton;
                case WpfActionButtonKind.GetLinks: return GetLinksActionButton;
                case WpfActionButtonKind.Cancel: return CancelActionButton;
                default: throw new ArgumentOutOfRangeException("kind");
            }
        }

        private string GetActionButtonText(WpfActionButtonKind kind)
        {
            switch (kind)
            {
                case WpfActionButtonKind.Refresh: return text.RefreshButton;
                case WpfActionButtonKind.Search: return text.SearchButton;
                case WpfActionButtonKind.Download: return text.DownloadButton;
                case WpfActionButtonKind.Install: return text.InstallButton;
                case WpfActionButtonKind.Uninstall: return text.UninstallButton;
                case WpfActionButtonKind.Hide: return HideButtonText;
                case WpfActionButtonKind.GetLinks: return text.GetLinksButton;
                case WpfActionButtonKind.Cancel: return text.CancelButton;
                default: throw new ArgumentOutOfRangeException("kind");
            }
        }

        private void RefreshActionButtonTooltips()
        {
            foreach (WpfActionButtonSpec spec in WpfActionButtonSpec.CreateDefault())
            {
                Controls.Button button = GetActionButton(spec.Kind);
                string label = GetActionButtonText(spec.Kind);
                button.ToolTip = label;
                System.Windows.Automation.AutomationProperties.SetName(button, label);
            }
        }

        private static Controls.TextBlock CreateActionIcon(string glyph)
        {
            var icon = new Controls.TextBlock
            {
                Text = glyph,
                FontFamily = new FontFamily(WpfActionButtonSpec.IconFontFamily),
                FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            icon.SetResourceReference(Controls.TextBlock.ForegroundProperty, "TextBrush");
            return icon;
        }

        private void WuMgrWpfWindow_Closing(object sender, CancelEventArgs e)
        {
            if (ShouldUseTray() && allowShowDisplay)
            {
                e.Cancel = true;
                allowShowDisplay = false;
                Hide();
                return;
            }

            SaveWindowSettings();
        }

        private void WuMgrWpfWindow_Closed(object sender, EventArgs e)
        {
            Program.Agent.Progress -= Agent_Progress;
            Program.Agent.UpdatesChaged -= Agent_UpdatesChanged;
            Program.Agent.Finished -= Agent_Finished;
            ContentRendered -= WuMgrWpfWindow_ContentRendered;
            StateChanged -= WuMgrWpfWindow_StateChanged;
            Program.ipc.PipeMessage -= PipesMessageHandler;
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;

            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                notifyIcon = null;
            }

            if (autoUpdateTimer != null)
            {
                autoUpdateTimer.Stop();
                autoUpdateTimer = null;
            }

            if (progressAnimationTimer != null)
            {
                progressAnimationTimer.Stop();
                progressAnimationTimer.Tick -= ProgressAnimationTimer_Tick;
                progressAnimationTimer = null;
            }
        }

        private void WuMgrWpfWindow_ContentRendered(object sender, EventArgs e)
        {
            InitializeAgentAfterStartup();
        }

        private void WuMgrWpfWindow_StateChanged(object sender, EventArgs e)
        {
            UpdateMaximizeRestoreButton();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            if (e.ClickCount == 2)
            {
                ToggleMaximizeRestore();
                return;
            }

            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreWindow_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximizeRestore();
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleMaximizeRestore()
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        public void InitializeAgentAfterStartup()
        {
            if (agentInitializationStarted)
                return;

            agentInitializationStarted = true;
            Dispatcher.BeginInvoke(new Action(InitializeAgentAfterWindowShown), DispatcherPriority.ApplicationIdle);
        }

        private void InitializeAgentAfterWindowShown()
        {
            if (!Program.Agent.IsActive())
            {
                StatusText = text.InitializingAgent;
                Program.Agent.Init();
            }

            RefreshAgentBackedState();
            StatusText = "";
        }

        private void RefreshAgentBackedState()
        {
            string source = SelectedSource;
            registerMicrosoftUpdate = Program.Agent.IsActive() && Program.Agent.TestService(WuAgent.MsUpdGUID);
            OnPropertyChanged("RegisterMicrosoftUpdate");
            LoadSources(string.IsNullOrEmpty(source) ? GetConfig("Source", "Windows Update") : source);
            LoadList();
            NotifyAllStateChanged();
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

        public void ShowMainWindow()
        {
            allowShowDisplay = true;
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
            Show();
            ShowInTaskbar = true;
            Activate();
        }

        private void CreateNotifyIcon()
        {
            notifyIcon = new Forms.NotifyIcon();
            notifyIcon.Text = Program.mName;
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
            notifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked;

            Forms.MenuItem open = new Forms.MenuItem(text.OpenMenu, NotifyIcon_Open);
            Forms.MenuItem exit = new Forms.MenuItem(text.ExitMenu, NotifyIcon_Exit);
            notifyIcon.ContextMenu = new Forms.ContextMenu(new Forms.MenuItem[] { open, exit });
            UpdateNotifyIcon();
        }

        private void CreateAutoUpdateTimer()
        {
            autoUpdateTimer = new DispatcherTimer();
            autoUpdateTimer.Interval = TimeSpan.FromSeconds(30);
            autoUpdateTimer.Tick += AutoUpdateTimer_Tick;
            autoUpdateTimer.Start();
        }

        private void LoadAutoUpdateOptions()
        {
            AutoUpdateOptions.Clear();
            foreach (string option in text.AutoUpdateOptions)
                AutoUpdateOptions.Add(option);

            if (selectedAutoUpdateIndex < 0 || selectedAutoUpdateIndex >= AutoUpdateOptions.Count)
                selectedAutoUpdateIndex = 0;
        }

        private void LoadScheduleOptions()
        {
            ScheduleDays.Clear();
            foreach (string day in text.ScheduleDays)
                ScheduleDays.Add(day);

            ScheduleTimes.Clear();
            for (int hour = 0; hour < 24; hour++)
                ScheduleTimes.Add(hour.ToString("00", CultureInfo.InvariantCulture) + ":00");
        }

        private void LoadPolicyOptions()
        {
            suspendPolicyUpdate = true;
            try
            {
                IncludeDriversInUpdates = FromGpoCheckState(GPO.GetDriverAU());

                gpoRespect = GPO.GetRespect();
                windowsVersion = GPO.GetWinVersion();
                if (gpoRespect == GPO.Respect.Unknown)
                    AppendLog("Unrecognized Windows Edition, respect for GPO settings is unknown.");

                HideWindowsUpdatePage = GPO.IsUpdatePageHidden();
                BlockMicrosoftServers = FromGpoCheckState(GPO.GetBlockMS());

                int day;
                int time;
                selectedPolicyAutoUpdate = GPO.GetAU(out day, out time);
                selectedScheduleDay = ClampIndex(day, ScheduleDays.Count);
                selectedScheduleTime = ClampIndex(time, ScheduleTimes.Count);

                DisableUpdateFacilitators = windowsVersion >= 10.0f && GPO.GetDisableAU();
                DisableStoreAutoUpdate = GPO.GetStoreAU();
                CoercePolicyDerivedState();
            }
            finally
            {
                suspendPolicyUpdate = false;
            }
        }

        private void LoadLastCheck()
        {
            DateTime parsed;
            if (DateTime.TryParse(GetConfig("LastCheck", ""), out parsed))
                lastCheck = parsed;
            else
                lastCheck = DateTime.Now;
        }

        private bool HandleBlockMicrosoftServersChanged(bool? previous)
        {
            if (selectedPolicyAutoUpdate == GPO.AUOptions.Disabled
                && gpoRespect == GPO.Respect.Partial
                && BlockMicrosoftServers != true
                && !DisableUpdateFacilitators)
            {
                switch (MessageBox.Show(Translate.fmt("msg_gpo"), Program.mName, MessageBoxButton.YesNoCancel))
                {
                    case MessageBoxResult.Yes:
                        SetDisableUpdateFacilitators(true, true);
                        break;
                    case MessageBoxResult.No:
                        SetSelectedPolicyAutoUpdate(GPO.AUOptions.Default);
                        break;
                    case MessageBoxResult.Cancel:
                        blockMicrosoftServers = previous;
                        OnPropertyChanged("BlockMicrosoftServers");
                        NotifyPolicyOptionStateChanged();
                        return false;
                }
            }

            if (IsAdministrator)
                GPO.BlockMS(BlockMicrosoftServers == true);

            return true;
        }

        private void SetSelectedPolicyAutoUpdate(GPO.AUOptions option)
        {
            if (selectedPolicyAutoUpdate == option)
                return;

            selectedPolicyAutoUpdate = option;
            NotifyPolicySelectionChanged();

            if (option != GPO.AUOptions.Disabled && disableUpdateFacilitators)
                SetDisableUpdateFacilitators(false, !suspendPolicyUpdate);

            CoercePolicyDerivedState();
            NotifyPolicyOptionStateChanged();

            if (!suspendPolicyUpdate && IsAdministrator)
                ApplySelectedAutoUpdatePolicy();
        }

        private void ApplySelectedAutoUpdatePolicy()
        {
            if (selectedPolicyAutoUpdate == GPO.AUOptions.Disabled)
            {
                if (DisableUpdateFacilitators)
                    ApplyDisableFacilitators(true);
                GPO.ConfigAU(GPO.AUOptions.Disabled);
                return;
            }

            if (selectedPolicyAutoUpdate == GPO.AUOptions.Notification)
                GPO.ConfigAU(GPO.AUOptions.Notification);
            else if (selectedPolicyAutoUpdate == GPO.AUOptions.Download)
                GPO.ConfigAU(GPO.AUOptions.Download);
            else if (selectedPolicyAutoUpdate == GPO.AUOptions.Scheduled)
                GPO.ConfigAU(GPO.AUOptions.Scheduled, SelectedScheduleDay, SelectedScheduleTime);
            else
                GPO.ConfigAU(GPO.AUOptions.Default);
        }

        private void ApplyScheduledPolicyIfNeeded()
        {
            if (!suspendPolicyUpdate && IsAdministrator && selectedPolicyAutoUpdate == GPO.AUOptions.Scheduled)
                GPO.ConfigAU(GPO.AUOptions.Scheduled, SelectedScheduleDay, SelectedScheduleTime);
        }

        private void SetDisableUpdateFacilitators(bool value, bool applyPolicy)
        {
            WpfPolicyOptionState state = CurrentPolicyOptionState;
            if (state.DisableFacilitatorsForcedOn)
                value = true;

            if (!SetField(ref disableUpdateFacilitators, value, "DisableUpdateFacilitators") && !applyPolicy)
                return;

            if (value)
                SetHideWindowsUpdatePage(true, applyPolicy);

            NotifyPolicyOptionStateChanged();

            if (applyPolicy && IsAdministrator)
                ApplyDisableFacilitators(value);
        }

        private void ApplyDisableFacilitators(bool value)
        {
            bool previous = GPO.GetDisableAU();
            GPO.DisableAU(value);
            if (previous != value)
                MessageBox.Show(Translate.fmt("msg_disable_au"), Program.mName);
        }

        private void SetHideWindowsUpdatePage(bool value, bool applyPolicy)
        {
            WpfPolicyOptionState state = CurrentPolicyOptionState;
            if (state.HideWindowsUpdatePageForcedOn)
                value = true;

            if (!SetField(ref hideWindowsUpdatePage, value, "HideWindowsUpdatePage") && !applyPolicy)
                return;

            NotifyPolicyOptionStateChanged();

            if (applyPolicy && IsAdministrator)
                GPO.HideUpdatePage(value);
        }

        private void CoercePolicyDerivedState()
        {
            WpfPolicyOptionState state = CurrentPolicyOptionState;

            if (state.DisableFacilitatorsForcedOn && !disableUpdateFacilitators)
            {
                disableUpdateFacilitators = true;
                OnPropertyChanged("DisableUpdateFacilitators");
            }

            if (state.HideWindowsUpdatePageForcedOn && !hideWindowsUpdatePage)
            {
                hideWindowsUpdatePage = true;
                OnPropertyChanged("HideWindowsUpdatePage");
            }
        }

        private static bool? FromGpoCheckState(int checkState)
        {
            if (checkState == 1)
                return true;
            if (checkState == 0)
                return false;
            return null;
        }

        private static int ToGpoCheckState(bool? checkState)
        {
            if (checkState == true)
                return 1;
            if (checkState == false)
                return 0;
            return 2;
        }

        private static int ClampIndex(int value, int count)
        {
            if (count <= 0 || value < 0 || value >= count)
                return 0;
            return value;
        }

        private void UpdateNotifyIcon()
        {
            if (notifyIcon != null)
                notifyIcon.Visible = ShouldUseTray();
        }

        private bool ShouldUseTray()
        {
            return RunInBackground || StartupUiMode.ShouldStartInTray(Program.args);
        }

        private void NotifyIcon_MouseDoubleClick(object sender, Forms.MouseEventArgs e)
        {
            ShowMainWindow();
        }

        private void NotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            ShowMainWindow();
        }

        private void NotifyIcon_Open(object sender, EventArgs e)
        {
            ShowMainWindow();
        }

        private void NotifyIcon_Exit(object sender, EventArgs e)
        {
            allowShowDisplay = false;
            Close();
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
                if (!WpfUpdateFilter.Matches(row, SearchFilter))
                    continue;

                row.PropertyChanged += UpdateRow_PropertyChanged;
                Updates.Add(row);
            }

            UpdateSelectionControls();
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
            {
                UpdateSelectionControls();
                NotifyActionStateChanged();
            }
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
            AppendLog(WpfStatusText.CurrentListRefreshed);
        }

        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            SearchFilter = "";
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (!Program.Agent.IsActive() || Program.Agent.IsBusy())
                return;

            StartSearch();
        }

        private void StartSearch()
        {
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
            if (!ConfirmCancelOperation())
                return;

            Program.Agent.CancelOperations();
        }

        private bool ConfirmCancelOperation()
        {
            System.Media.SystemSounds.Exclamation.Play();
            return MessageBox.Show(this, CancelConfirmation.Message, CancelConfirmation.Title, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes;
        }

        private void SelectAllRows_Click(object sender, RoutedEventArgs e)
        {
            if (suppressSelectAllRowsChange || !WpfListSelectionPolicy.CanSelectRows(currentList))
                return;

            bool select = selectAllRowsState != true;
            foreach (WpfUpdateRow row in Updates)
                row.Selected = select;

            UpdateSelectionControls();
            NotifyActionStateChanged();
        }

        private void UpdateSelectionControls()
        {
            if (SelectionColumn == null || SelectAllRowsCheckBox == null)
                return;

            bool canSelectRows = WpfListSelectionPolicy.CanSelectRows(currentList);
            SelectionColumn.Visibility = canSelectRows ? Visibility.Visible : Visibility.Collapsed;
            SelectAllRowsCheckBox.IsEnabled = canSelectRows && Updates.Count > 0;
            SelectAllRowsCheckBox.ToolTip = text.SelectAllRows;
            System.Windows.Automation.AutomationProperties.SetName(SelectAllRowsCheckBox, text.SelectAllRows);

            bool? checkState = false;
            if (canSelectRows && Updates.Count > 0)
            {
                int selected = Updates.Count(update => update.Selected);
                if (selected == Updates.Count)
                    checkState = true;
                else if (selected > 0)
                    checkState = null;
            }

            suppressSelectAllRowsChange = true;
            try
            {
                selectAllRowsState = checkState;
                SelectAllRowsCheckBox.IsChecked = checkState;
            }
            finally
            {
                suppressSelectAllRowsChange = false;
            }
        }

        private void ProgressTrack_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateProgressFill();
        }

        private void CreateProgressAnimationTimer()
        {
            progressAnimationTimer = new DispatcherTimer();
            progressAnimationTimer.Interval = TimeSpan.FromMilliseconds(90);
            progressAnimationTimer.Tick += ProgressAnimationTimer_Tick;
        }

        private void ProgressAnimationTimer_Tick(object sender, EventArgs e)
        {
            progressAnimationPhase += 0.04;
            if (progressAnimationPhase > 1.0)
                progressAnimationPhase = 0.0;

            UpdateProgressFill();
        }

        private void UpdateProgressAnimationState()
        {
            if (progressAnimationTimer == null)
                return;

            if (isBusyIndeterminate)
            {
                if (!progressAnimationTimer.IsEnabled)
                    progressAnimationTimer.Start();
            }
            else
            {
                progressAnimationTimer.Stop();
                progressAnimationPhase = 0.0;
            }
        }

        private void UpdateProgressFill()
        {
            if (ProgressTrack == null || ProgressFill == null || ProgressMarquee == null || ProgressLabel == null)
                return;

            double width = isBusyIndeterminate ? 0.0 : WpfProgressValue.GetFillWidth(ProgressTrack.ActualWidth, totalPercent, false);
            ProgressFill.Width = width;
            ProgressFill.Visibility = width > 0 ? Visibility.Visible : Visibility.Collapsed;

            double marqueeWidth = WpfProgressValue.GetMarqueeWidth(ProgressTrack.ActualWidth);
            ProgressMarquee.Width = marqueeWidth;
            ProgressMarquee.Visibility = isBusyIndeterminate && marqueeWidth > 0 ? Visibility.Visible : Visibility.Collapsed;
            ProgressMarqueeTransform.X = WpfProgressValue.GetMarqueeLeft(ProgressTrack.ActualWidth, marqueeWidth, progressAnimationPhase);
            ProgressLabel.Text = WpfProgressValue.GetDisplayText(totalPercent, isBusyIndeterminate);
        }

        private void LoadWindowSettings()
        {
            WpfWindowPlacement placement;
            if (!WpfWindowPlacement.TryCreate(
                    GetConfig("WindowLeft", ""),
                    GetConfig("WindowTop", ""),
                    GetConfig("WindowWidth", ""),
                    GetConfig("WindowHeight", ""),
                    GetConfig("WindowState", ""),
                    MinWidth,
                    MinHeight,
                    out placement))
                return;

            System.Drawing.Rectangle bounds = new System.Drawing.Rectangle((int)placement.Left, (int)placement.Top, (int)placement.Width, (int)placement.Height);
            bool visibleOnAnyScreen = Forms.Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(bounds));
            if (!visibleOnAnyScreen)
            {
                System.Drawing.Rectangle workingArea = Forms.Screen.PrimaryScreen.WorkingArea;
                placement.Left = workingArea.Left;
                placement.Top = workingArea.Top;
            }

            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = placement.Left;
            Top = placement.Top;
            Width = placement.Width;
            Height = placement.Height;

            if (placement.Maximized)
                WindowState = WindowState.Maximized;
        }

        private void SaveWindowSettings()
        {
            WindowState state = WindowState == WindowState.Minimized ? WindowState.Normal : WindowState;
            Rect bounds = state == WindowState.Normal ? new Rect(Left, Top, Width, Height) : RestoreBounds;

            SetConfig("WindowLeft", bounds.Left.ToString("0", CultureInfo.InvariantCulture));
            SetConfig("WindowTop", bounds.Top.ToString("0", CultureInfo.InvariantCulture));
            SetConfig("WindowWidth", bounds.Width.ToString("0", CultureInfo.InvariantCulture));
            SetConfig("WindowHeight", bounds.Height.ToString("0", CultureInfo.InvariantCulture));
            SetConfig("WindowState", state.ToString());
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
                    lastCheck = DateTime.Now;
                    SetConfig("LastCheck", lastCheck.ToString());
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

        private void AutoUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (!RunInBackground || !Program.Agent.IsActive())
                return;

            AutoUpdateMode mode = GetAutoUpdateMode();
            int daysDue = AutoUpdateSchedule.GetDueDays(mode, lastCheck, DateTime.Now);
            if (daysDue != 0 && !Program.Agent.IsBusy())
            {
                uint idleTime = MiscFunc.GetIdleTime();
                if (idleDelay * 60 < idleTime)
                {
                    AppendLog("Starting automatic search for updates.");
                    StartSearch();
                    return;
                }

                if (daysDue > AutoUpdateSchedule.GetGraceDays(mode))
                    ShowBalloon(Translate.fmt("cap_chk_upd"), Translate.fmt("msg_chk_upd", Program.mName, daysDue), Forms.ToolTipIcon.Warning);
            }

            if (Program.Agent.mPendingUpdates.Count > 0)
                ShowBalloon(Translate.fmt("cap_new_upd"), GetPendingUpdatesBalloonText(), Forms.ToolTipIcon.Info);
        }

        private AutoUpdateMode GetAutoUpdateMode()
        {
            if (SelectedAutoUpdateIndex < 0 || SelectedAutoUpdateIndex > (int)AutoUpdateMode.EveryMonth)
                return AutoUpdateMode.No;

            return (AutoUpdateMode)SelectedAutoUpdateIndex;
        }

        private void ShowBalloon(string title, string text, Forms.ToolTipIcon icon)
        {
            if (notifyIcon == null || !notifyIcon.Visible)
                return;

            if (LastBalloonWasRecent())
                return;

            lastBalloon = DateTime.Now;
            notifyIcon.ShowBalloonTip(int.MaxValue, title, text, icon);
        }

        private bool LastBalloonWasRecent()
        {
            return lastBalloon >= DateTime.Now.AddHours(-4);
        }

        private string GetPendingUpdatesBalloonText()
        {
            string text = Translate.fmt("msg_new_upd", Program.mName, Program.Agent.mPendingUpdates.Count);
            List<string> titles = Program.Agent.mPendingUpdates
                .Take(5)
                .Select(update => "- " + update.Title)
                .ToList();

            if (titles.Count > 0)
                text += Environment.NewLine + string.Join(Environment.NewLine, titles.ToArray());

            int remaining = Program.Agent.mPendingUpdates.Count - titles.Count;
            if (remaining > 0)
                text += Environment.NewLine + string.Format("...and {0} more", remaining);

            return text;
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
            OnPropertyChanged("RunInBackground");
            OnPropertyChanged("CanChangeRunInBackground");
            OnPropertyChanged("SelectedAutoUpdateIndex");
            OnPropertyChanged("SelectedSource");
            OnPropertyChanged("CanUseOnlineSource");
            OnPropertyChanged("BlockMicrosoftServers");
            OnPropertyChanged("DisableUpdateFacilitators");
            OnPropertyChanged("HideWindowsUpdatePage");
            OnPropertyChanged("DisableStoreAutoUpdate");
            OnPropertyChanged("IncludeDriversInUpdates");
            OnPropertyChanged("SelectedScheduleDay");
            OnPropertyChanged("SelectedScheduleTime");
            OnPropertyChanged("PendingLabel");
            OnPropertyChanged("InstalledLabel");
            OnPropertyChanged("HiddenLabel");
            OnPropertyChanged("HistoryLabel");
            OnPropertyChanged("IsPendingList");
            OnPropertyChanged("IsInstalledList");
            OnPropertyChanged("IsHiddenList");
            OnPropertyChanged("IsHistoryList");
            OnPropertyChanged("HideButtonText");
            RefreshActionButtonTooltips();
            NotifyPolicySelectionChanged();
            NotifyPolicyOptionStateChanged();
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

        private void NotifyPolicySelectionChanged()
        {
            OnPropertyChanged("IsPolicyDefault");
            OnPropertyChanged("IsPolicyDisabled");
            OnPropertyChanged("IsPolicyNotification");
            OnPropertyChanged("IsPolicyDownload");
            OnPropertyChanged("IsPolicyScheduled");
        }

        private void NotifyPolicyOptionStateChanged()
        {
            OnPropertyChanged("CanChangeBlockMicrosoft");
            OnPropertyChanged("CanSelectNotification");
            OnPropertyChanged("CanSelectDownload");
            OnPropertyChanged("CanSelectScheduled");
            OnPropertyChanged("CanChangeSchedule");
            OnPropertyChanged("CanChangeDisableFacilitators");
            OnPropertyChanged("CanChangeHideWindowsUpdatePage");
            OnPropertyChanged("CanChangeStoreAutoUpdate");
            OnPropertyChanged("CanChangeDrivers");
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
            Date = UpdateDateFormatter.FormatForDisplay(update.Date);
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
