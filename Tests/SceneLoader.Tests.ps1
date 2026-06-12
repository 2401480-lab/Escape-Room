$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$sceneNames = @(
    'Scene_Corridor',
    'Scene_DressingRoom',
    'Scene_OperatingRoom'
)

function Assert-True {
    param(
        [bool] $Condition,
        [string] $Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Get-MetaGuid {
    param([string] $MetaPath)

    $guidLine = Select-String -LiteralPath $MetaPath -Pattern '^guid:\s*([0-9a-f]{32})$'
    Assert-True ($null -ne $guidLine) "Missing Unity guid in $MetaPath"
    return $guidLine.Matches[0].Groups[1].Value
}

$buildSettingsPath = Join-Path $root 'ProjectSettings/EditorBuildSettings.asset'
$buildSettings = Get-Content -LiteralPath $buildSettingsPath -Raw
$sceneGuids = @{}

foreach ($sceneName in $sceneNames) {
    $scenePath = Join-Path $root "Assets/Scenes/$sceneName.unity"
    $metaPath = "$scenePath.meta"
    $buildPath = "Assets/Scenes/$sceneName.unity"

    Assert-True (Test-Path -LiteralPath $scenePath) "Missing scene file: $buildPath"
    Assert-True (Test-Path -LiteralPath $metaPath) "Missing scene meta file: $buildPath.meta"

    $guid = Get-MetaGuid $metaPath
    Assert-True (-not $sceneGuids.ContainsKey($guid)) "Duplicate Unity scene guid: $guid"
    $sceneGuids[$guid] = $sceneName

    Assert-True ($buildSettings -match [regex]::Escape("path: $buildPath")) "Build Settings missing $buildPath"
    Assert-True ($buildSettings -match [regex]::Escape("guid: $guid")) "Build Settings missing guid $guid for $buildPath"

    $sceneText = Get-Content -LiteralPath $scenePath -Raw
    foreach ($objectName in @('StageRoot', 'PlayerStart', 'Clues', 'Doors', 'Triggers')) {
        Assert-True ($sceneText -match "m_Name:\s+$objectName(\r?\n|$)") "$buildPath missing GameObject $objectName"
    }
}

$corridorScenePath = Join-Path $root 'Assets/Scenes/Scene_Corridor.unity'
$corridorScene = Get-Content -LiteralPath $corridorScenePath -Raw
Assert-True ($corridorScene -match 'm_Name:\s+SceneLoader(\r?\n|$)') 'Scene_Corridor.unity missing SceneLoader GameObject'
Assert-True ($corridorScene -match 'm_Script: \{fileID: 11500000, guid: f806e1d33cfe47479f449022b037a9ca, type: 3\}') 'Scene_Corridor.unity missing SceneLoader component'

$sceneLoaderPath = Join-Path $root 'Assets/_Shared/Scripts/SceneLoader.cs'
Assert-True (Test-Path -LiteralPath $sceneLoaderPath) 'Missing SceneLoader script at Assets/_Shared/Scripts/SceneLoader.cs'
$sceneLoader = Get-Content -LiteralPath $sceneLoaderPath -Raw

$expectedConstants = @{
    CorridorScene = 'Scene_Corridor'
    DressingRoomScene = 'Scene_DressingRoom'
    OperatingRoomScene = 'Scene_OperatingRoom'
}

foreach ($constantName in $expectedConstants.Keys) {
    $sceneName = $expectedConstants[$constantName]
    $pattern = "public\s+const\s+string\s+$constantName\s*=\s*`"$sceneName`"\s*;"
    Assert-True ($sceneLoader -match $pattern) "SceneLoader missing public const string $constantName = `"$sceneName`";"
}

foreach ($methodName in @('LoadCorridor', 'LoadDressingRoom', 'LoadOperatingRoom', 'LoadSceneByName', 'ReloadCurrentScene')) {
    Assert-True ($sceneLoader -match "public\s+void\s+$methodName\s*\(") "SceneLoader missing public method $methodName"
}

Write-Host 'SceneLoader repository checks passed.'
