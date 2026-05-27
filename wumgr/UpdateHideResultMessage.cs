using System;
using System.Runtime.InteropServices;

namespace wumgr
{
    static class UpdateHideResultMessage
    {
        public static string Failure(MsUpdate update, bool hide, Exception error)
        {
            string action = hide ? "hide" : "unhide";
            string title = GetUpdateLabel(update);
            string detail = GetErrorDetail(error);

            return string.Format("Failed to {0} update \"{1}\": {2}", action, title, detail);
        }

        private static string GetUpdateLabel(MsUpdate update)
        {
            if (update == null)
                return "unknown update";

            if (!string.IsNullOrWhiteSpace(update.Title))
                return update.Title;

            if (!string.IsNullOrWhiteSpace(update.KB))
                return update.KB;

            if (!string.IsNullOrWhiteSpace(update.UUID))
                return update.UUID;

            return "unknown update";
        }

        private static string GetErrorDetail(Exception error)
        {
            if (error == null)
                return "Unknown Error";

            COMException comError = error as COMException;
            if (comError != null)
            {
                uint code = unchecked((uint)comError.ErrorCode);
                return string.Format("Error 0x{0}: {1}", code.ToString("X").PadLeft(8, '0'), UpdateErrors.GetErrorStr(code));
            }

            if (!string.IsNullOrWhiteSpace(error.Message))
                return error.Message;

            return error.GetType().Name;
        }
    }
}
