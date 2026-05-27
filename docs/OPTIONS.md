# WuMgr Options

## Options Tab

- Offline Mode: use the Microsoft offline scan CAB instead of an online service.
- Download wsusscn2.cab: refresh the offline scan CAB before searching.
- Manual Download/Install: download update files directly and install them
  outside the Windows Update download cache.
- Include superseded: include potentially superseded updates in searches.
- Register Microsoft Update: include Microsoft product updates, not only Windows.
- Run in background: start WuMgr in the notification area.
- Start minimized: show WuMgr minimized on launch without hiding it in the tray.
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

## Update Size Column

The Max Size column shows the maximum download size reported by the Windows
Update API. For cumulative and feature updates, Windows may report a much larger
upper bound than the bytes actually downloaded for the current machine.

For removal and policy recovery steps, see
[UNINSTALL_AND_RECOVERY.md](UNINSTALL_AND_RECOVERY.md).

## UI Mode

WuMgr opens the WPF shell by default. Use `-winforms` to launch the legacy
WinForms UI for fallback testing. `-wpf` is still accepted and selects the
default WPF shell explicitly.

Use `-tray` to start hidden in the notification area. Use `-minimized` to show
the main window minimized on the taskbar. `-tray` takes precedence when both
are supplied.

Use `-update` to start a search after the Windows Update Agent is initialized.
It can be combined with `-tray` for a hidden startup search.

## Language Override

WuMgr chooses the UI language from the installed Windows UI culture by default.
To force a specific translation without changing Windows regional settings, add
the language code to `wumgr.ini` next to `wumgr.exe`:

```ini
[Options]
Lang=en
```

Use the section names from `Translation.ini`, such as `en`, `de`, `fr`, `it`,
`ja`, `pl`, `pt`, `pt-br`, `ru`, or `zh-cmn-Hans-CN`. Missing translated
strings fall back to the built-in English text.

The WPF path supports source selection, search, list switching, selected-update
actions, progress, cancel, single-instance restore, window placement
persistence, and tray restore/exit behavior. It also supports automatic search
scheduling while the background tray option is enabled, plus Auto Update
policy/options controls. The migrated WPF controls use the same translation keys
as the WinForms UI where available.
