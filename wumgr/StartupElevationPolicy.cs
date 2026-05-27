namespace wumgr
{
    internal static class StartupElevationPolicy
    {
        public static bool ShouldAttemptStartupElevation(bool isAdministrator, bool isDebugging, bool skipUacConfigured)
        {
            return !isAdministrator && !isDebugging && skipUacConfigured;
        }
    }
}
