# WuMgr Options

## Options Tab

- Offline Mode: use the Microsoft offline scan CAB instead of an online service.
- Download wsusscn2.cab: refresh the offline scan CAB before searching.
- Manual Download/Install: download update files directly and install them
  outside the Windows Update download cache.
- Include superseded: include potentially superseded updates in searches.
- Register Microsoft Update: include Microsoft product updates, not only Windows.
- Run in background: start WuMgr in the notification area.
- Automatic search: search daily, weekly, or monthly when WuMgr is running in the
  tray and the computer has been idle long enough.
- Always run as Administrator: creates or removes the Skip UAC scheduled task.
  When unchecked, WuMgr starts without prompting and admin-only actions remain
  unavailable until the user launches WuMgr elevated.

## Auto Update Tab

These controls write Windows policy, service, or registry settings and require
administrator rights.

- Block Access to WU Servers: applies policy intended to restrict Microsoft
  Windows Update traffic.
- Disable Automatic Update: disables automatic update behavior.
- Disable Update Facilitators: disables Windows services that can re-enable
  update behavior on some editions.
- Notification Only, Download Only, Scheduled & Installation, Automatic Update:
  configure Windows Update automatic update policy.
- Hide WU Settings Page: hides the Windows Update Settings page.
- Disable Store Auto Update: disables automatic Store app updates.
- Include Drivers: controls driver inclusion in Windows Update policy.

Some options are disabled on Windows editions that ignore the relevant policy or
when WuMgr is not elevated.

For removal and policy recovery steps, see
[UNINSTALL_AND_RECOVERY.md](UNINSTALL_AND_RECOVERY.md).

## WPF Preview

Launch with `-wpf` to open the opt-in WPF shell. The WPF path supports source
selection, search, list switching, selected-update actions, progress, cancel,
single-instance restore, window placement persistence, and basic tray
restore/exit behavior. It also supports automatic search scheduling while the
background tray option is enabled. Full Auto Update policy/options parity and
translation cleanup for WPF-only labels are still handled by the default
WinForms UI while migration continues.
