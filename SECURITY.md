# Security Policy

## Supported Versions

This maintained fork supports the latest release published from `Darkaxt/wumgr`.
Older upstream releases are preserved for GPL compliance and historical reference,
but fixes are only shipped from this fork after `v1.2.0`.

## Reporting a Vulnerability

Open a private security advisory on GitHub if available, or create an issue with
minimal reproduction details and no exploit payload. Include:

- Windows version and edition
- WuMgr version
- Whether the app was running elevated
- Affected feature, such as Skip UAC, Tools.ini command hooks, manual downloads,
  Windows Update service control, or GPO changes
- For Skip UAC reports, whether the `WuMgrNoUAC` scheduled task is enabled
- For command-hook reports, the relevant `Tools.ini` section or command-line
  option, with secrets and exploit payloads removed

## Security-Sensitive Areas

WuMgr performs administrative Windows Update tasks. Review changes carefully when
they touch:

- scheduled task creation for the Skip UAC option
- `Tools.ini`, `-onclose`, or any command execution path
- named-pipe IPC used for single-instance activation
- manual update download and install flows
- service and registry/GPO manipulation

Published binaries are unsigned unless release notes state otherwise.

For the current security review status and accepted local-admin trust
boundaries, see [docs/SECURITY_REVIEW.md](docs/SECURITY_REVIEW.md).
