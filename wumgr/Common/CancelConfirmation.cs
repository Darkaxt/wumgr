using System.Windows.Forms;

internal static class CancelConfirmation
{
    public static string Title { get { return wumgr.Translate.fmt("cap_cancel_op"); } }
    public static string Message { get { return wumgr.Translate.fmt("msg_cancel_op"); } }

    public static bool IsConfirmed(DialogResult result)
    {
        return result == DialogResult.Yes;
    }

}
