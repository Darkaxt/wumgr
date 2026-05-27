using System;

namespace wumgr
{
    internal enum StartupUiKind
    {
        Wpf,
        WinForms
    }

    internal static class StartupUiMode
    {
        public static StartupUiKind Select(string[] args)
        {
            if (HasArg(args, "-winforms") || HasArg(args, "/winforms"))
                return StartupUiKind.WinForms;

            return StartupUiKind.Wpf;
        }

        private static bool HasArg(string[] args, string name)
        {
            if (args == null)
                return false;

            foreach (string arg in args)
            {
                if (arg != null && arg.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
