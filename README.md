# WuMgr

WuMgr (Update Manager for Windows) is a portable Windows Update manager for
Windows 10 and Windows 11. It uses the
[Windows Update Agent API](https://learn.microsoft.com/windows/win32/wua_sdk/portal-client)
to search, download, install, hide, and review Microsoft product updates with
more direct control than the built-in Settings app exposes.

This repository is a public maintained fork of
[DavidXanatos/wumgr](https://github.com/DavidXanatos/wumgr). Current fork
releases are published at
[Darkaxt/wumgr releases](https://github.com/Darkaxt/wumgr/releases), with the
latest packaged maintenance release tagged `v1.2.1`.

## Interface

The maintained fork keeps the original lightweight desktop-tool shape while
modernizing the default WPF shell. The app supports light mode, dark mode, and
following the Windows app theme.

![WuMgr light mode](docs/assets/wumgr-wpf-light.png)

![WuMgr dark mode](docs/assets/wumgr-wpf-dark.png)

## Features

- Search Windows Update, Microsoft Update, installed updates, hidden updates,
  and update history.
- Download, install, uninstall, hide, unhide, and copy update links from a
  portable executable.
- Run read-only without elevation, then elevate only for admin-only actions.
- Configure automatic update policy controls, Microsoft Update registration,
  offline scan mode, manual download mode, and startup behavior.
- Use a portable release zip with `Translation.ini` next to `wumgr.exe`.

## Documentation

- [Build and release commands](docs/BUILDING.md)
- [Options reference](docs/OPTIONS.md)
- [Uninstall and Windows Update recovery](docs/UNINSTALL_AND_RECOVERY.md)
- [Security review notes](docs/SECURITY_REVIEW.md)
- [Upstream issue triage](docs/ISSUE_TRIAGE.md)
- [Modernization notes](docs/MODERNIZATION.md)
- [Security reporting](SECURITY.md)

## Attribution

WuMgr was created by
[DavidXanatos](https://github.com/DavidXanatos) and is inspired by
[Windows Update Mini Tool (WUMT)](https://www.majorgeeks.com/files/details/windows_update_minitool.html).
This fork preserves the original GPLv3 licensing and attribution while
continuing maintenance in `Darkaxt/wumgr`.

To support the original author, use
[DavidXanatos on Patreon](https://www.patreon.com/DavidXanatos).

Legacy WinForms icons are provided by [Icons8](https://icons8.com/).
