# Building WuMgr

WuMgr is a WinForms application targeting .NET Framework 4.6.1.

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

## Package

```powershell
.\scripts\package-release.ps1 -Version 1.2.0
```

The package script creates `artifacts\WuMgr_v1.2.0.zip` and
`artifacts\SHA256SUMS.txt`.
