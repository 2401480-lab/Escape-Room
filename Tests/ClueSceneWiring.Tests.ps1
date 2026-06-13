$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$assetGeneratorPath = Join-Path $root 'Assets/Clues/Editor/ClueAssetGenerator.cs'
$sceneSetupPath = Join-Path $root 'Assets/Clues/Editor/ClueSceneSetupTool.cs'
$interactablePath = Join-Path $root 'Assets/Clues/ClueInteractable.cs'
$managerPath = Join-Path $root 'Assets/Clues/ClueJournalManager.cs'
$popupMetaPath = Join-Path $root 'Assets/Clues/CluePickupPopupUI.cs.meta'
$sceneSetupMetaPath = Join-Path $root 'Assets/Clues/Editor/ClueSceneSetupTool.cs.meta'
$normalCluePath = Join-Path $root 'Assets/Clues/Normal'
$keyCluePath = Join-Path $root 'Assets/Clues/KeyClue'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $assetGeneratorPath) 'Missing ClueAssetGenerator.cs'
Assert-True (Test-Path -LiteralPath $sceneSetupPath) 'Missing ClueSceneSetupTool.cs'
Assert-True (Test-Path -LiteralPath $interactablePath) 'Missing ClueInteractable.cs'
Assert-True (Test-Path -LiteralPath $managerPath) 'Missing ClueJournalManager.cs'
Assert-True (Test-Path -LiteralPath $popupMetaPath) 'Missing UI script meta files; Unity scene/script references need committed .meta GUIDs.'
Assert-True (Test-Path -LiteralPath $sceneSetupMetaPath) 'Missing ClueSceneSetupTool.cs.meta; Unity editor scripts need committed .meta GUIDs.'
Assert-True (Test-Path -LiteralPath $normalCluePath) 'Missing generated normal clue asset folder.'
Assert-True (Test-Path -LiteralPath $keyCluePath) 'Missing generated key clue asset folder.'

$assetGenerator = Get-Content -LiteralPath $assetGeneratorPath -Raw -Encoding UTF8
$sceneSetup = Get-Content -LiteralPath $sceneSetupPath -Raw -Encoding UTF8
$interactable = Get-Content -LiteralPath $interactablePath -Raw -Encoding UTF8
$manager = Get-Content -LiteralPath $managerPath -Raw -Encoding UTF8

Assert-True ($assetGenerator -match 'GetEntries\s*\(\s*\)') 'ClueAssetGenerator must expose clue entries to the scene setup tool.'
Assert-True ($assetGenerator -match 'internal\s+readonly\s+struct\s+ClueEntry') 'Clue entries must be shareable with editor setup.'
Assert-True ($sceneSetup -match 'MenuItem\("Tools/Clues/Setup Current Stage Clues"\)') 'Scene setup tool must expose a current-scene clue setup menu.'
Assert-True ($sceneSetup -notmatch 'SetupAllStageClues' -and $sceneSetup -notmatch 'EditorSceneManager') 'Scene setup must not switch or save scenes automatically.'
Assert-True ($sceneSetup -match 'GenerateStoryClueAssets\s*\(') 'Scene setup must generate missing ClueData assets before wiring scene objects.'
Assert-True ($sceneSetup -match 'Scene_OperatingRoom') 'Scene setup must support the integrated stage scene.'
Assert-True ($sceneSetup -notmatch 'case\s+"Scene_Corridor"' -and $sceneSetup -notmatch 'case\s+"Scene_DressingRoom"') 'Scene setup must not target deleted split scenes.'
Assert-True ($sceneSetup -match 'AddComponent<ClueInteractable>\s*\(') 'Scene setup must place EscapeRoom.ClueInteractable components.'
Assert-True ($sceneSetup -match 'AddComponent<BoxCollider>\s*\(' -or $sceneSetup -match 'CreatePrimitive') 'Scene setup must give clues a collider so range checks have visible objects.'
Assert-True ($sceneSetup -match 'AddComponent<MeshRenderer>\s*\(' -or $sceneSetup -match 'CreatePrimitive') 'Scene setup must create visible clue markers.'
Assert-True ($sceneSetup -match 'SerializedObject' -and $sceneSetup -match '"clueData"') 'Scene setup must assign ClueData asset references to ClueInteractable.'
Assert-True ($sceneSetup -match 'ClueJournalManager' -and $sceneSetup -match 'ClueJournalUI' -and $sceneSetup -match 'CluePickupPopupUI') 'Scene setup must ensure clue manager, journal UI, and pickup popup exist.'
Assert-True ($interactable -match 'RegisterClueDefinition') 'ClueInteractable must register its ClueData so the journal can show available clues.'
Assert-True ($manager -notmatch 'DontDestroyOnLoad') 'ClueJournalManager must not use DontDestroyOnLoad in the single-scene flow.'

$normalAssets = Get-ChildItem -LiteralPath $normalCluePath -Filter '*.asset' -File
$keyAssets = Get-ChildItem -LiteralPath $keyCluePath -Filter '*.asset' -File
Assert-True ($normalAssets.Count -eq 28) "Expected 28 generated normal clue assets, found $($normalAssets.Count)."
Assert-True ($keyAssets.Count -eq 3) "Expected 3 generated key clue assets, found $($keyAssets.Count)."

Write-Host 'Clue scene wiring checks passed.'
