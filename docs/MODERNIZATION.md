# Modernization Notes

WuMgr remains a .NET Framework WinForms application for the default maintenance
track. The maintained fork is adding WPF incrementally so the working update
tool remains usable while the UI is migrated.

## WPF Track

The first WPF slice is an opt-in preview shell launched with `-wpf`. It provides
the target layout structure and reads the current agent cache, but full update
operations still belong to the default WinForms UI while migration continues.

The existing WinForms UI, command-line options, portable zip format, and .NET
Framework target remain unchanged for normal v1.2.x releases until the WPF path
has equivalent behavior and smoke-test coverage.
