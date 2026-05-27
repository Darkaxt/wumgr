# WuMgr User Guide

This guide describes the maintained fork at `Darkaxt/wumgr`.

## Choose A Build

- Use the latest release zip for normal use. The current packaged release is
  `v1.2.1`, which is the conservative WinForms maintenance build.
- Use current `master` only when testing unreleased `v1.2.2` changes. Current
  `master` opens the WPF shell by default and keeps the WinForms UI available
  with `-winforms`.
- Use the release asset `SHA256SUMS.txt` to check downloaded archives after
  extraction.

Published binaries are unsigned, so Windows may show a SmartScreen or publisher
warning.

## First Run

1. Extract the zip to a writable folder.
2. Start `wumgr.exe`.
3. Use Search or Refresh to load available updates.
4. Relaunch as Administrator before downloading, installing, uninstalling,
   hiding, changing services, or changing Windows Update policy.

Current `master` starts read-only without prompting for UAC unless Skip UAC was
explicitly configured. The packaged `v1.2.1` release may still request elevation
earlier because it uses the older WinForms startup path.

## Elevation Rules

| Action | Administrator required | Notes |
| --- | --- | --- |
| View available, installed, hidden, and history lists | No | Depends on Windows Update Agent availability. |
| Search for updates | Usually no | Some Windows policy or service states can still block searches. |
| Download, install, uninstall, hide, or unhide updates | Yes | These change Windows Update state. |
| Change Auto Update tab policy controls | Yes | These write service, registry, or policy settings. |
| Enable Always run as Administrator | Yes | Creates the Skip UAC scheduled task. |
| Run custom `Tools.ini` hooks | Uses current process rights | Treat local hook files as trusted local-admin configuration. |

## Update Sources

- `Windows Update` searches the default Windows Update service.
- `Microsoft Update` includes Microsoft product updates when registered.
- `Offline Mode` uses the Microsoft offline scan CAB.
- `Download wsusscn2.cab` refreshes the offline scan CAB before searching.
- `Manual Download/Install` downloads update files directly and then runs the
  downloaded installer outside the normal Windows Update cache.

Manual install support depends on each update package accepting silent install
arguments. The fork handles common package types and known SCEP installer
arguments, but some vendor packages still need reproduction data before custom
handling can be added.

## Command-Line Options

These options are supported by the application entry point:

```text
-tray             Start hidden in the notification area
-minimized        Start minimized on the taskbar
-onclose [cmd]    Execute a command when WuMgr closes
-update           Search for updates after startup
-winforms         Use the legacy WinForms UI
-wpf              Use the WPF UI
-console          Show a debug console
-help or /?       Show command-line help
```

The legacy WinForms path also recognizes older compatibility switches such as
`-online [serviceId]`, `-offline [download|no_download]`, `-manual`, and
`-provisioned`. Current WPF work uses the saved options UI/configuration for
those modes instead of fully reimplementing every legacy startup switch.

## Background Operation

- `Run in background` registers a current-user startup entry and keeps WuMgr in
  the notification area when the main window is closed.
- `Start minimized` starts the main window minimized without hiding it in the
  tray.
- Automatic search runs only while WuMgr is running in the tray and the machine
  has been idle for the configured delay.
- `Always run as Administrator` configures the Skip UAC scheduled task. Disable
  it when you no longer need silent elevated startup.

## Files And Configuration

WuMgr is portable and stores most local state next to `wumgr.exe` when the
folder is writable:

- `wumgr.ini`: options, window placement, source selection, and language
  override.
- `Updates.ini`: cached update metadata.
- `Translation.ini`: portable translation strings shipped with the release.
- `Tools\Tools.ini`: optional local command hooks and tools-menu entries.

If the application folder is read-only, WuMgr falls back to a `WuMgr` folder
under the user's Downloads directory for writable state.

## Troubleshooting

- Use current `master` with `-winforms` if a WPF regression blocks an update
  workflow.
- Use `-console` when collecting logs for a bug report.
- Include the Windows version, whether WuMgr was elevated, selected update
  titles, KB numbers when present, and the exact error text from the status log.
- For manual install failures, include the downloaded package filename and the
  process exit code if WuMgr logs one.
- For policy or uninstall recovery, follow
  [UNINSTALL_AND_RECOVERY.md](UNINSTALL_AND_RECOVERY.md).

Open focused fork issues for reproducible problems. Upstream issue history is
tracked in [ISSUE_TRIAGE.md](ISSUE_TRIAGE.md), but the fork does not mirror every
historical upstream issue.
