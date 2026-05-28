using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace wumgr.Wpf
{
    public sealed class WpfUpdateGroup : INotifyPropertyChanged
    {
        private bool isExpanded = true;
        private bool updatingRows;
        private bool? selectedState;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Category { get; private set; }
        public ObservableCollection<WpfUpdateRow> Rows { get; private set; }
        public int Count { get { return Rows.Count; } }
        public string ExpandGlyph { get { return IsExpanded ? "\uE70D" : "\uE70E"; } }
        public Visibility RowsVisibility { get { return IsExpanded ? Visibility.Visible : Visibility.Collapsed; } }

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (isExpanded == value)
                    return;

                isExpanded = value;
                OnPropertyChanged("IsExpanded");
                OnPropertyChanged("ExpandGlyph");
                OnPropertyChanged("RowsVisibility");
            }
        }

        public bool? SelectedState
        {
            get { return selectedState; }
            private set
            {
                if (selectedState == value)
                    return;

                selectedState = value;
                OnPropertyChanged("SelectedState");
            }
        }

        public WpfUpdateGroup(string category, IEnumerable<WpfUpdateRow> rows)
        {
            Category = category;
            Rows = new ObservableCollection<WpfUpdateRow>();
            foreach (WpfUpdateRow row in rows)
            {
                row.PropertyChanged += Row_PropertyChanged;
                Rows.Add(row);
            }

            RefreshSelectedState();
        }

        public void SetSelected(bool selected)
        {
            updatingRows = true;
            try
            {
                foreach (WpfUpdateRow row in Rows)
                    row.Selected = selected;
            }
            finally
            {
                updatingRows = false;
            }

            RefreshSelectedState();
        }

        private void Row_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Selected" && !updatingRows)
                RefreshSelectedState();
        }

        private void RefreshSelectedState()
        {
            if (Rows.Count == 0)
            {
                SelectedState = false;
                return;
            }

            int selected = 0;
            foreach (WpfUpdateRow row in Rows)
            {
                if (row.Selected)
                    selected++;
            }

            if (selected == 0)
                SelectedState = false;
            else if (selected == Rows.Count)
                SelectedState = true;
            else
                SelectedState = null;
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal static class WpfUpdateGroupBuilder
    {
        public const string OtherCategoryName = "Other Updates";

        public static ObservableCollection<WpfUpdateGroup> Create(IEnumerable<WpfUpdateRow> rows)
        {
            ObservableCollection<WpfUpdateGroup> groups = new ObservableCollection<WpfUpdateGroup>();
            List<string> categoryOrder = new List<string>();
            Dictionary<string, List<WpfUpdateRow>> groupedRows = new Dictionary<string, List<WpfUpdateRow>>();

            foreach (WpfUpdateRow row in rows)
            {
                string category = NormalizeCategory(row.Category);
                if (!groupedRows.ContainsKey(category))
                {
                    groupedRows.Add(category, new List<WpfUpdateRow>());
                    categoryOrder.Add(category);
                }

                groupedRows[category].Add(row);
            }

            foreach (string category in categoryOrder)
                groups.Add(new WpfUpdateGroup(category, groupedRows[category]));

            return groups;
        }

        public static bool UseGroupedCards(WpfUpdateListKind list)
        {
            return list == WpfUpdateListKind.Pending || list == WpfUpdateListKind.Installed || list == WpfUpdateListKind.Hidden;
        }

        private static string NormalizeCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return OtherCategoryName;

            return category.Trim();
        }
    }

    internal static class WpfUpdateGroupSelection
    {
        public static void SetAll(IEnumerable<WpfUpdateGroup> groups, bool selected)
        {
            foreach (WpfUpdateGroup group in groups)
                group.SetSelected(selected);
        }
    }
}
