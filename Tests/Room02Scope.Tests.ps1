$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$room02ScenePath = Join-Path $root 'Assets/Room02_Operating/Scenes/Show.unity'
$oldScenePath = Join-Path $root 'Assets/Scenes/Scene_OperatingRoom.unity'
$room02ShowScenePath = Join-Path $root 'Assets/Room02_Operating/Scenes/Show.unity'
$oldShowScenePath = Join-Path $root 'Assets/Abandoned_Asylum/Show.unity'
$oldShowFolderPath = Join-Path $root 'Assets/Abandoned_Asylum/Show'
$oldShowAssetsScenePath = Join-Path $root 'Assets/Abandoned_Asylum/Show_Assets.unity'
$oldShowAssetsFolderPath = Join-Path $root 'Assets/Abandoned_Asylum/Show_Assets'
$room02BgmPath = Join-Path $root 'Assets/Room02_Operating/Audio/music/darkness/dk-theroom.aif'
$oldBgmPath = Join-Path $root 'Assets/music/darkness/dk-theroom.aif'
$room02IntroSfxPath = Join-Path $root 'Assets/Room02_Operating/Resources/Audio/SFX/Deadly Kombat Free version/guts_and_gore_19.wav'
$oldIntroSfxFolderPath = Join-Path $root 'Assets/Deadly Kombat Free version'
$buildSettingsPath = Join-Path $root 'ProjectSettings/EditorBuildSettings.asset'
$setupToolPath = Join-Path $root 'Assets/Room02_Operating/Clues/Editor/ClueSceneSetupTool.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $room02ScenePath) 'Room02 gameplay scene must live under Assets/Room02_Operating/Scenes.'
Assert-True (-not (Test-Path -LiteralPath $oldScenePath)) 'Room02 gameplay scene must not remain in Assets/Scenes.'
Assert-True (Test-Path -LiteralPath $room02ShowScenePath) 'Room02 Show source scene must live under Assets/Room02_Operating/Scenes.'
Assert-True (-not (Test-Path -LiteralPath $oldShowScenePath)) 'Show.unity must not remain under Assets/Abandoned_Asylum.'
Assert-True (-not (Test-Path -LiteralPath $oldShowFolderPath)) 'Show lighting data folder must not remain under Assets/Abandoned_Asylum.'
Assert-True (-not (Test-Path -LiteralPath $oldShowAssetsScenePath)) 'Show_Assets.unity must not remain under Assets/Abandoned_Asylum.'
Assert-True (-not (Test-Path -LiteralPath $oldShowAssetsFolderPath)) 'Show_Assets lighting folder must not remain under Assets/Abandoned_Asylum.'
Assert-True (Test-Path -LiteralPath $room02BgmPath) 'Room02 selected BGM must live under Assets/Room02_Operating/Audio.'
Assert-True (-not (Test-Path -LiteralPath $oldBgmPath)) 'Room02 selected BGM must not remain under Assets/music.'
Assert-True (Test-Path -LiteralPath $room02IntroSfxPath) 'Room02 intro SFX must live under Assets/Room02_Operating/Resources/Audio/SFX.'
Assert-True (-not (Test-Path -LiteralPath $oldIntroSfxFolderPath)) 'Imported intro SFX package must not remain at the Assets root.'

$buildSettings = Get-Content -LiteralPath $buildSettingsPath -Raw -Encoding UTF8
$setupTool = Get-Content -LiteralPath $setupToolPath -Raw -Encoding UTF8

Assert-True ($buildSettings -match 'Assets/Room02_Operating/Scenes/Show\.unity') 'Build Settings must point to the Room02-owned Show scene path.'
Assert-True ($buildSettings -notmatch 'Assets/Scenes/Scene_OperatingRoom\.unity') 'Build Settings must not point to the old shared scene path.'
Assert-True ($setupTool -match 'Assets/Room02_Operating/Scenes/Show\.unity') 'Room02 clue setup tool must target the Room02-owned Show scene path.'
Assert-True ($setupTool -notmatch 'Assets/Scenes/Scene_OperatingRoom\.unity') 'Room02 clue setup tool must not target the old shared scene path.'

Write-Host 'Room02 scope checks passed.'
