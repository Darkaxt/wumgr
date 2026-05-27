namespace wumgr
{
    class WuaSearchCriteria
    {
        private const string InstalledOrHidden =
            "(IsInstalled = 0 and IsHidden = 0) or (IsInstalled = 1 and IsHidden = 0) or (IsHidden = 1)";

        private const string OptionalInstalledOrHidden =
            "(IsInstalled = 0 and IsHidden = 0 and DeploymentAction='OptionalInstallation') or " +
            "(IsInstalled = 1 and IsHidden = 0 and DeploymentAction='OptionalInstallation') or " +
            "(IsHidden = 1 and DeploymentAction='OptionalInstallation')";

        public static string Create(bool isWindows7OrLower)
        {
            if (isWindows7OrLower)
                return InstalledOrHidden;

            return InstalledOrHidden + " or " + OptionalInstalledOrHidden;
        }
    }
}
