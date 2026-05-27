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
latest packaged maintenance release tagged `v1.2.1`. Current `master` includes
unreleased WPF modernization work for the next maintenance release.

## Quick Start

1. Download `WuMgr_v1.2.1.zip` from the fork releases page.
2. Unzip it to a writable folder.
3. Run `wumgr.exe`.

The latest packaged release uses the maintained WinForms UI. Current `master`
starts read-only by default and opens the WPF shell unless `-winforms` is used.
Search and review flows can run without elevation, while download, install,
uninstall, policy, service, and Skip UAC changes require an elevated launch.

## Current Master Preview

The maintained fork keeps the original lightweight desktop-tool shape while
modernizing the default WPF shell. This work is currently unreleased. It
supports light mode, dark mode, and following the Windows app theme.

![WuMgr light mode](docs/assets/wumgr-wpf-light.png)

![WuMgr dark mode](docs/assets/wumgr-wpf-dark.png)

The WPF path is wired to the main update lists and actions, restores the
original action icons, uses a progress strip only while work is active, and
keeps the legacy WinForms UI available with `-winforms` while migration testing
continues.

## Features

- Search Windows Update, Microsoft Update, installed updates, hidden updates,
  and update history.
- Download, install, uninstall, hide, unhide, and copy update links from a
  portable executable.
- Run current `master` read-only without elevation, then elevate only for
  admin-only actions.
- Configure automatic update policy controls, Microsoft Update registration,
  offline scan mode, manual download mode, and startup behavior.
- Use a portable release zip with `Translation.ini` next to `wumgr.exe`.
- Force a UI language with `Lang=` in `wumgr.ini` when Windows regional
  settings should not control the app language.

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
