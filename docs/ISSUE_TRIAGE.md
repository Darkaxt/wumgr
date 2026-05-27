# Upstream Issue Triage

This fork tracks upstream issues from
[DavidXanatos/wumgr](https://github.com/DavidXanatos/wumgr) as maintenance
input. The fork does not mirror every upstream issue. Focused fork issues are
opened only when the work is actionable for this maintained fork.

## Already handled by v1.2.0

- [#5](https://github.com/DavidXanatos/wumgr/issues/5) Window position, size, column sizes, not saved or restored
- [#6](https://github.com/DavidXanatos/wumgr/issues/6) Help file
- [#94](https://github.com/DavidXanatos/wumgr/issues/94) Options description
- [#103](https://github.com/DavidXanatos/wumgr/issues/103) Translation does not work v.1.1
- [#104](https://github.com/DavidXanatos/wumgr/issues/104) Which version of Visual Studio is needed with this code?
- [#108](https://github.com/DavidXanatos/wumgr/issues/108) Looking for documentation for the Options and Auto Update tabs attributes
- [#117](https://github.com/DavidXanatos/wumgr/issues/117) Installing updates fails, if list is not refreshed
- [#135](https://github.com/DavidXanatos/wumgr/issues/135) Add Swedish translation to WuMgr
- [#142](https://github.com/DavidXanatos/wumgr/issues/142) Column settings are not saved
- [#145](https://github.com/DavidXanatos/wumgr/issues/145) Missing download URLs wrongly report success
- [#146](https://github.com/DavidXanatos/wumgr/issues/146) Log messages are not copyable

## Partially handled

- [#54](https://github.com/DavidXanatos/wumgr/issues/54) Large update size reports; size is now labeled as the Windows Update API maximum, not an exact payload size
- [#59](https://github.com/DavidXanatos/wumgr/issues/59) Large update size reports; size is now labeled as the Windows Update API maximum, not an exact payload size
- [#99](https://github.com/DavidXanatos/wumgr/issues/99) Large update size reports; size is now labeled as the Windows Update API maximum, not an exact payload size
- [#138](https://github.com/DavidXanatos/wumgr/issues/138) Options documentation and large update size report
- [#113](https://github.com/DavidXanatos/wumgr/issues/113) Automatic search/startup search; `-update` startup search is restored in the WPF path, while scheduled automatic search still requires the background tray option
- [#87](https://github.com/DavidXanatos/wumgr/issues/87) Jittering during large update operations; duplicate dialogs and WPF first-render progress issues are mitigated, but the original install/download failure still needs reproduction data
- [#85](https://github.com/DavidXanatos/wumgr/issues/85) Internal error downloading updates; reported error `0x80072EE6` is now mapped to a WinINet URL-scheme message, while the underlying update failure still needs reproduction data
- [#10](https://github.com/DavidXanatos/wumgr/issues/10) Unavailable settings; the default WPF Auto Update tab now explains common disabled-control reasons
- [#14](https://github.com/DavidXanatos/wumgr/issues/14) Disable Windows Update; Hide WU Settings Page now preserves unrelated `SettingsPageVisibility` entries, while the Settings app crash report still needs reproduction data
- [#129](https://github.com/DavidXanatos/wumgr/issues/129) Disabled facilitator and WU Settings controls are greyed out; the default WPF Auto Update tab now explains common disabled-control reasons
- [#41](https://github.com/DavidXanatos/wumgr/issues/41) Does not find available updates; search criteria now avoids the undocumented `DeploymentAction=*` wildcard and explicitly includes optional-installation groups, while machine-specific missing updates still need reproduction data
- [#112](https://github.com/DavidXanatos/wumgr/issues/112) Update failures reported as applied; manual installer exit code `1` is now treated as failure instead of success, while broader Windows Update applicability failures still need reproduction data
- [#137](https://github.com/DavidXanatos/wumgr/issues/137) Packages without a KB cite download but do not appear; manual downloads now use the package title as the folder/key for `KBUnknown` updates, while the requested separate Drivers folder remains a deferred UX change
- [#82](https://github.com/DavidXanatos/wumgr/issues/82) SQL Server updates support; manual installer command lines are now logged, redirected child-process output is drained to avoid output-pipe hangs, and Cancel can stop a still-running child installer, while SQL-specific silent switches still need reproduction data
- [#83](https://github.com/DavidXanatos/wumgr/issues/83) Office 2013 updates are not installed; manual installer command and failure output logging is improved, while Office-specific install handling still needs a reproducible package/exit-code example
- [#151](https://github.com/DavidXanatos/wumgr/issues/151) Shows 0 updates; search criteria now avoids the undocumented `DeploymentAction=*` wildcard and explicitly includes optional-installation groups, while zero-result environments still need local Windows Update log/repro data

## Handled after v1.2.0

- [#58](https://github.com/DavidXanatos/wumgr/issues/58) Select All not updating available actions
- [#71](https://github.com/DavidXanatos/wumgr/issues/71) Typo in changelog.md
- [#73](https://github.com/DavidXanatos/wumgr/issues/73) Typo in repository description
- [#90](https://github.com/DavidXanatos/wumgr/issues/90) Link to download new releases
- [#96](https://github.com/DavidXanatos/wumgr/issues/96) Cannot remove Group Policy
- [#109](https://github.com/DavidXanatos/wumgr/issues/109) Uninstall WUMGR
- [#119](https://github.com/DavidXanatos/wumgr/issues/119) DPIAwareness does not work properly in v1.1
- [#141](https://github.com/DavidXanatos/wumgr/issues/141) Program can open behind other windows
- [#144](https://github.com/DavidXanatos/wumgr/issues/144) Hide WU Settings Page is too unobtrusive
- [#149](https://github.com/DavidXanatos/wumgr/issues/149) Typo: "managemetn"
- [#152](https://github.com/DavidXanatos/wumgr/issues/152) SAST analysis results; tracked in security review notes, CodeQL, and issue #3
- [#40](https://github.com/DavidXanatos/wumgr/issues/40) Add time marks in log panel
- [#55](https://github.com/DavidXanatos/wumgr/issues/55) Launch minimized
- [#65](https://github.com/DavidXanatos/wumgr/issues/65) Search button
- [#69](https://github.com/DavidXanatos/wumgr/issues/69) Progress percentage and download speed; download speed is shown for manual HTTP downloads where the fork controls the transfer
- [#80](https://github.com/DavidXanatos/wumgr/issues/80) Ability to resize and copy the output/status window
- [#102](https://github.com/DavidXanatos/wumgr/issues/102) WUMgr logging and resizing
- [#91](https://github.com/DavidXanatos/wumgr/issues/91) SCEP update KB3209361 install error; manual SCEP installers now use `/s /q` arguments
- [#45](https://github.com/DavidXanatos/wumgr/issues/45) Delete KB scan never stops; Uninstall is now enabled only when the selected installed updates include at least one uninstallable update
- [#44](https://github.com/DavidXanatos/wumgr/issues/44) Stacked error windows; guarded in both WinForms and WPF result dialogs
- [#95](https://github.com/DavidXanatos/wumgr/issues/95) Non-local date format
- [#121](https://github.com/DavidXanatos/wumgr/issues/121) Update history showing wrong date; mitigated by invariant update-cache dates and legacy localized cache parsing
- [#132](https://github.com/DavidXanatos/wumgr/issues/132) Program not asking for reboot; the WPF shell now reports successful installs that require a reboot
- [#133](https://github.com/DavidXanatos/wumgr/issues/133) Cancellation confirmation
- [#143](https://github.com/DavidXanatos/wumgr/issues/143) Honor system dark mode
- [#105](https://github.com/DavidXanatos/wumgr/issues/105) Switch GUI language from German to English; documented the existing `Lang=en` override

## Needs reproduction before code changes

- [#60](https://github.com/DavidXanatos/wumgr/issues/60) Fresh Windows 8.1 install cannot launch WuMgr
- [#61](https://github.com/DavidXanatos/wumgr/issues/61) Options page does not apply some settings
- [#100](https://github.com/DavidXanatos/wumgr/issues/100) Hide option does not work in Store update manager
- [#107](https://github.com/DavidXanatos/wumgr/issues/107) Repeated failures on Windows 10
- [#118](https://github.com/DavidXanatos/wumgr/issues/118) Windows Server 2019 hidden update behavior
- [#139](https://github.com/DavidXanatos/wumgr/issues/139) Windows 11 upgrade not applied on reboot
- [#153](https://github.com/DavidXanatos/wumgr/issues/153) Surface Slim Pen not charging

## Deferred feature or major-scope requests

- [#42](https://github.com/DavidXanatos/wumgr/issues/42) Select remote computers
- [#50](https://github.com/DavidXanatos/wumgr/issues/50) Installer-based version
- [#53](https://github.com/DavidXanatos/wumgr/issues/53) Command-line version
- [#57](https://github.com/DavidXanatos/wumgr/issues/57) MMC snap-in and Control Panel object
- [#63](https://github.com/DavidXanatos/wumgr/issues/63) Check certificates and repair Windows Update
- [#79](https://github.com/DavidXanatos/wumgr/issues/79) Feature update version options
- [#93](https://github.com/DavidXanatos/wumgr/issues/93) Block specific updates and filter by size
- [#97](https://github.com/DavidXanatos/wumgr/issues/97) Security Intelligence update automation only
- [#98](https://github.com/DavidXanatos/wumgr/issues/98) 32-bit version
- [#110](https://github.com/DavidXanatos/wumgr/issues/110) Defer updates
- [#116](https://github.com/DavidXanatos/wumgr/issues/116) Manage apps and app updates

## Support or meta

- [#78](https://github.com/DavidXanatos/wumgr/issues/78) Defender Updates task when Windows Update service is disabled; the fork currently has no Defender update scheduled-task artifact to patch
- [#134](https://github.com/DavidXanatos/wumgr/issues/134) Need for changes with Windows 11
- [#140](https://github.com/DavidXanatos/wumgr/issues/140) Future of Windows 11
- [#147](https://github.com/DavidXanatos/wumgr/issues/147) Dead project?

## Low priority or insufficiently actionable

- [#92](https://github.com/DavidXanatos/wumgr/issues/92) KB4549951
- [#101](https://github.com/DavidXanatos/wumgr/issues/101) General pre-test questions
