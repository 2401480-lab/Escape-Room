$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$scenePath = Join-Path $root 'Assets/Scenes/Scene_OperatingRoom.unity'
$legacyPackagePath = Join-Path $root 'Assets/3rdPerson+Fly'
$legacyPackageMetaPath = Join-Path $root 'Assets/3rdPerson+Fly.meta'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

$requiredAssets = @(
    'Assets/Room02_Operating/Models/char_shadow.fbx',
    'Assets/Room02_Operating/Models/char_shadow.fbx.meta',
    'Assets/Room02_Operating/Materials/char_shadow.mat',
    'Assets/Room02_Operating/Materials/char_shadow.mat.meta',
    'Assets/Room02_Operating/Materials/char_shadow_toon.cubemap',
    'Assets/Room02_Operating/Materials/char_shadow_toon.cubemap.meta',
    'Assets/Room02_Operating/Textures/char_shadow_darker.png',
    'Assets/Room02_Operating/Textures/char_shadow_darker.png.meta',
    'Assets/Room02_Operating/Textures/char_shadow_ligther.png',
    'Assets/Room02_Operating/Textures/char_shadow_ligther.png.meta'
)

foreach ($asset in $requiredAssets) {
    Assert-True (Test-Path -LiteralPath (Join-Path $root $asset)) "Missing Room02 culprit visual asset: $asset"
}

Assert-True (-not (Test-Path -LiteralPath $legacyPackagePath)) 'Legacy 3rdPerson+Fly package folder must not remain in the project.'
Assert-True (-not (Test-Path -LiteralPath $legacyPackageMetaPath)) 'Legacy 3rdPerson+Fly package meta file must not remain in the project.'

$culpritMetaFiles = $requiredAssets | Where-Object { $_ -like '*.meta' }
foreach ($metaFile in $culpritMetaFiles) {
    $metaPath = Join-Path $root $metaFile
    $meta = Get-Content -LiteralPath $metaPath -Raw -Encoding UTF8
    Assert-True ($meta -notmatch 'Assets/3rdPerson\+Fly') "Culprit asset meta must not reference legacy package path: $metaFile"
}

$scene = Get-Content -LiteralPath $scenePath -Raw -Encoding UTF8
Assert-True ($scene -notmatch 'char_shadow') 'Culprit visual asset should be imported only; it must not be silently placed into Scene_OperatingRoom.'

Write-Host 'Culprit asset scope checks passed.'
