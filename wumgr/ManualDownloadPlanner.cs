using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace wumgr
{
    class ManualDownloadPlan
    {
        public List<UpdateDownloader.Task> Downloads { get; private set; }
        public List<MsUpdate> Updates { get; private set; }
        public List<MsUpdate> MissingUpdates { get; private set; }

        public bool HasSkippedUpdates
        {
            get { return MissingUpdates.Count != 0; }
        }

        public ManualDownloadPlan()
        {
            Downloads = new List<UpdateDownloader.Task>();
            Updates = new List<MsUpdate>();
            MissingUpdates = new List<MsUpdate>();
        }
    }

    static class ManualDownloadPlanner
    {
        public static ManualDownloadPlan Create(IEnumerable<MsUpdate> updates, string downloadRoot)
        {
            ManualDownloadPlan plan = new ManualDownloadPlan();

            foreach (MsUpdate update in updates)
            {
                List<string> urls = update.Downloads.Cast<string>()
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .ToList();

                if (urls.Count == 0)
                {
                    plan.MissingUpdates.Add(update);
                    continue;
                }

                plan.Updates.Add(update);

                foreach (string url in urls)
                {
                    plan.Downloads.Add(new UpdateDownloader.Task
                    {
                        Url = url,
                        Path = Path.Combine(downloadRoot, update.KB),
                        KB = update.KB
                    });
                }
            }

            return plan;
        }

        public static MultiValueDictionary<string, string> GetCompletedFiles(IEnumerable<UpdateDownloader.Task> downloads)
        {
            MultiValueDictionary<string, string> allFiles = new MultiValueDictionary<string, string>();

            foreach (UpdateDownloader.Task task in downloads)
            {
                if (task.Failed || string.IsNullOrEmpty(task.FileName))
                    continue;

                allFiles.Add(task.KB, Path.Combine(task.Path, task.FileName));
            }

            return allFiles;
        }
    }
}
