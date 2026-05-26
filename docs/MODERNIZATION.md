# Modernization Notes

WuMgr remains a .NET Framework WinForms application for the current maintenance
track. The goal for the maintained fork is to stabilize security, release
packaging, and reliability before changing the UI framework.

## WPF Track

A WPF migration is a future major modernization project, not a v1.2.x
maintenance task. It should happen on a separate branch with its own design and
test plan because it would touch most UI code, persistence behavior, translation
loading, and manual smoke-test coverage.

The existing WinForms UI, command-line options, portable zip format, and .NET
Framework target remain unchanged for v1.2.x security releases.
