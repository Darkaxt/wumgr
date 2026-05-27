using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using wumgr.Wpf;

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
            Run("Select all reports selection changes", SelectAllReportsSelectionChanges);
            Run("Result dialog guard suppresses duplicate dialogs", ResultDialogGuardSuppressesDuplicateDialogs);
            Run("Pipe security avoids World access", PipeSecurityAvoidsWorldAccess);
            Run("Download file names are sanitized", DownloadFileNamesAreSanitized);
            Run("Content-Disposition filename parsing is guarded", ContentDispositionFilenameParsingIsGuarded);
            Run("Startup elevation only runs when explicitly configured", StartupElevationOnlyRunsWhenConfigured);
            Run("Startup UI defaults to WPF with WinForms fallback", StartupUiDefaultsToWpfWithWinFormsFallback);
            Run("Startup defers agent init for WPF shell", StartupDefersAgentInitForWpfShell);
            Run("WPF action state mirrors admin and list rules", WpfActionStateMirrorsAdminAndListRules);
            Run("WPF window placement rejects missing and tiny persisted bounds", WpfWindowPlacementRejectsInvalidBounds);
            Run("Auto update schedule reports due days", AutoUpdateScheduleReportsDueDays);
            Run("WPF policy options disable writes without elevation", WpfPolicyOptionsDisableWritesWithoutElevation);
            Run("WPF policy options mirror GPO respect rules", WpfPolicyOptionsMirrorGpoRespectRules);
            Run("WPF progress value clamps percentages", WpfProgressValueClampsPercentages);
            Run("WPF progress fill width mirrors progress state", WpfProgressFillWidthMirrorsProgressState);
            Run("WPF action toolbar uses original icon resources", WpfActionToolbarUsesOriginalIconResources);
            Run("WPF list selection policy hides history selectors", WpfListSelectionPolicyHidesHistorySelectors);
            Run("WPF status text avoids implementation labels", WpfStatusTextAvoidsImplementationLabels);
            Run("WPF localized text mirrors shared translations", WpfLocalizedTextMirrorsSharedTranslations);

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

        private static void SelectAllReportsSelectionChanges()
        {
            using (var list = new ListView())
            {
                list.Items.Add(new ListViewItem("one"));
                list.Items.Add(new ListViewItem("two"));

                bool changed = UpdateSelectionHelper.SetAllChecked(list.Items, true);

                Assert(changed, "selection helper should report that items changed");
                Assert(list.Items[0].Checked, "first item should be checked");
                Assert(list.Items[1].Checked, "second item should be checked");
            }
        }

        private static void ResultDialogGuardSuppressesDuplicateDialogs()
        {
            var guard = new ResultDialogGuard();

            Assert(guard.TryBegin(), "first dialog should be allowed");
            Assert(!guard.TryBegin(), "duplicate dialog should be suppressed");

            guard.End();

            Assert(guard.TryBegin(), "dialog should be allowed after previous dialog ends");
            guard.End();
        }

        private static void PipeSecurityAvoidsWorldAccess()
        {
            PipeSecurity security = PipeSecurityFactory.CreateCurrentUserSecurity();
            AuthorizationRuleCollection rules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

            SecurityIdentifier worldSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            SecurityIdentifier currentUserSid = WindowsIdentity.GetCurrent().User;

            bool grantsWorld = rules
                .Cast<PipeAccessRule>()
                .Any(rule => rule.IdentityReference.Value == worldSid.Value && rule.AccessControlType == AccessControlType.Allow);

            bool grantsCurrentUser = rules
                .Cast<PipeAccessRule>()
                .Any(rule => rule.IdentityReference.Value == currentUserSid.Value
                    && rule.AccessControlType == AccessControlType.Allow
                    && (rule.PipeAccessRights & PipeAccessRights.FullControl) == PipeAccessRights.FullControl);

            Assert(!grantsWorld, "pipe security must not grant World access");
            Assert(grantsCurrentUser, "pipe security should grant the current user full control");
        }

        private static void DownloadFileNamesAreSanitized()
        {
            AssertEqual("evil.msu", DownloadFileNameHelper.Sanitize(@"..\evil.msu"), "parent path stripped");
            AssertEqual("file.msu", DownloadFileNameHelper.Sanitize("folder/sub/file.msu"), "forward slash path stripped");
            AssertEqual("bad_name_.msu", DownloadFileNameHelper.Sanitize("bad:name?.msu"), "invalid characters replaced");
            AssertEqual("", DownloadFileNameHelper.Sanitize(".."), "directory-only name rejected");
        }

        private static void ContentDispositionFilenameParsingIsGuarded()
        {
            AssertEqual("package.msu", DownloadFileNameHelper.GetContentDispositionFileName("attachment; filename=\"package.msu\""), "quoted filename");
            AssertEqual("safe.msu", DownloadFileNameHelper.GetContentDispositionFileName(@"attachment; filename=""..\safe.msu"""), "path filename sanitized");
            AssertEqual(null, DownloadFileNameHelper.GetContentDispositionFileName("attachment; name=\"package.msu\""), "missing filename ignored");
            AssertEqual(null, DownloadFileNameHelper.GetContentDispositionFileName("attachment; filename=\"..\""), "directory filename ignored");
        }

        private static void StartupElevationOnlyRunsWhenConfigured()
        {
            Assert(!StartupElevationPolicy.ShouldAttemptStartupElevation(false, false, false), "non-admin startup without Skip UAC should not auto-elevate");
            Assert(StartupElevationPolicy.ShouldAttemptStartupElevation(false, false, true), "non-admin startup with Skip UAC should attempt configured elevation");
            Assert(!StartupElevationPolicy.ShouldAttemptStartupElevation(true, false, true), "already-admin startup should not re-elevate");
            Assert(!StartupElevationPolicy.ShouldAttemptStartupElevation(false, true, true), "debug startup should not auto-elevate");
        }

        private static void StartupUiDefaultsToWpfWithWinFormsFallback()
        {
            AssertEqual(StartupUiKind.Wpf, StartupUiMode.Select(new string[0]), "default UI mode");
            AssertEqual(StartupUiKind.Wpf, StartupUiMode.Select(new[] { "-wpf" }), "explicit WPF UI mode");
            AssertEqual(StartupUiKind.WinForms, StartupUiMode.Select(new[] { "-winforms" }), "explicit WinForms fallback");
            AssertEqual(StartupUiKind.WinForms, StartupUiMode.Select(new[] { "/winforms" }), "slash WinForms fallback");
            AssertEqual(StartupUiKind.WinForms, StartupUiMode.Select(new[] { "-wpf", "-winforms" }), "fallback should override WPF when both are present");
            Assert(!StartupUiMode.ShouldStartInTray(new string[0]), "normal launch should show a window");
            Assert(StartupUiMode.ShouldStartInTray(new[] { "-tray" }), "dash tray launch should start hidden");
            Assert(StartupUiMode.ShouldStartInTray(new[] { "/tray" }), "slash tray launch should start hidden");
        }

        private static void StartupDefersAgentInitForWpfShell()
        {
            Assert(!StartupUiMode.ShouldInitializeAgentBeforeWindow(StartupUiKind.Wpf), "WPF should show the window before slow WUA initialization");
            Assert(StartupUiMode.ShouldInitializeAgentBeforeWindow(StartupUiKind.WinForms), "WinForms should preserve the legacy initialization order");
        }

        private static void WpfActionStateMirrorsAdminAndListRules()
        {
            WpfActionState pendingNonAdmin = WpfActionState.Create(true, false, true, false, true, false, WpfUpdateListKind.Pending);
            Assert(pendingNonAdmin.CanSearch, "active non-busy agent can search");
            Assert(pendingNonAdmin.CanDownload, "selected pending updates can be downloaded");
            Assert(!pendingNonAdmin.CanInstall, "non-admin user cannot install");
            Assert(pendingNonAdmin.CanHide, "selected pending updates can be hidden");

            WpfActionState pendingAdmin = WpfActionState.Create(true, true, true, false, true, false, WpfUpdateListKind.Pending);
            Assert(pendingAdmin.CanInstall, "admin user can install selected pending updates");

            WpfActionState installedAdmin = WpfActionState.Create(true, true, true, false, true, false, WpfUpdateListKind.Installed);
            Assert(installedAdmin.CanUninstall, "admin user can uninstall selected installed updates");
            Assert(!installedAdmin.CanDownload, "installed updates cannot be downloaded");

            WpfActionState hidden = WpfActionState.Create(true, false, true, false, true, false, WpfUpdateListKind.Hidden);
            Assert(hidden.CanHide, "selected hidden updates can be unhidden");

            WpfActionState busy = WpfActionState.Create(true, true, true, true, true, true, WpfUpdateListKind.Pending);
            Assert(!busy.CanSearch, "busy agent cannot search");
            Assert(!busy.CanDownload, "busy agent cannot download");
            Assert(busy.CanCancel, "busy agent can be cancelled");
        }

        private static void WpfWindowPlacementRejectsInvalidBounds()
        {
            WpfWindowPlacement placement;

            Assert(!WpfWindowPlacement.TryCreate("", "10", "1200", "800", "Normal", 900, 600, out placement), "missing left should be rejected");
            Assert(!WpfWindowPlacement.TryCreate("10", "10", "200", "800", "Normal", 900, 600, out placement), "too-small width should be rejected");
            Assert(WpfWindowPlacement.TryCreate("10", "20", "1200", "800", "Maximized", 900, 600, out placement), "valid bounds should be accepted");
            AssertEqual(10.0, placement.Left, "left");
            AssertEqual(20.0, placement.Top, "top");
            AssertEqual(1200.0, placement.Width, "width");
            AssertEqual(800.0, placement.Height, "height");
            Assert(placement.Maximized, "maximized state should be retained");
        }

        private static void AutoUpdateScheduleReportsDueDays()
        {
            DateTime now = new DateTime(2026, 5, 27, 12, 0, 0);

            AssertEqual(0, AutoUpdateSchedule.GetDueDays(AutoUpdateMode.No, now.AddYears(-1), now), "disabled mode");
            AssertEqual(0, AutoUpdateSchedule.GetDueDays(AutoUpdateMode.EveryDay, now.AddHours(-12), now), "not due daily");
            AssertEqual(1, AutoUpdateSchedule.GetDueDays(AutoUpdateMode.EveryDay, now.AddDays(-2), now), "daily overdue one day");
            AssertEqual(1, AutoUpdateSchedule.GetDueDays(AutoUpdateMode.EveryWeek, now.AddDays(-8), now), "weekly overdue one day");
            AssertEqual(2, AutoUpdateSchedule.GetDueDays(AutoUpdateMode.EveryMonth, now.AddMonths(-1).AddDays(-2), now), "monthly overdue two days");
            AssertEqual(3, AutoUpdateSchedule.GetGraceDays(AutoUpdateMode.EveryDay), "daily grace");
            AssertEqual(15, AutoUpdateSchedule.GetGraceDays(AutoUpdateMode.EveryMonth), "monthly grace");
        }

        private static void WpfPolicyOptionsDisableWritesWithoutElevation()
        {
            WpfPolicyOptionState state = WpfPolicyOptionState.Create(false, GPO.Respect.Full, 10.0f, GPO.AUOptions.Scheduled, true, false);

            Assert(!state.CanChangeBlockMicrosoft, "non-admin cannot change WU server block");
            Assert(!state.CanSelectNotification, "non-admin cannot select notification policy");
            Assert(!state.CanSelectDownload, "non-admin cannot select download policy");
            Assert(!state.CanSelectScheduled, "non-admin cannot select scheduled policy");
            Assert(!state.CanChangeSchedule, "non-admin cannot change scheduled day/time");
            Assert(!state.CanChangeDisableFacilitators, "non-admin cannot change update facilitators");
            Assert(!state.CanChangeHideWindowsUpdatePage, "non-admin cannot hide Settings page");
            Assert(!state.CanChangeStoreAutoUpdate, "non-admin cannot change Store auto-update policy");
            Assert(!state.CanChangeDrivers, "non-admin cannot change driver policy");
        }

        private static void WpfPolicyOptionsMirrorGpoRespectRules()
        {
            WpfPolicyOptionState partialWithoutBlock = WpfPolicyOptionState.Create(true, GPO.Respect.Partial, 10.0f, GPO.AUOptions.Disabled, false, false);
            Assert(!partialWithoutBlock.CanSelectNotification, "partial GPO support disables notification policy");
            Assert(!partialWithoutBlock.CanSelectDownload, "partial GPO support disables download policy");
            Assert(!partialWithoutBlock.CanSelectScheduled, "partial GPO support disables scheduled policy");
            Assert(partialWithoutBlock.DisableFacilitatorsForcedOn, "partial support without WU block forces facilitator disable");
            Assert(!partialWithoutBlock.CanChangeDisableFacilitators, "forced facilitator disable is read-only");
            Assert(partialWithoutBlock.HideWindowsUpdatePageForcedOn, "disabled facilitators force the Settings page hidden");

            WpfPolicyOptionState partialWithBlock = WpfPolicyOptionState.Create(true, GPO.Respect.Partial, 10.0f, GPO.AUOptions.Disabled, true, false);
            Assert(partialWithBlock.CanChangeDisableFacilitators, "partial support with WU block can change facilitator disable");

            WpfPolicyOptionState fullScheduled = WpfPolicyOptionState.Create(true, GPO.Respect.Full, 10.0f, GPO.AUOptions.Scheduled, false, false);
            Assert(fullScheduled.CanSelectScheduled, "full GPO support can select scheduled policy");
            Assert(fullScheduled.CanChangeSchedule, "scheduled policy enables day/time controls");
            Assert(!fullScheduled.CanChangeDisableFacilitators, "non-disabled AU policy disables facilitator control");

            WpfPolicyOptionState windows7 = WpfPolicyOptionState.Create(true, GPO.Respect.Full, 6.1f, GPO.AUOptions.Default, false, false);
            Assert(!windows7.CanChangeHideWindowsUpdatePage, "pre-Windows 10 cannot hide WU Settings page");
            Assert(!windows7.CanChangeStoreAutoUpdate, "pre-Windows 8 cannot change Store auto-update policy");
        }

        private static void WpfLocalizedTextMirrorsSharedTranslations()
        {
            wumgr.Program.appPath = Path.Combine(Directory.GetCurrentDirectory(), "wumgr");
            Translate.Load("en");

            var text = new WpfLocalizedText();

            AssertEqual(Translate.fmt("lbl_opt"), text.OptionsTab, "options tab");
            AssertEqual(Translate.fmt("lbl_au"), text.AutoUpdateTab, "auto update tab");
            AssertEqual(Translate.fmt("lbl_auto"), text.RunInBackground, "run in background");
            AssertEqual(Translate.fmt("lbl_ac_week"), text.AutoUpdateOptions[2], "auto update weekly option");
            AssertEqual(Translate.fmt("lbl_block_ms"), text.BlockMicrosoftServers, "block Microsoft servers");
            AssertEqual(Translate.fmt("tip_inst"), text.InstallButton, "install button");
            AssertEqual(Translate.fmt("col_kb"), text.KbColumn, "KB column");
            AssertEqual("Refresh", text.RefreshButton, "button text should strip WinForms mnemonic markers");
            AssertEqual("Exit", text.ExitMenu, "WPF tray menu text should strip WinForms mnemonic markers");
            AssertEqual("Initializing Windows Update Agent...", text.InitializingAgent, "WPF startup status text");
            AssertEqual("Wednesday", text.ScheduleDays[4], "WPF schedule day text");
        }

        private static void WpfProgressValueClampsPercentages()
        {
            AssertEqual(0.0, WpfProgressValue.NormalizePercent(-25), "negative percent");
            AssertEqual(0.0, WpfProgressValue.NormalizePercent(0), "zero percent");
            AssertEqual(0.42, WpfProgressValue.NormalizePercent(42), "mid percent");
            AssertEqual(1.0, WpfProgressValue.NormalizePercent(100), "full percent");
            AssertEqual(1.0, WpfProgressValue.NormalizePercent(150), "overfull percent");
        }

        private static void WpfProgressFillWidthMirrorsProgressState()
        {
            AssertEqual(0.0, WpfProgressValue.GetFillWidth(200.0, 0, false), "empty determinate progress");
            AssertEqual(84.0, WpfProgressValue.GetFillWidth(200.0, 42, false), "partial determinate progress");
            AssertEqual(200.0, WpfProgressValue.GetFillWidth(200.0, 100, false), "complete determinate progress");
            AssertEqual(200.0, WpfProgressValue.GetFillWidth(200.0, 0, true), "indeterminate progress");
            AssertEqual(0.0, WpfProgressValue.GetFillWidth(0.0, 50, false), "zero-width track");
        }

        private static void WpfActionToolbarUsesOriginalIconResources()
        {
            WpfActionButtonSpec[] specs = WpfActionButtonSpec.CreateDefault();

            AssertEqual(8, specs.Length, "toolbar action count");
            AssertEqual(WpfActionButtonKind.Refresh, specs[0].Kind, "refresh action order");
            AssertEqual("icons8_refresh_32", specs[0].ResourceName, "refresh icon");
            AssertEqual(WpfActionButtonKind.Search, specs[1].Kind, "search action order");
            AssertEqual("icons8_available_updates_32", specs[1].ResourceName, "search icon");
            AssertEqual("icons8_downloading_updates_32", specs[2].ResourceName, "download icon");
            AssertEqual("icons8_software_installer_32", specs[3].ResourceName, "install icon");
            AssertEqual("icons8_trash_32", specs[4].ResourceName, "uninstall icon");
            AssertEqual("icons8_hide_32", specs[5].ResourceName, "hide icon");
            AssertEqual("icons8_link_32", specs[6].ResourceName, "link icon");
            AssertEqual("icons8_cancel_32", specs[7].ResourceName, "cancel icon");
        }

        private static void WpfListSelectionPolicyHidesHistorySelectors()
        {
            Assert(WpfListSelectionPolicy.CanSelectRows(WpfUpdateListKind.Pending), "pending updates should be selectable");
            Assert(WpfListSelectionPolicy.CanSelectRows(WpfUpdateListKind.Installed), "installed updates should be selectable");
            Assert(WpfListSelectionPolicy.CanSelectRows(WpfUpdateListKind.Hidden), "hidden updates should be selectable");
            Assert(!WpfListSelectionPolicy.CanSelectRows(WpfUpdateListKind.History), "history rows should not expose purposeless selection checkboxes");
        }

        private static void WpfStatusTextAvoidsImplementationLabels()
        {
            AssertEqual("Ready.", WpfStatusText.Ready, "startup status");
            AssertEqual("Current list refreshed from the update cache.", WpfStatusText.CurrentListRefreshed, "refresh status");
            Assert(!WpfStatusText.Ready.Contains("WPF"), "ready text should not mention implementation framework");
            Assert(!WpfStatusText.CurrentListRefreshed.Contains("WPF"), "refresh text should not mention implementation framework");
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
