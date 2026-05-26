# WuMgr

## Maintained fork

This repository is a public maintained fork of
[DavidXanatos/wumgr](https://github.com/DavidXanatos/wumgr). The first forked
maintenance release is `v1.2.0`, focused on build hygiene, documentation,
release packaging, and conservative bug fixes.

Current fork releases are published at
[Darkaxt/wumgr releases](https://github.com/Darkaxt/wumgr/releases).

## Overview
WuMgr (Update Manager for Windows) is a tool to manage updates of Microsoft products on the Windows operating system.
It uses the ["Windows Update Agent API"](https://docs.microsoft.com/en-us/windows/win32/wua_sdk/portal-client) to identify as well as download and install missing updates.
It allows the user fine control of updates on modern (Windows 10) operating system versions, comparable to what windows 7 and 8.1 offered.

This tool is inspired by the [Windows Update Mini Tool (WUMT)](https://www.majorgeeks.com/files/details/windows_update_minitool.html), however in comparison to WUMT it is written in pure .NET instead of C/C++, and it is open source. 

Here are my official donation options:
* ETH: `0xBf08c3c47C5175015cEF4E32fB2315c9111F5305`
* LTC: `LTqXK1UEri1FCv7fNn9bcFhsrh78SaNdSM`
* BTC: `18tQgfoog4VyespgskpNiaNFMTdcm6j5Gd`

And if you preffer more legit payment methods, here is [my pateron page](https://www.patreon.com/DavidXanatos): https://www.patreon.com/DavidXanatos

Icons provided by: 
* Icons8-com (http://icons8.com/)

## Building and releases

See [docs/BUILDING.md](docs/BUILDING.md) for local build, test, and packaging
commands. Release zips include `wumgr.exe`, `Translation.ini`, project docs,
and SHA256 hashes.

See [docs/OPTIONS.md](docs/OPTIONS.md) for option descriptions,
[docs/UNINSTALL_AND_RECOVERY.md](docs/UNINSTALL_AND_RECOVERY.md) for uninstall
and Windows Update policy recovery, [docs/ISSUE_TRIAGE.md](docs/ISSUE_TRIAGE.md)
for upstream issue triage, and [SECURITY.md](SECURITY.md) for security
reporting.
