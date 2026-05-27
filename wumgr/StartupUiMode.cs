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
        public const string StartMinimizedConfigKey = "StartMinimized";

        public static StartupUiKind Select(string[] args)
        {
            if (HasArg(args, "-winforms") || HasArg(args, "/winforms"))
                return StartupUiKind.WinForms;

            return StartupUiKind.Wpf;
        }

        public static bool ShouldInitializeAgentBeforeWindow(StartupUiKind uiKind)
        {
            return uiKind == StartupUiKind.WinForms;
        }

        public static bool ShouldStartInTray(string[] args)
        {
            return HasArg(args, "-tray") || HasArg(args, "/tray");
        }

        public static bool ShouldSearchOnStartup(string[] args)
        {
            return HasArg(args, "-update") || HasArg(args, "/update");
        }

        public static bool ShouldStartMinimized(string[] args, bool configured)
        {
            if (ShouldStartInTray(args))
                return false;

            return configured || HasArg(args, "-minimized") || HasArg(args, "/minimized");
        }

        public static bool IsStartMinimizedConfigured()
        {
            return MiscFunc.parseInt(Program.IniReadValue("Options", StartMinimizedConfigKey, "0")) != 0;
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
