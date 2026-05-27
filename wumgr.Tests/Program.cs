using System;
using System.Collections.Generic;
using System.Globalization;
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
        private static bool englishTranslationsLoaded;

        private static int Main()
        {
            Run("Create skips updates without download URLs", CreateSkipsUpdatesWithoutDownloadUrls);
            Run("Create treats empty download URLs as missing", CreateTreatsEmptyDownloadUrlsAsMissing);
            Run("Create uses package title keys for KBUnknown downloads", CreateUsesPackageTitleKeysForKbUnknownDownloads);
            Run("GetCompletedFiles skips failed and incomplete downloads", GetCompletedFilesSkipsFailedAndIncompleteDownloads);
            Run("Select all reports selection changes", SelectAllReportsSelectionChanges);
            Run("Result dialog guard suppresses duplicate dialogs", ResultDialogGuardSuppressesDuplicateDialogs);
            Run("WPF result messages report successful reboot requirements", WpfResultMessagesReportSuccessfulRebootRequirements);
            Run("Pipe security avoids World access", PipeSecurityAvoidsWorldAccess);
            Run("Download file names are sanitized", DownloadFileNamesAreSanitized);
            Run("Content-Disposition filename parsing is guarded", ContentDispositionFilenameParsingIsGuarded);
            Run("Command-line argument lookup guards missing values", CommandLineArgumentLookupGuardsMissingValues);
            Run("Manual installer exit codes report failures", ManualInstallerExitCodesReportFailures);
            Run("Manual installer arguments support SCEP packages", ManualInstallerArgumentsSupportScepPackages);
            Run("WinINet unrecognized scheme error is named", WinInetUnrecognizedSchemeErrorIsNamed);
            Run("Startup elevation only runs when explicitly configured", StartupElevationOnlyRunsWhenConfigured);
            Run("Startup UI defaults to WPF with WinForms fallback", StartupUiDefaultsToWpfWithWinFormsFallback);
            Run("Startup defers agent init for WPF shell", StartupDefersAgentInitForWpfShell);
            Run("Update selection state detects uninstallable selections", UpdateSelectionStateDetectsUninstallableSelections);
            Run("WPF action state mirrors admin and list rules", WpfActionStateMirrorsAdminAndListRules);
            Run("WPF window placement rejects missing and tiny persisted bounds", WpfWindowPlacementRejectsInvalidBounds);
            Run("WPF status pane height rejects invalid persisted values", WpfStatusPaneHeightRejectsInvalidPersistedValues);
            Run("Auto update schedule reports due days", AutoUpdateScheduleReportsDueDays);
            Run("Update dates display with local culture", UpdateDatesDisplayWithLocalCulture);
            Run("Update cache dates round-trip invariantly", UpdateCacheDatesRoundTripInvariantly);
            Run("Update cache dates read legacy localized values", UpdateCacheDatesReadLegacyLocalizedValues);
            Run("App log formats lines with timestamps", AppLogFormatsLinesWithTimestamps);
            Run("Cancel confirmation accepts only affirmative result", CancelConfirmationAcceptsOnlyAffirmativeResult);
            Run("Settings page visibility preserves unrelated hidden pages", SettingsPageVisibilityPreservesUnrelatedHiddenPages);
            Run("WPF policy options disable writes without elevation", WpfPolicyOptionsDisableWritesWithoutElevation);
            Run("WPF policy options mirror GPO respect rules", WpfPolicyOptionsMirrorGpoRespectRules);
            Run("WPF policy disabled reason explains gated controls", WpfPolicyDisabledReasonExplainsGatedControls);
            Run("WPF progress value clamps percentages", WpfProgressValueClampsPercentages);
            Run("WPF progress fill width mirrors progress state", WpfProgressFillWidthMirrorsProgressState);
            Run("WPF progress visual helpers keep custom bar readable", WpfProgressVisualHelpersKeepCustomBarReadable);
            Run("WPF progress visibility follows active work", WpfProgressVisibilityFollowsActiveWork);
            Run("Progress status includes current percent and speed", ProgressStatusIncludesCurrentPercentAndSpeed);
            Run("WPF action toolbar uses modern glyph icons", WpfActionToolbarUsesModernGlyphIcons);
            Run("WPF theme settings persist and resolve modes", WpfThemeSettingsPersistAndResolveModes);
            Run("WPF caption glyphs reflect window state", WpfCaptionGlyphsReflectWindowState);
            Run("WPF list selection policy hides history selectors", WpfListSelectionPolicyHidesHistorySelectors);
            Run("WPF update filter matches visible columns", WpfUpdateFilterMatchesVisibleColumns);
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

        private static void CreateUsesPackageTitleKeysForKbUnknownDownloads()
        {
            var driver = new MsUpdate { KB = "KBUnknown", Title = "Lenovo Driver Update (15.11.30.11)" };
            driver.Downloads.Add("https://download.example.test/driver.cab");

            ManualDownloadPlan plan = ManualDownloadPlanner.Create(new List<MsUpdate> { driver }, @"C:\Updates");

            AssertEqual(1, plan.Downloads.Count, "download task count");
            AssertEqual("Lenovo Driver Update (15.11.30.11)", plan.Downloads[0].KB, "download key");
            AssertEqual(@"C:\Updates\Lenovo Driver Update (15.11.30.11)", plan.Downloads[0].Path, "download path");
            AssertEqual("Lenovo Driver Update (15.11.30.11)", ManualDownloadPlanner.GetUpdateKey(driver), "installer key");
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

            int shown = 0;
            Assert(guard.TryRun(() =>
            {
                shown++;
                Assert(!guard.TryRun(() => shown++), "nested dialog should be suppressed");
            }), "first guarded callback should run");
            AssertEqual(1, shown, "suppressed callback should not run");
            Assert(guard.TryRun(() => shown++), "guard should reset after callback");
            AssertEqual(2, shown, "second callback should run after reset");
        }

        private static void WpfResultMessagesReportSuccessfulRebootRequirements()
        {
            LoadEnglishTranslations();

            WpfOperationResultMessage successReboot = WpfOperationResultMessage.Create(WuAgent.AgentOperation.InstallingUpdates, WuAgent.RetCodes.Success, true, "Installing Updates", "");
            Assert(successReboot.ShowDialog, "successful install with reboot should show a message");
            AssertEqual("Updates successfully installed, however, a reboot is required.", successReboot.Message, "successful reboot message");
            AssertEqual("Installing Updates: Updates successfully installed, however, a reboot is required.", successReboot.DuplicateDescription, "successful reboot duplicate description");

            WpfOperationResultMessage successNoReboot = WpfOperationResultMessage.Create(WuAgent.AgentOperation.InstallingUpdates, WuAgent.RetCodes.Success, false, "Installing Updates", "");
            Assert(!successNoReboot.ShowDialog, "successful install without reboot remains quiet");

            WpfOperationResultMessage inProgress = WpfOperationResultMessage.Create(WuAgent.AgentOperation.InstallingUpdates, WuAgent.RetCodes.InProgress, true, "Installing Updates", "");
            Assert(!inProgress.ShowDialog, "in-progress result remains quiet");
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

        private static void CommandLineArgumentLookupGuardsMissingValues()
        {
            string[] savedArgs = wumgr.Program.args;
            try
            {
                wumgr.Program.args = new[] { "-onclose", "notepad.exe" };
                AssertEqual("notepad.exe", wumgr.Program.GetArg("-onclose"), "present command value");

                wumgr.Program.args = new[] { "-ONCLOSE", "cmd.exe" };
                AssertEqual("cmd.exe", wumgr.Program.GetArg("-onclose"), "case-insensitive command value");

                wumgr.Program.args = new[] { "-onclose", "-tray" };
                AssertEqual("", wumgr.Program.GetArg("-onclose"), "flag after option should not be treated as command");

                wumgr.Program.args = new[] { "-onclose" };
                AssertEqual("", wumgr.Program.GetArg("-onclose"), "missing command value should be empty");

                wumgr.Program.args = new string[0];
                AssertEqual(null, wumgr.Program.GetArg("-onclose"), "absent command option");
            }
            finally
            {
                wumgr.Program.args = savedArgs;
            }
        }

        private static void ManualInstallerExitCodesReportFailures()
        {
            ManualInstallExitCode success = ManualInstallExitCode.FromProcessExitCode(0);
            Assert(success.Success, "zero exit code should succeed");
            Assert(!success.RebootRequired, "zero exit code should not require reboot");

            ManualInstallExitCode rebootRequired = ManualInstallExitCode.FromProcessExitCode(3010);
            Assert(rebootRequired.Success, "3010 should succeed");
            Assert(rebootRequired.RebootRequired, "3010 should require reboot");

            ManualInstallExitCode rebootInitiated = ManualInstallExitCode.FromProcessExitCode(1641);
            Assert(rebootInitiated.Success, "1641 should succeed");
            Assert(rebootInitiated.RebootRequired, "1641 should require reboot");

            ManualInstallExitCode genericFailure = ManualInstallExitCode.FromProcessExitCode(1);
            Assert(!genericFailure.Success, "generic exit code 1 should fail");
            Assert(!genericFailure.RebootRequired, "generic exit code 1 should not require reboot");
        }

        private static void ManualInstallerArgumentsSupportScepPackages()
        {
            AssertEqual("/s /q", ManualInstallArguments.GetExeArguments(@"C:\Updates\KB3209361\scepinstaller_amd64.exe"), "SCEP installer arguments");
            AssertEqual("/q /norestart", ManualInstallArguments.GetExeArguments(@"C:\Updates\ndp48.exe"), ".NET installer arguments");
            AssertEqual("/q /z", ManualInstallArguments.GetExeArguments(@"C:\Updates\generic.exe"), "default exe installer arguments");
        }

        private static void WinInetUnrecognizedSchemeErrorIsNamed()
        {
            string message = UpdateErrors.GetErrorStr(0x80072EE6);
            AssertEqual("The URL scheme could not be recognized or is not supported.", message, "WinINet unrecognized scheme message");
            Assert(!message.Contains("Unknown Error"), "message should not be generic unknown text");
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
            Assert(!StartupUiMode.ShouldStartMinimized(new string[0], false), "normal launch should not start minimized");
            Assert(StartupUiMode.ShouldStartMinimized(new[] { "-minimized" }, false), "dash minimized launch should start minimized");
            Assert(StartupUiMode.ShouldStartMinimized(new[] { "/minimized" }, false), "slash minimized launch should start minimized");
            Assert(StartupUiMode.ShouldStartMinimized(new string[0], true), "persisted option should start minimized");
            Assert(!StartupUiMode.ShouldStartMinimized(new[] { "-tray" }, true), "tray launch should remain hidden instead of minimized");
            Assert(!StartupUiMode.ShouldSearchOnStartup(new string[0]), "normal launch should not force a search");
            Assert(StartupUiMode.ShouldSearchOnStartup(new[] { "-update" }), "dash update launch should search on startup");
            Assert(StartupUiMode.ShouldSearchOnStartup(new[] { "/update" }), "slash update launch should search on startup");
        }

        private static void StartupDefersAgentInitForWpfShell()
        {
            Assert(!StartupUiMode.ShouldInitializeAgentBeforeWindow(StartupUiKind.Wpf), "WPF should show the window before slow WUA initialization");
            Assert(StartupUiMode.ShouldInitializeAgentBeforeWindow(StartupUiKind.WinForms), "WinForms should preserve the legacy initialization order");
        }

        private static void UpdateSelectionStateDetectsUninstallableSelections()
        {
            UpdateSelectionState empty = UpdateSelectionState.FromUpdates(new List<MsUpdate>());
            Assert(!empty.HasSelection, "empty selection should report no selection");
            Assert(!empty.HasUninstallableSelection, "empty selection should report no uninstallable items");

            UpdateSelectionState nonUninstallable = UpdateSelectionState.FromUpdates(new List<MsUpdate>
            {
                new MsUpdate { Attributes = (int)MsUpdate.UpdateAttr.Installed }
            });
            Assert(nonUninstallable.HasSelection, "selected installed update should report a selection");
            Assert(!nonUninstallable.HasUninstallableSelection, "installed update without uninstallable flag should not be eligible");

            UpdateSelectionState uninstallable = UpdateSelectionState.FromUpdates(new List<MsUpdate>
            {
                new MsUpdate { Attributes = (int)MsUpdate.UpdateAttr.Uninstallable }
            });
            Assert(uninstallable.HasSelection, "uninstallable update should report a selection");
            Assert(uninstallable.HasUninstallableSelection, "uninstallable update should be eligible");
        }

        private static void WpfActionStateMirrorsAdminAndListRules()
        {
            WpfActionState pendingNonAdmin = WpfActionState.Create(true, false, false, true, false, true, false, WpfUpdateListKind.Pending);
            Assert(pendingNonAdmin.CanSearch, "active non-busy agent can search");
            Assert(pendingNonAdmin.CanDownload, "selected pending updates can be downloaded");
            Assert(!pendingNonAdmin.CanInstall, "non-admin user cannot install");
            Assert(pendingNonAdmin.CanHide, "selected pending updates can be hidden");

            WpfActionState pendingAdmin = WpfActionState.Create(true, false, true, true, false, true, false, WpfUpdateListKind.Pending);
            Assert(pendingAdmin.CanInstall, "admin user can install selected pending updates");

            WpfActionState installedAdmin = WpfActionState.Create(true, true, true, true, false, true, false, WpfUpdateListKind.Installed);
            Assert(installedAdmin.CanUninstall, "admin user can uninstall selected installed updates");
            Assert(!installedAdmin.CanDownload, "installed updates cannot be downloaded");

            WpfActionState nonUninstallableInstalled = WpfActionState.Create(true, false, true, true, false, true, false, WpfUpdateListKind.Installed);
            Assert(!nonUninstallableInstalled.CanUninstall, "non-uninstallable installed updates should not enable uninstall");

            WpfActionState hidden = WpfActionState.Create(true, false, false, true, false, true, false, WpfUpdateListKind.Hidden);
            Assert(hidden.CanHide, "selected hidden updates can be unhidden");

            WpfActionState busy = WpfActionState.Create(true, false, true, true, true, true, true, WpfUpdateListKind.Pending);
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

        private static void WpfStatusPaneHeightRejectsInvalidPersistedValues()
        {
            WpfStatusPaneHeight height;

            Assert(!WpfStatusPaneHeight.TryCreate("", out height), "missing status pane height should be rejected");
            Assert(!WpfStatusPaneHeight.TryCreate("20", out height), "too-small status pane height should be rejected");
            Assert(!WpfStatusPaneHeight.TryCreate("900", out height), "too-large status pane height should be rejected");
            Assert(WpfStatusPaneHeight.TryCreate("220", out height), "valid status pane height should be accepted");
            AssertEqual(220.0, height.Height, "status pane height");
            AssertEqual(96.0, WpfStatusPaneHeight.Coerce(20), "coerce low height");
            AssertEqual(420.0, WpfStatusPaneHeight.Coerce(900), "coerce high height");
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

        private static void UpdateDatesDisplayWithLocalCulture()
        {
            DateTime date = new DateTime(2021, 5, 14, 9, 30, 0);

            AssertEqual("14/05/2021", UpdateDateFormatter.FormatForDisplay(date, new CultureInfo("en-GB")), "British short date");
            AssertEqual("5/14/2021", UpdateDateFormatter.FormatForDisplay(date, new CultureInfo("en-US")), "US short date");
            AssertEqual("", UpdateDateFormatter.FormatForDisplay(DateTime.MinValue, new CultureInfo("en-US")), "empty minimum date");
        }

        private static void UpdateCacheDatesRoundTripInvariantly()
        {
            DateTime date = new DateTime(2021, 5, 14, 9, 30, 0, DateTimeKind.Local);
            string value = UpdateDateFormatter.SerializeForCache(date);

            DateTime parsed;
            Assert(UpdateDateFormatter.TryDeserializeFromCache(value, new CultureInfo("de-DE"), out parsed), "serialized cache date should parse");
            AssertEqual(date, parsed, "serialized cache date should round-trip");
        }

        private static void UpdateCacheDatesReadLegacyLocalizedValues()
        {
            DateTime parsed;
            Assert(UpdateDateFormatter.TryDeserializeFromCache("15.05.2021", new CultureInfo("de-DE"), out parsed), "legacy German cache date should parse");
            AssertEqual(new DateTime(2021, 5, 15), parsed.Date, "legacy German cache date");
            Assert(!UpdateDateFormatter.TryDeserializeFromCache("", new CultureInfo("en-US"), out parsed), "empty cache date should fail");
            Assert(!UpdateDateFormatter.TryDeserializeFromCache("not a date", new CultureInfo("en-US"), out parsed), "invalid cache date should fail");
        }

        private static void AppLogFormatsLinesWithTimestamps()
        {
            DateTime timestamp = new DateTime(2026, 5, 27, 13, 4, 5);

            AssertEqual("[13:04:05] Searching for updates", AppLog.FormatLine("Searching for updates", timestamp), "timestamped line");
        }

        private static void CancelConfirmationAcceptsOnlyAffirmativeResult()
        {
            Assert(CancelConfirmation.IsConfirmed(DialogResult.Yes), "yes should confirm cancellation");
            Assert(!CancelConfirmation.IsConfirmed(DialogResult.No), "no should not confirm cancellation");
            Assert(!CancelConfirmation.IsConfirmed(DialogResult.Cancel), "cancel should not confirm cancellation");
        }

        private static void SettingsPageVisibilityPreservesUnrelatedHiddenPages()
        {
            AssertEqual("hide:windowsupdate", SettingsPageVisibilityPolicy.SetHidden("", "windowsupdate", true), "empty hide value");
            AssertEqual("hide:appsfeatures;windowsupdate", SettingsPageVisibilityPolicy.SetHidden("hide:appsfeatures", "windowsupdate", true), "append hidden page");
            AssertEqual("hide:appsfeatures;windowsupdate", SettingsPageVisibilityPolicy.SetHidden("hide:appsfeatures;WindowsUpdate", "windowsupdate", true), "avoid duplicate hidden page");
            AssertEqual("hide:appsfeatures", SettingsPageVisibilityPolicy.SetHidden("hide:appsfeatures;windowsupdate", "windowsupdate", false), "remove only target page");
            AssertEqual("", SettingsPageVisibilityPolicy.SetHidden("hide:windowsupdate", "windowsupdate", false), "empty hide list removes value");

            Assert(SettingsPageVisibilityPolicy.IsHidden("hide:appsfeatures;windowsupdate", "windowsupdate"), "target page hidden after another page");
            Assert(!SettingsPageVisibilityPolicy.IsHidden("hide:appsfeatures", "windowsupdate"), "unlisted target is not hidden");

            Assert(SettingsPageVisibilityPolicy.IsHidden("showonly:appsfeatures", "windowsupdate"), "show-only lists hide unlisted pages");
            Assert(!SettingsPageVisibilityPolicy.IsHidden("showonly:appsfeatures;windowsupdate", "windowsupdate"), "show-only lists expose listed pages");
            AssertEqual("showonly:appsfeatures", SettingsPageVisibilityPolicy.SetHidden("showonly:appsfeatures;windowsupdate", "windowsupdate", true), "hide removes target from show-only list");
            AssertEqual("showonly:appsfeatures;windowsupdate", SettingsPageVisibilityPolicy.SetHidden("showonly:appsfeatures", "windowsupdate", false), "unhide adds target to show-only list");
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

        private static void WpfPolicyDisabledReasonExplainsGatedControls()
        {
            LoadEnglishTranslations();

            AssertEqual("Run WuMgr as Administrator to change Auto Update policy settings.",
                WpfPolicyDisabledReason.Get(false, false, GPO.Respect.Full),
                "non-admin policy reason");
            AssertEqual("Store app mode cannot write Windows policy registry settings.",
                WpfPolicyDisabledReason.Get(true, true, GPO.Respect.Full),
                "Store/UWP policy reason");
            AssertEqual("This Windows edition does not support the standard Windows Update policy settings.",
                WpfPolicyDisabledReason.Get(true, false, GPO.Respect.None),
                "unsupported policy reason");
            AssertEqual("This Windows edition only partially supports standard Windows Update policy settings, so some controls are limited.",
                WpfPolicyDisabledReason.Get(true, false, GPO.Respect.Partial),
                "partial support policy reason");
            AssertEqual("",
                WpfPolicyDisabledReason.Get(true, false, GPO.Respect.Full),
                "fully enabled policy reason");
        }

        private static void WpfLocalizedTextMirrorsSharedTranslations()
        {
            LoadEnglishTranslations();

            var text = new WpfLocalizedText();

            AssertEqual(Translate.fmt("lbl_opt"), text.OptionsTab, "options tab");
            AssertEqual(Translate.fmt("lbl_au"), text.AutoUpdateTab, "auto update tab");
            AssertEqual(Translate.fmt("lbl_auto"), text.RunInBackground, "run in background");
            AssertEqual(Translate.fmt("lbl_start_min"), text.StartMinimized, "start minimized");
            AssertEqual(Translate.fmt("lbl_ac_week"), text.AutoUpdateOptions[2], "auto update weekly option");
            AssertEqual(Translate.fmt("lbl_block_ms"), text.BlockMicrosoftServers, "block Microsoft servers");
            AssertEqual(Translate.fmt("tip_inst"), text.InstallButton, "install button");
            AssertEqual(Translate.fmt("col_kb"), text.KbColumn, "KB column");
            AssertEqual("Max Size", text.SizeColumn, "size column should indicate WUA maximum size");
            AssertEqual("Refresh", text.RefreshButton, "button text should strip WinForms mnemonic markers");
            AssertEqual("Exit", text.ExitMenu, "WPF tray menu text should strip WinForms mnemonic markers");
            AssertEqual("Initializing Windows Update Agent...", text.InitializingAgent, "WPF startup status text");
            AssertEqual("Wednesday", text.ScheduleDays[4], "WPF schedule day text");
            AssertEqual("Theme", text.ThemeLabel, "WPF theme label");
            AssertEqual("Search filter:", text.SearchFilter, "WPF search filter label");
            AssertEqual("Clear filter", text.ClearFilter, "WPF clear filter label");
            AssertEqual("Follow system setting", text.ThemeOptions[0], "WPF system theme option");
            AssertEqual("Light", text.ThemeOptions[1], "WPF light theme option");
            AssertEqual("Dark", text.ThemeOptions[2], "WPF dark theme option");
        }

        private static void LoadEnglishTranslations()
        {
            if (englishTranslationsLoaded)
                return;

            wumgr.Program.appPath = Path.Combine(Directory.GetCurrentDirectory(), "wumgr");
            Translate.Load("en");
            englishTranslationsLoaded = true;
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

        private static void WpfProgressVisualHelpersKeepCustomBarReadable()
        {
            AssertEqual("", WpfProgressValue.GetDisplayText(0, false), "idle label");
            AssertEqual("42%", WpfProgressValue.GetDisplayText(42, false), "determinate label");
            AssertEqual("Working...", WpfProgressValue.GetDisplayText(0, true), "indeterminate label");
            AssertEqual(60.0, WpfProgressValue.GetMarqueeWidth(200.0), "normal marquee width");
            AssertEqual(0.0, WpfProgressValue.GetMarqueeWidth(0.0), "zero marquee width");
            AssertEqual(70.0, WpfProgressValue.GetMarqueeLeft(200.0, 60.0, 0.5), "marquee phase");
            AssertEqual(0.0, WpfProgressValue.GetMarqueeLeft(200.0, 60.0, -1.0), "marquee low clamp");
            AssertEqual(140.0, WpfProgressValue.GetMarqueeLeft(200.0, 60.0, 2.0), "marquee high clamp");
        }

        private static void WpfProgressVisibilityFollowsActiveWork()
        {
            Assert(!WpfProgressValue.ShouldShowProgress(false, 0, false), "idle progress should be hidden");
            Assert(WpfProgressValue.ShouldShowProgress(true, 0, false), "busy progress should show before percent arrives");
            Assert(!WpfProgressValue.ShouldShowProgress(false, 42, false), "stale progress should be hidden");
            Assert(!WpfProgressValue.ShouldShowProgress(false, 0, true), "stale indeterminate progress should be hidden");
            Assert(WpfProgressValue.ShouldShowProgress(true, 42, false), "active determinate progress should show");
            Assert(WpfProgressValue.ShouldShowProgress(true, 0, true), "active indeterminate progress should show");
        }

        private static void ProgressStatusIncludesCurrentPercentAndSpeed()
        {
            AssertEqual("Downloading Updates...", ProgressStatusFormatter.Format("Downloading Updates", new WuAgent.ProgressArgs(-1, 0, 0, 0, "")), "indeterminate status");
            AssertEqual("Downloading Updates 2/4 35%", ProgressStatusFormatter.Format("Downloading Updates", new WuAgent.ProgressArgs(4, 50, 2, 35, "")), "current percent status");
            AssertEqual("Downloading Updates 2/4 35% 2.00 MB/s", ProgressStatusFormatter.Format("Downloading Updates", new WuAgent.ProgressArgs(4, 50, 2, 35, "", 2 * 1024 * 1024)), "speed status");
            AssertEqual("", ProgressStatusFormatter.FormatSpeed(0), "zero speed");
            AssertEqual("512 B/s", ProgressStatusFormatter.FormatSpeed(512), "byte speed");
            AssertEqual("2.00 MB/s", ProgressStatusFormatter.FormatSpeed(2 * 1024 * 1024), "megabyte speed");
        }

        private static void WpfActionToolbarUsesModernGlyphIcons()
        {
            WpfActionButtonSpec[] specs = WpfActionButtonSpec.CreateDefault();

            AssertEqual(8, specs.Length, "toolbar action count");
            AssertEqual(WpfActionButtonKind.Refresh, specs[0].Kind, "refresh action order");
            AssertEqual("\uE72C", specs[0].Glyph, "refresh icon");
            AssertEqual(WpfActionButtonKind.Search, specs[1].Kind, "search action order");
            AssertEqual("\uE721", specs[1].Glyph, "search icon");
            AssertEqual("\uE896", specs[2].Glyph, "download icon");
            AssertEqual("\uE7B8", specs[3].Glyph, "install icon");
            AssertEqual("\uE74D", specs[4].Glyph, "uninstall icon");
            AssertEqual("\uE8C5", specs[5].Glyph, "hide icon");
            AssertEqual("\uE71B", specs[6].Glyph, "link icon");
            AssertEqual("\uE711", specs[7].Glyph, "cancel icon");
            AssertEqual("Segoe Fluent Icons, Segoe MDL2 Assets", WpfActionButtonSpec.IconFontFamily, "icon font fallback");
        }

        private static void WpfThemeSettingsPersistAndResolveModes()
        {
            AssertEqual(WpfThemeMode.System, WpfThemeSettings.Parse(null), "null config");
            AssertEqual(WpfThemeMode.System, WpfThemeSettings.Parse(""), "empty config");
            AssertEqual(WpfThemeMode.System, WpfThemeSettings.Parse("unknown"), "unknown config");
            AssertEqual(WpfThemeMode.Light, WpfThemeSettings.Parse("LIGHT"), "light config");
            AssertEqual(WpfThemeMode.Dark, WpfThemeSettings.Parse("dark"), "dark config");

            AssertEqual("system", WpfThemeSettings.ToConfigValue(WpfThemeMode.System), "system config value");
            AssertEqual("light", WpfThemeSettings.ToConfigValue(WpfThemeMode.Light), "light config value");
            AssertEqual("dark", WpfThemeSettings.ToConfigValue(WpfThemeMode.Dark), "dark config value");

            AssertEqual(0, WpfThemeSettings.ToSelectedIndex(WpfThemeMode.System), "system index");
            AssertEqual(1, WpfThemeSettings.ToSelectedIndex(WpfThemeMode.Light), "light index");
            AssertEqual(2, WpfThemeSettings.ToSelectedIndex(WpfThemeMode.Dark), "dark index");
            AssertEqual(WpfThemeMode.System, WpfThemeSettings.FromSelectedIndex(-1), "low index");
            AssertEqual(WpfThemeMode.System, WpfThemeSettings.FromSelectedIndex(3), "high index");

            AssertEqual(WpfThemeMode.Light, WpfThemeSettings.ResolveEffectiveMode(WpfThemeMode.System, true), "system light");
            AssertEqual(WpfThemeMode.Dark, WpfThemeSettings.ResolveEffectiveMode(WpfThemeMode.System, false), "system dark");
            AssertEqual(WpfThemeMode.Light, WpfThemeSettings.ResolveEffectiveMode(WpfThemeMode.Light, false), "forced light");
            AssertEqual(WpfThemeMode.Dark, WpfThemeSettings.ResolveEffectiveMode(WpfThemeMode.Dark, true), "forced dark");
        }

        private static void WpfCaptionGlyphsReflectWindowState()
        {
            AssertEqual("\uE921", WpfWindowCaptionGlyph.Minimize, "minimize glyph");
            AssertEqual("\uE8BB", WpfWindowCaptionGlyph.Close, "close glyph");
            AssertEqual("\uE922", WpfWindowCaptionGlyph.GetMaximizeRestoreGlyph(false), "maximize glyph");
            AssertEqual("\uE923", WpfWindowCaptionGlyph.GetMaximizeRestoreGlyph(true), "restore glyph");
        }

        private static void WpfListSelectionPolicyHidesHistorySelectors()
        {
            Assert(WpfListSelectionPolicy.CanSelectRows(WpfUpdateListKind.Pending), "pending updates should be selectable");
            Assert(WpfListSelectionPolicy.CanSelectRows(WpfUpdateListKind.Installed), "installed updates should be selectable");
            Assert(WpfListSelectionPolicy.CanSelectRows(WpfUpdateListKind.Hidden), "hidden updates should be selectable");
            Assert(!WpfListSelectionPolicy.CanSelectRows(WpfUpdateListKind.History), "history rows should not expose purposeless selection checkboxes");
        }

        private static void WpfUpdateFilterMatchesVisibleColumns()
        {
            var update = new MsUpdate
            {
                Title = "Cumulative Update for Windows",
                Category = "Security Updates",
                KB = "KB5001234",
                Date = new DateTime(2026, 5, 27),
                Size = 1024,
                State = MsUpdate.UpdateState.Pending
            };
            var row = new WpfUpdateRow(update);

            Assert(WpfUpdateFilter.Matches(row, ""), "empty filter should match");
            Assert(WpfUpdateFilter.Matches(row, "kb500"), "filter should match KB case-insensitively");
            Assert(WpfUpdateFilter.Matches(row, "security"), "filter should match category");
            Assert(!WpfUpdateFilter.Matches(row, "office"), "unmatched filter should exclude row");
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
