using System;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

internal static class PipeSecurityFactory
{
    public static PipeSecurity CreateCurrentUserSecurity()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        if (identity == null || identity.User == null)
            throw new InvalidOperationException("Unable to determine the current Windows user for IPC security.");

        PipeSecurity security = new PipeSecurity();
        security.AddAccessRule(new PipeAccessRule(identity.User, PipeAccessRights.FullControl, AccessControlType.Allow));
        security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
        security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
        return security;
    }
}
