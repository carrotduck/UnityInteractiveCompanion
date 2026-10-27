param(
    [Parameter(Mandatory = $true)]
    [string]$WaveFile
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$resolvedWave = (Resolve-Path -LiteralPath $WaveFile).Path
if ([System.IO.Path]::GetExtension($resolvedWave) -ine '.fbx') {
    throw 'Select the Waving FBX downloaded from Mixamo.'
}
$assetDirectory = Join-Path $repoRoot 'Assets\CharacterAnimations'
$targetWave = Join-Path $assetDirectory 'Waving.fbx'
if (Test-Path -LiteralPath $targetWave) {
    throw 'Waving.fbx already exists. Keep your existing file or move it before restoring.'
}
New-Item -ItemType Directory -Path $assetDirectory -Force | Out-Null
Copy-Item -LiteralPath $resolvedWave -Destination $targetWave
Copy-Item -LiteralPath (Join-Path $repoRoot 'docs\asset-import-settings\Waving.fbx.meta') -Destination ($targetWave + '.meta')
Write-Output 'Wave restored. Reopen Unity and check the Waving state in CompanionHuman.'
