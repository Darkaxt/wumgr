# Modernization Notes

WuMgr remains a .NET Framework WinForms application for the default maintenance
track. The maintained fork is adding WPF incrementally so the working update
tool remains usable while the UI is migrated.

## WPF Track

The WPF path is an opt-in shell launched with `-wpf`. It provides the target
layout structure and is now wired to core update operations: source selection,
search, list switching, selection, download/install/uninstall, hide/unhide,
link copying, cancellation, progress, single-instance restore, window placement
persistence, tray restore/exit behavior, automatic search scheduling, idle
checks, tray notifications, and the Auto Update policy/options controls for
Windows Update GPO, facilitator services, Store auto-update, Settings-page, and
driver policies. Migrated WPF tabs, buttons, columns, tray menu labels, and
option controls now use the same translation keys as the WinForms UI where
available.

The existing WinForms UI, command-line options, portable zip format, and .NET
Framework target remain unchanged for normal v1.2.x releases until the WPF path
has equivalent behavior and smoke-test coverage.

Remaining WPF migration work includes manual smoke coverage before making WPF
the default UI.
