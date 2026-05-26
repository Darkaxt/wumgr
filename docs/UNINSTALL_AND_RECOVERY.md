# Uninstall And Recovery

WuMgr is portable. There is no installer state to remove unless you enabled
startup, Skip UAC, Windows Update policy, service, or Store policy options.

## Remove WuMgr startup entries

1. Run `wumgr.exe` as Administrator.
2. Open the Options tab.
3. Clear `Run in background`.
4. Clear `Always run as Administrator` if it is enabled.
5. Close WuMgr.

`Run in background` removes the current-user startup entry. `Always run as
Administrator` removes the scheduled task used for Skip UAC.

## Restore Windows Update policy

1. Run `wumgr.exe` as Administrator.
2. Open the Auto Update tab.
3. Select `Automatic Update`.
4. Clear `Block Access to WU Servers`.
5. Clear `Disable Automatic Update`.
6. Clear `Hide WU Settings Page`.
7. Clear `Disable Store Auto Update` if you changed it.
8. Reboot Windows.

On some Windows editions, policy changes are not reflected until after a reboot.
If the Windows Settings app still hides the Windows Update page, start WuMgr
again as Administrator, check and then clear `Hide WU Settings Page`, close
WuMgr, and reboot.

## Manual checks

If WuMgr is no longer available, check these locations with Administrator rights:

- Current-user startup entry:
  `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
- Skip UAC scheduled task:
  Task Scheduler Library, task named for WuMgr
- Windows Update policy:
  `HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate`
- Windows Update settings-page visibility:
  `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer`
- Store auto-update policy:
  `HKLM\SOFTWARE\Policies\Microsoft\WindowsStore`

Prefer using WuMgr to restore settings when possible, because it applies the same
policy paths it writes.
