# Upstream Pull Request Triage

This fork reviewed the open pull requests from
[DavidXanatos/wumgr](https://github.com/DavidXanatos/wumgr) and tracks the
maintenance decision here. The fork does not merge upstream pull requests
directly when they are stale, conflicting, or contain unrelated churn; safe
changes are reimplemented or cherry-picked into focused fork pull requests.

## Handled In The Fork

- [#114](https://github.com/DavidXanatos/wumgr/pull/114) Unicode INI
  read/write support. The maintained fork applies Unicode imports consistently
  for both `GetPrivateProfileString` and `WritePrivateProfileString`.
- [#48](https://github.com/DavidXanatos/wumgr/pull/48) Add `.gitignore`. The
  fork uses a clean Visual Studio `.gitignore` and removed tracked user files.
- [#20](https://github.com/DavidXanatos/wumgr/pull/20) Notification text fix.
  The fork reimplemented the useful behavior so update notifications list
  update titles instead of formatting the update collection object.

## Partially Handled

- [#75](https://github.com/DavidXanatos/wumgr/pull/75) Typo fixes. The fork
  did not merge the PR because it contains a compile-breaking stray `l` after
  `if (ret == RetCodes.Success)`. Safe typo/comment fixes were cherry-picked or
  independently corrected where they still applied.

## Deferred

- [#49](https://github.com/DavidXanatos/wumgr/pull/49) User interface
  improvements. The PR is old, broad, and conflicts with the maintained fork's
  current WinForms/WPF split. Useful UI goals have been addressed separately in
  focused WPF work, while the original PR remains too large to merge safely.
