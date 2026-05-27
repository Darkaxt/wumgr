namespace wumgr.Wpf
{
    internal static class WpfListSelectionPolicy
    {
        public static bool CanSelectRows(WpfUpdateListKind list)
        {
            return list != WpfUpdateListKind.History;
        }
    }
}
