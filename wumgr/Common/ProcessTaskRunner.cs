using System;
using System.Diagnostics;
using System.Text;

namespace wumgr
{
    class ProcessTaskResult
    {
        public const int CanceledExitCode = -1;

        public int ExitCode { get; private set; }
        public string StandardOutput { get; private set; }
        public string StandardError { get; private set; }
        public bool Canceled { get; private set; }

        public ProcessTaskResult(int exitCode, string standardOutput, string standardError, bool canceled)
        {
            ExitCode = exitCode;
            StandardOutput = standardOutput ?? "";
            StandardError = standardError ?? "";
            Canceled = canceled;
        }
    }

    class ProcessTaskRunner
    {
        public static string FormatCommandLine(ProcessStartInfo startInfo)
        {
            if (startInfo == null)
                return "";

            string command = Quote(startInfo.FileName);

            if (string.IsNullOrWhiteSpace(startInfo.Arguments))
                return command;

            return command + " " + startInfo.Arguments;
        }

        public static ProcessTaskResult Run(ProcessStartInfo startInfo, Func<bool> isCanceled, bool silent)
        {
            startInfo.FileName = Environment.ExpandEnvironmentVariables(startInfo.FileName);

            StringBuilder standardOutput = new StringBuilder();
            StringBuilder standardError = new StringBuilder();

            if (silent)
            {
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
            }

            using (Process proc = new Process())
            {
                proc.StartInfo = startInfo;
                proc.EnableRaisingEvents = true;

                if (silent)
                {
                    proc.OutputDataReceived += (sender, args) => AppendLine(standardOutput, args.Data);
                    proc.ErrorDataReceived += (sender, args) => AppendLine(standardError, args.Data);
                }

                proc.Start();

                if (silent)
                {
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                }

                bool canceled = false;
                while (!proc.WaitForExit(250))
                {
                    if (isCanceled == null || !isCanceled())
                        continue;

                    canceled = true;
                    try
                    {
                        if (!proc.HasExited)
                            proc.Kill();
                    }
                    catch
                    {
                    }
                    break;
                }

                proc.WaitForExit();

                return new ProcessTaskResult(
                    canceled ? ProcessTaskResult.CanceledExitCode : proc.ExitCode,
                    standardOutput.ToString(),
                    standardError.ToString(),
                    canceled);
            }
        }

        private static void AppendLine(StringBuilder builder, string line)
        {
            if (line == null)
                return;

            lock (builder)
            {
                builder.AppendLine(line);
            }
        }

        private static string Quote(string value)
        {
            return "\"" + (value ?? "").Replace("\"", "\\\"") + "\"";
        }
    }
}
