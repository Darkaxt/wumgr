using System;

namespace wumgr
{
    internal enum AutoUpdateMode
    {
        No = 0,
        EveryDay,
        EveryWeek,
        EveryMonth
    }

    internal static class AutoUpdateSchedule
    {
        public static int GetDueDays(AutoUpdateMode mode, DateTime lastCheck, DateTime now)
        {
            if (mode == AutoUpdateMode.No)
                return 0;

            DateTime nextUpdate;
            switch (mode)
            {
                case AutoUpdateMode.EveryDay:
                    nextUpdate = lastCheck.AddDays(1);
                    break;
                case AutoUpdateMode.EveryWeek:
                    nextUpdate = lastCheck.AddDays(7);
                    break;
                case AutoUpdateMode.EveryMonth:
                    nextUpdate = lastCheck.AddMonths(1);
                    break;
                default:
                    return 0;
            }

            if (nextUpdate >= now)
                return 0;

            return (int)Math.Ceiling((now - nextUpdate).TotalDays);
        }

        public static int GetGraceDays(AutoUpdateMode mode)
        {
            return mode == AutoUpdateMode.EveryMonth ? 15 : 3;
        }
    }
}
