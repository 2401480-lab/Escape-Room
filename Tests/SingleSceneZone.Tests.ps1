$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$operatingScenePath = Join-Path $root 'Assets/Scenes/Scene_OperatingRoom.unity'
$corridorScenePath = Join-Path $root 'Assets/Scenes/Scene_Corridor.unity'
$dressingScenePath = Join-Path $root 'Assets/Scenes/Scene_DressingRoom.unity'
$sceneLoaderPath = Join-Path $root 'Assets/_Shared/Scripts/SceneLoader.cs'
$sceneLoaderMetaPath = Join-Path $root 'Assets/_Shared/Scripts/SceneLoader.cs.meta'
$zoneManagerPath = Join-Path $root 'Assets/Clues/ZoneManager.cs'
$zoneDoorPath = Join-Path $root 'Assets/Clues/ZoneDoorActivator.cs'
$buildSettingsPath = Join-Path $root 'ProjectSettings/EditorBuildSettings.asset'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $operatingScenePath) 'Scene_OperatingRoom must remain as the integrated scene.'
Assert-True (-not (Test-Path -LiteralPath $corridorScenePath)) 'Scene_Corridor.unity must be removed.'
Assert-True (-not (Test-Path -LiteralPath $dressingScenePath)) 'Scene_DressingRoom.unity must be removed.'
Assert-True (-not (Test-Path -LiteralPath $sceneLoaderPath)) 'SceneLoader.cs must be removed.'
Assert-True (-not (Test-Path -LiteralPath $sceneLoaderMetaPath)) 'SceneLoader.cs.meta must be removed.'
Assert-True (Test-Path -LiteralPath $zoneManagerPath) 'ZoneManager.cs must be added.'
Assert-True (Test-Path -LiteralPath $zoneDoorPath) 'ZoneDoorActivator.cs must be added for door-driven zone activation.'

$scene = Get-Content -LiteralPath $operatingScenePath -Raw -Encoding UTF8
$zoneManager = Get-Content -LiteralPath $zoneManagerPath -Raw -Encoding UTF8
$zoneDoor = Get-Content -LiteralPath $zoneDoorPath -Raw -Encoding UTF8
$buildSettings = Get-Content -LiteralPath $buildSettingsPath -Raw -Encoding UTF8
$allCode = "$zoneManager`n$zoneDoor"
$dontDestroyMatches = & rg 'DontDestroyOnLoad' (Join-Path $root 'Assets') --glob '*.cs'
$dontDestroyExitCode = $LASTEXITCODE

foreach ($rootName in @(
    'HUD_Canvas',
    'Managers',
    'Player',
    'Zone_Lobby',
    'Zone_Corridor',
    'Zone_Ward',
    'Zone_Storage',
    'Zone_DressingRoom',
    'Zone_OperatingRoom'
)) {
    Assert-True ($scene -match "m_Name:\s+$rootName(\r?\n|$)") "Integrated scene missing root object: $rootName"
}

foreach ($zone in @('Lobby', 'Corridor', 'Ward', 'Storage', 'DressingRoom', 'OperatingRoom')) {
    Assert-True ($zoneManager -match $zone) "ZoneManager missing zone enum/member: $zone"
}

Assert-True ($zoneManager -match 'namespace\s+EscapeRoom') 'ZoneManager must use namespace EscapeRoom.'
Assert-True ($zoneManager -match 'class\s+ZoneManager\s*:\s*MonoBehaviour') 'ZoneManager must be a MonoBehaviour.'
Assert-True ($zoneManager -match 'ActivateZone\s*\(\s*ZoneType\s+\w+\s*\)') 'ZoneManager must expose ActivateZone(ZoneType).'
Assert-True ($zoneManager -match 'CurrentZone') 'ZoneManager must track the current active zone.'
Assert-True ($zoneManager -match 'OnZoneEntered') 'ZoneManager must expose a zone-entered text event.'
Assert-True ($zoneManager -match 'Zone_Lobby' -and $zoneManager -match 'SetActive\s*\(') 'ZoneManager must activate Zone_Lobby and toggle zone roots.'
Assert-True ($zoneManager -match 'Start\s*\(\s*\)' -and $zoneManager -match 'ActivateZone\s*\(\s*ZoneType\.Lobby\s*\)') 'ZoneManager must start with only Lobby active.'
Assert-True ($zoneDoor -match 'ActivateNextZone' -and $zoneDoor -match 'ZoneManager\.Instance\.ActivateZone') 'ZoneDoorActivator must activate the next zone for door hooks.'

Assert-True ($buildSettings -match 'Scene_OperatingRoom\.unity') 'Build Settings must include Scene_OperatingRoom.'
Assert-True ($buildSettings -notmatch 'Scene_Corridor\.unity') 'Build Settings must not include Scene_Corridor.'
Assert-True ($buildSettings -notmatch 'Scene_DressingRoom\.unity') 'Build Settings must not include Scene_DressingRoom.'

Assert-True ($dontDestroyExitCode -eq 1) "Single-scene project must not use DontDestroyOnLoad: $dontDestroyMatches"
Assert-True ($allCode -notmatch 'CursorController') 'Zone systems must not touch CursorController.'
Assert-True ($allCode -notmatch 'Time\.timeScale') 'Zone systems must not change Time.timeScale.'

Write-Host 'Single scene zone checks passed.'
