namespace wumgr
{
    class ManualInstallExitCode
    {
        public bool Success { get; private set; }
        public bool RebootRequired { get; private set; }

        private ManualInstallExitCode(bool success, bool rebootRequired)
        {
            Success = success;
            RebootRequired = rebootRequired;
        }

        public static ManualInstallExitCode FromProcessExitCode(int exitCode)
        {
            if (exitCode == 0)
                return new ManualInstallExitCode(true, false);

            if (exitCode == 3010 || exitCode == 1641)
                return new ManualInstallExitCode(true, true);

            return new ManualInstallExitCode(false, false);
        }
    }
}
