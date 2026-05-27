using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace wumgr.Wpf
{
    public partial class WuMgrWpfWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<WpfUpdateRow> Updates { get; private set; }

        public string VersionText { get { return "v" + Program.mVersion + " preview"; } }
        public string ElevationText { get { return IsAdministrator ? "Running elevated" : "Read-only launch. Admin actions require elevation."; } }
        public string PendingLabel { get { return string.Format("Windows Update ({0})", Program.Agent.mPendingUpdates.Count); } }
        public string InstalledLabel { get { return string.Format("Installed Updates ({0})", Program.Agent.mInstalledUpdates.Count); } }
        public string HiddenLabel { get { return string.Format("Hidden Updates ({0})", Program.Agent.mHiddenUpdates.Count); } }
        public string HistoryLabel { get { return string.Format("Update History ({0})", Program.Agent.mUpdateHistory.Count); } }
        public bool IsAdministrator { get; private set; }
        public bool HasSelection { get { return Updates.Any(update => update.Selected); } }
        public bool CanRunAdminSelectionAction { get { return IsAdministrator && HasSelection; } }
        public bool SkipUacEnabled { get; set; }
        public bool OfflineMode { get; set; }
        public bool ManualMode { get; set; }
        public bool IncludeSuperseded { get; set; }
        public bool RegisterMicrosoftUpdate { get; set; }
        public string StatusLog { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public WuMgrWpfWindow()
        {
            Updates = new ObservableCollection<WpfUpdateRow>();
            IsAdministrator = MiscFunc.IsAdministrator();
            SkipUacEnabled = Program.IsSkipUacRun();
            OfflineMode = MiscFunc.parseInt(Program.IniReadValue("Options", "Offline", "0")) != 0;
            ManualMode = MiscFunc.parseInt(Program.IniReadValue("Options", "Manual", "0")) != 0;
            IncludeSuperseded = MiscFunc.parseInt(Program.IniReadValue("Options", "IncludeOld", "0")) != 0;
            RegisterMicrosoftUpdate = Program.Agent.IsActive() && Program.Agent.TestService(WuAgent.MsUpdGUID);
            StatusLog = "WPF preview shell loaded.\r\nUse the default WinForms UI for full update operations while migration continues.";

            InitializeComponent();
            DataContext = this;
            LoadPendingUpdates();
        }

        private void LoadPendingUpdates()
        {
            Updates.Clear();
            foreach (MsUpdate update in Program.Agent.mPendingUpdates)
            {
                WpfUpdateRow row = new WpfUpdateRow(update);
                row.PropertyChanged += UpdateRow_PropertyChanged;
                Updates.Add(row);
            }

            NotifyStateChanged();
        }

        private void UpdateRow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Selected")
                NotifyStateChanged();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadPendingUpdates();
            StatusLog = "WPF preview list refreshed from the current agent cache.";
            OnPropertyChanged("StatusLog");
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Search is still handled by the WinForms UI during the WPF migration.", Program.mName);
        }

        private void PreviewOnly_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This WPF shell is a migration preview. Use the default WinForms UI for this operation.", Program.mName);
        }

        private void OpenWinForms_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Close this preview and launch WuMgr without -wpf to use the full WinForms UI.", Program.mName);
        }

        private void NotifyStateChanged()
        {
            OnPropertyChanged("PendingLabel");
            OnPropertyChanged("InstalledLabel");
            OnPropertyChanged("HiddenLabel");
            OnPropertyChanged("HistoryLabel");
            OnPropertyChanged("HasSelection");
            OnPropertyChanged("CanRunAdminSelectionAction");
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
            Title = update.Title;
            Category = update.Category;
            KB = update.KB;
            Date = update.Date == DateTime.MinValue ? "" : update.Date.ToShortDateString();
            Size = FormatSize(update.Size);
            State = update.State.ToString();
        }

        private static string FormatSize(decimal bytes)
        {
            if (bytes <= 0)
                return "";

            decimal megabytes = bytes / 1024 / 1024;
            if (megabytes < 1024)
                return string.Format("{0:0.##} MB", megabytes);

            return string.Format("{0:0.##} GB", megabytes / 1024);
        }
    }
}
