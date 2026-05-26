using System;
using System.Collections.Generic;

namespace wumgr.Tests
{
    internal static class Program
    {
        private static int failures;

        private static int Main()
        {
            Run("Create skips updates without download URLs", CreateSkipsUpdatesWithoutDownloadUrls);
            Run("Create treats empty download URLs as missing", CreateTreatsEmptyDownloadUrlsAsMissing);
            Run("GetCompletedFiles skips failed and incomplete downloads", GetCompletedFilesSkipsFailedAndIncompleteDownloads);

            if (failures != 0)
                Console.Error.WriteLine("{0} test(s) failed.", failures);

            return failures == 0 ? 0 : 1;
        }

        private static void Run(string name, Action test)
        {
            try
            {
                test();
                Console.WriteLine("PASS {0}", name);
            }
            catch (Exception ex)
            {
                failures++;
                Console.Error.WriteLine("FAIL {0}: {1}", name, ex.Message);
            }
        }

        private static void CreateSkipsUpdatesWithoutDownloadUrls()
        {
            var missing = new MsUpdate { KB = "KB1", Title = "Missing package" };
            var downloadable = new MsUpdate { KB = "KB2", Title = "Downloadable package" };
            downloadable.Downloads.Add("https://download.example.test/kb2.msu");

            ManualDownloadPlan plan = ManualDownloadPlanner.Create(new List<MsUpdate> { missing, downloadable }, @"C:\Updates");

            AssertEqual(1, plan.MissingUpdates.Count, "missing update count");
            AssertEqual(1, plan.Updates.Count, "eligible update count");
            AssertEqual(1, plan.Downloads.Count, "download task count");
            AssertEqual(@"C:\Updates\KB2", plan.Downloads[0].Path, "download path");
            Assert(plan.HasSkippedUpdates, "plan should record skipped updates");
        }

        private static void CreateTreatsEmptyDownloadUrlsAsMissing()
        {
            var update = new MsUpdate { KB = "KB3", Title = "Empty URL package" };
            update.Downloads.Add("");
            update.Downloads.Add("   ");

            ManualDownloadPlan plan = ManualDownloadPlanner.Create(new List<MsUpdate> { update }, @"C:\Updates");

            AssertEqual(1, plan.MissingUpdates.Count, "missing update count");
            AssertEqual(0, plan.Updates.Count, "eligible update count");
            AssertEqual(0, plan.Downloads.Count, "download task count");
            Assert(plan.HasSkippedUpdates, "plan should record skipped updates");
        }

        private static void GetCompletedFilesSkipsFailedAndIncompleteDownloads()
        {
            var downloads = new List<UpdateDownloader.Task>
            {
                new UpdateDownloader.Task { KB = "KB4", Path = @"C:\Updates\KB4", FileName = "good.msu" },
                new UpdateDownloader.Task { KB = "KB5", Path = @"C:\Updates\KB5", FileName = "failed.msu", Failed = true },
                new UpdateDownloader.Task { KB = "KB6", Path = @"C:\Updates\KB6" }
            };

            MultiValueDictionary<string, string> files = ManualDownloadPlanner.GetCompletedFiles(downloads);

            AssertEqual(1, files.GetCount(), "completed file count");
            AssertEqual(@"C:\Updates\KB4\good.msu", files.GetValues("KB4")[0], "completed file path");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void AssertEqual<T>(T expected, T actual, string message)
        {
            if (!object.Equals(expected, actual))
                throw new InvalidOperationException(string.Format("{0}: expected <{1}> but was <{2}>", message, expected, actual));
        }
    }
}
