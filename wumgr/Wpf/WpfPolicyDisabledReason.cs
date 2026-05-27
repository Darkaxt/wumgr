namespace wumgr
{
    internal static class WpfPolicyDisabledReason
    {
        public static string Get(bool isAdministrator, bool isRunningAsUwp, GPO.Respect gpoRespect)
        {
            if (isRunningAsUwp)
                return Translate.fmt("wpf_policy_disabled_uwp");

            if (!isAdministrator)
                return Translate.fmt("wpf_policy_disabled_admin");

            if (gpoRespect == GPO.Respect.None)
                return Translate.fmt("wpf_policy_disabled_unsupported");

            if (gpoRespect == GPO.Respect.Partial)
                return Translate.fmt("wpf_policy_disabled_partial");

            return "";
        }
    }
}
