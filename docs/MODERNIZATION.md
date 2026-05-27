# Modernization Notes

WuMgr remains a .NET Framework WinForms application for the default maintenance
track. The maintained fork is adding WPF incrementally so the working update
tool remains usable while the UI is migrated.

## WPF Track

The WPF path is an opt-in shell launched with `-wpf`. It provides the target
layout structure and is now wired to core update operations: source selection,
search, list switching, selection, download/install/uninstall, hide/unhide,
link copying, cancellation, progress, single-instance restore, window placement
persistence, and basic tray restore/exit behavior.

The existing WinForms UI, command-line options, portable zip format, and .NET
Framework target remain unchanged for normal v1.2.x releases until the WPF path
has equivalent behavior and smoke-test coverage.

Remaining WPF migration work includes automatic search, full policy/options
parity, richer tray notifications, and manual smoke coverage before making WPF
the default UI.
