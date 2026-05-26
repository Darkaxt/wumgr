# Security Review Notes for v1.2.0

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
