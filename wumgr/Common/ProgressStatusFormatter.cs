using System.Collections.Generic;

internal static class ProgressStatusFormatter
{
    public static string Format(string operation, wumgr.WuAgent.ProgressArgs args)
    {
        if (args == null)
            return operation ?? "";

        if (args.TotalCount == -1)
            return (operation ?? "") + "...";

        List<string> parts = new List<string>();
        parts.Add(operation ?? "");

        if (args.TotalCount > 1)
            parts.Add(args.CurrentIndex + "/" + args.TotalCount);

        if (args.CurrentPercent > 0 && args.CurrentPercent <= 100)
            parts.Add(args.CurrentPercent + "%");

        string speed = FormatSpeed(args.BytesPerSecond);
        if (speed.Length != 0)
            parts.Add(speed);

        return string.Join(" ", parts.ToArray()).Trim();
    }

    public static string FormatSpeed(long bytesPerSecond)
    {
        if (bytesPerSecond <= 0)
            return "";

        return FileOps.FormatSize(bytesPerSecond) + "/s";
    }
}
