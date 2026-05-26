using System.Windows.Forms;

namespace wumgr
{
    public static class UpdateSelectionHelper
    {
        public static bool SetAllChecked(ListView.ListViewItemCollection items, bool isChecked)
        {
            bool changed = false;
            foreach (ListViewItem item in items)
            {
                if (item.Checked != isChecked)
                    changed = true;
                item.Checked = isChecked;
            }
            return changed;
        }
    }
}
