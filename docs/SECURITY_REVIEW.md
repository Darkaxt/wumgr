# Security Review Notes

The v1.2.0 maintenance pass reviewed the code paths most likely to affect local
privilege and update integrity.

## Reviewed

- Skip UAC scheduled task creation and execution.
- `Tools.ini` startup/shutdown command execution.
- `-onclose` command handling when Skip UAC is active.
- Named-pipe single-instance messages.
- Manual download and install flow.
- Windows Update service and policy manipulation.
- PVS-Studio findings reported upstream in issue #152.

## Changes Made

- Manual download planning now treats updates without usable download URLs as
  failed instead of reporting success.
- Manual install no longer continues when any selected update was skipped or any
  download task failed.
- INI read/write imports use Unicode consistently.
- A tracked Visual Studio user file was removed.

## Known Limitations

- Skip UAC remains a powerful local-administrator convenience feature and should
  be disabled by users who do not need it.
- `Tools.ini` command hooks intentionally execute local commands configured by
  the user from the application directory.
- v1.2.0 binaries are unsigned.

## v1.2.1 Follow-up

- Added a result-dialog guard so repeated operation failures do not stack
  duplicate modal dialogs while one result dialog is already open.
- The current repository does not include a Defender update scheduled-task
  artifact to patch for upstream issue #78. The issue remains review input for
  future packaging or documentation work.
- CodeQL passed on the v1.2.1 maintenance branch and on `master` after merge.
- The upstream SAST screenshot in issue #152 remains a review input. No
  additional confirmed code fix was identified for v1.2.1 beyond the dialog
  guard and the v1.2.0 hardening work.
- v1.2.1 binaries remain unsigned.

## v1.2.2 Follow-up

- GitHub code scanning and Dependabot security alerts were checked during the
  pass and both reported zero open alerts.
- Named-pipe single-instance IPC no longer grants `World` full control. The pipe
  ACL is limited to the current Windows user, local Administrators, and Local
  System so single-instance restore still works for the same user and elevated
  process pairings.
- Manual download filenames are derived from the final response URI or
  `Content-Disposition` header through a sanitizer. Directory components are
  stripped, invalid filename characters are replaced, and empty or directory-only
  names continue to fall back to generated `Download_N` names.
- HTTP redirects remain enabled because Microsoft download endpoints may redirect
  through CDN URLs. The app does not force HTTPS-only downloads in this
  maintenance pass to avoid breaking existing Windows Update Catalog links.
- `Tools.ini` `[OnStart]` and `[OnClose]` command hooks remain intentional
  local command-execution features. They run with the privileges used to launch
  WuMgr, so users should keep `Tools.ini` writable only by trusted local users.
- The `-onclose` command-line hook remains disabled during `-NoUAC` Skip UAC
  runs. This preserves the existing local privilege escalation guard.
- Skip UAC remains a local-admin convenience feature. It creates a highest-run
  scheduled task for the interactive user and should only be enabled on machines
  where that local-admin trust boundary is acceptable.
- Update facilitator service hardening still changes service registry state and
  can add a deny ACE for Local System when disabling those services. Recovery
  steps remain documented in `docs/UNINSTALL_AND_RECOVERY.md`.
- WPF migration is deferred. The maintained fork keeps the WinForms UI for
  maintenance releases while security and reliability work stabilizes.
