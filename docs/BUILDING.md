# Building WuMgr

WuMgr is a .NET Framework 4.6.1 desktop application. The maintained fork opens
the WPF shell by default and keeps the legacy WinForms UI available with
`-winforms`.

## Requirements

- Windows
- Visual Studio 2022 Build Tools or Visual Studio with MSBuild
- Windows Update Agent and Task Scheduler COM components

The project references `Microsoft.NETFramework.ReferenceAssemblies.net461` so
the .NET 4.6.1 reference assemblies can be restored during CI and local builds.

## Build

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" wumgr.sln /restore /p:Configuration=Release /p:Platform="Any CPU"
```

## Tests

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" wumgr.Tests\wumgr.Tests.csproj /restore /p:Configuration=Debug /p:Platform=AnyCPU
.\wumgr.Tests\bin\Debug\wumgr.Tests.exe
```

Release verification uses the full solution build plus the Release test binary:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" wumgr.sln /restore /p:Configuration=Release /p:Platform="Any CPU" /m
.\wumgr.Tests\bin\Release\wumgr.Tests.exe
```

## Package

Build `Release|Any CPU` before packaging, then pass the target release version
without the leading `v`:

```powershell
$version = "1.2.2"
.\scripts\package-release.ps1 -Version $version
```

The package script creates `artifacts\WuMgr_v$version.zip` and
`artifacts\SHA256SUMS.txt`. The release workflow runs the same package script
when a `v*` tag is pushed or when the workflow is started manually with a
version input.

## Release Checklist

Use this checklist before publishing a maintained-fork release:

1. Move completed changelog entries from `Unreleased` into the target version
   section.
2. Bump assembly/file versions in `wumgr\Properties\AssemblyInfo.cs`.
3. Build the full solution in `Release|Any CPU`.
4. Run the Release test binary.
5. Package locally with `scripts\package-release.ps1`.
6. Open a pull request and wait for Build, CodeQL, and dependency submission to
   pass.
7. Merge to `master`, tag `vX.Y.Z`, and let the release workflow publish the
   portable zip and `SHA256SUMS.txt`.
8. Download the GitHub release assets, verify the zip contents, confirm
   `Translation.ini` is included, and compare the published SHA256 hashes.

Keep release assets portable unless the release notes explicitly announce a
different packaging format.
