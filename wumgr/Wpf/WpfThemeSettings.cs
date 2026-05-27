using System;

namespace wumgr.Wpf
{
    internal enum WpfThemeMode
    {
        System,
        Light,
        Dark
    }

    internal static class WpfThemeSettings
    {
        public const string ConfigKey = "Theme";

        public static WpfThemeMode Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return WpfThemeMode.System;

            switch (value.Trim().ToLowerInvariant())
            {
                case "light":
                    return WpfThemeMode.Light;
                case "dark":
                    return WpfThemeMode.Dark;
                case "system":
                    return WpfThemeMode.System;
                default:
                    return WpfThemeMode.System;
            }
        }

        public static string ToConfigValue(WpfThemeMode mode)
        {
            switch (mode)
            {
                case WpfThemeMode.Light:
                    return "light";
                case WpfThemeMode.Dark:
                    return "dark";
                default:
                    return "system";
            }
        }

        public static int ToSelectedIndex(WpfThemeMode mode)
        {
            switch (mode)
            {
                case WpfThemeMode.Light:
                    return 1;
                case WpfThemeMode.Dark:
                    return 2;
                default:
                    return 0;
            }
        }

        public static WpfThemeMode FromSelectedIndex(int index)
        {
            switch (index)
            {
                case 1:
                    return WpfThemeMode.Light;
                case 2:
                    return WpfThemeMode.Dark;
                default:
                    return WpfThemeMode.System;
            }
        }

        public static WpfThemeMode ResolveEffectiveMode(WpfThemeMode mode, bool systemUsesLightTheme)
        {
            if (mode == WpfThemeMode.System)
                return systemUsesLightTheme ? WpfThemeMode.Light : WpfThemeMode.Dark;
            return mode;
        }
    }
}
