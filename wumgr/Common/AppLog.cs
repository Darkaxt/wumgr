using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

class AppLog
{
    private List<string> mLogList = new List<string>();
    private Dispatcher mDispatcher;

    static public void Line(string str, params object[] args)
    {
        Line(string.Format(str, args));
    }

    static public void Line(String line)
    {
        if (mInstance != null)
            mInstance.logLine(line);
    }

    public static string FormatLine(string line, DateTime timestamp)
    {
        return string.Format(System.Globalization.CultureInfo.InvariantCulture, "[{0:HH:mm:ss}] {1}", timestamp, line ?? "");
    }

    public void logLine(String line)
    {
        mDispatcher.BeginInvoke(new Action(() => {
            string formattedLine = FormatLine(line, DateTime.Now);
            mLogList.Add(formattedLine);
            while (mLogList.Count > 100)
                mLogList.RemoveAt(0);

            Logger?.Invoke(this, new LogEventArgs(formattedLine));
        }));
    }

    static public List<string> GetLog() { return mInstance.mLogList; }

    public class LogEventArgs : EventArgs
    {
        public string line { get; set; }
        public LogEventArgs(string _line) { line = _line; }
    }

    static public event EventHandler<LogEventArgs> Logger;

    static void LineLogger(object sender, LogEventArgs args)
    {
        Console.WriteLine("LOG: " + args.line);
    }

    static private AppLog mInstance = null;

    public static AppLog GetInstance() { return mInstance; }

    public AppLog()
    {
        mInstance = this;

        mDispatcher = Dispatcher.CurrentDispatcher;

        Logger += LineLogger;
    }
}
