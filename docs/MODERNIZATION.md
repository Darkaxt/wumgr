# Modernization Notes

WuMgr remains a .NET Framework desktop application for the default maintenance
track. The maintained fork has migrated the default shell to WPF while keeping
the legacy WinForms UI available as a fallback.

## WPF Track

The WPF path is the default shell. It provides the target layout structure and
is wired to core update operations: source selection,
search, list switching, selection, download/install/uninstall, hide/unhide,
link copying, cancellation, progress, single-instance restore, window placement
persistence, tray restore/exit behavior, automatic search scheduling, idle
checks, tray notifications, and the Auto Update policy/options controls for
Windows Update GPO, facilitator services, Store auto-update, Settings-page, and
driver policies. Migrated WPF tabs, buttons, columns, tray menu labels, and
option controls now use the same translation keys as the WinForms UI where
available. The WPF shell is shown before Windows Update Agent initialization so
slow WUA startup does not make launch appear stalled.

The existing WinForms UI remains available with `-winforms` for fallback
testing. The executable name, portable zip format, and .NET Framework target are
unchanged.

Remaining WPF migration work includes broader manual smoke coverage before the
next release.
