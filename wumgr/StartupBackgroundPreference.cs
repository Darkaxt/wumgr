namespace wumgr
{
    static class StartupBackgroundPreference
    {
        public const string ConfigKey = "RunInBackground";

        public static bool IsEnabled(string configuredValue, bool hasStartupEntry)
        {
            if (configuredValue == "1")
                return true;

            if (configuredValue == "0")
                return false;

            return hasStartupEntry;
        }

        public static string ToConfigValue(bool enabled)
        {
            return enabled ? "1" : "0";
        }
    }
}
