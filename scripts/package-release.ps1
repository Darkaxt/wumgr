param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$releaseDir = Join-Path $repoRoot "artifacts\WuMgr_v$Version"
$zipPath = Join-Path $repoRoot "artifacts\WuMgr_v$Version.zip"
$hashPath = Join-Path $repoRoot "artifacts\SHA256SUMS.txt"
$exePath = Join-Path $repoRoot "wumgr\bin\Release\wumgr.exe"
$configPath = Join-Path $repoRoot "wumgr\bin\Release\wumgr.exe.config"
$translationPath = Join-Path $repoRoot "wumgr\Translation.ini"

if (-not (Test-Path -LiteralPath $exePath)) {
    throw "Release executable not found: $exePath"
}

if (Test-Path -LiteralPath $releaseDir) {
    Remove-Item -LiteralPath $releaseDir -Recurse -Force
}
New-Item -ItemType Directory -Path $releaseDir | Out-Null

Copy-Item -LiteralPath $exePath -Destination $releaseDir
if (Test-Path -LiteralPath $configPath) {
    Copy-Item -LiteralPath $configPath -Destination $releaseDir
}
Copy-Item -LiteralPath (Join-Path $repoRoot "LICENSE") -Destination $releaseDir
Copy-Item -LiteralPath (Join-Path $repoRoot "README.md") -Destination $releaseDir
Copy-Item -LiteralPath (Join-Path $repoRoot "CHANGELOG.md") -Destination $releaseDir
Copy-Item -LiteralPath (Join-Path $repoRoot "PRIVACY_POLICY.md") -Destination $releaseDir

if (Test-Path -LiteralPath $translationPath) {
    Copy-Item -LiteralPath $translationPath -Destination $releaseDir
}

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}
Compress-Archive -Path (Join-Path $releaseDir "*") -DestinationPath $zipPath

$hashes = @()
$hashes += Get-FileHash -Algorithm SHA256 -LiteralPath $zipPath
Get-ChildItem -LiteralPath $releaseDir -File | Sort-Object Name | ForEach-Object {
    $hashes += Get-FileHash -Algorithm SHA256 -LiteralPath $_.FullName
}

$hashes | ForEach-Object {
    "{0}  {1}" -f $_.Hash.ToLowerInvariant(), (Split-Path -Leaf $_.Path)
} | Set-Content -LiteralPath $hashPath -Encoding ASCII

Write-Host "Created $zipPath"
Write-Host "Created $hashPath"
