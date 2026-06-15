$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$introPath = Join-Path $root 'Assets/Room02_Operating/Clues/IntroScenarioUI.cs'
$generatorPath = Join-Path $root 'Assets/Room02_Operating/Clues/Editor/ClueAssetGenerator.cs'
$scenePath = Join-Path $root 'Assets/Room02_Operating/Scenes/Show.unity'
$journalUiPath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueJournalUI.cs'
$storyManagerPath = Join-Path $root 'Assets/Room02_Operating/Clues/StoryProgressManager.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function U {
    param([int[]] $CodePoints)
    return -join ($CodePoints | ForEach-Object { [char] $_ })
}

function Get-EntryBlock {
    param([string] $Source, [string] $ClueID)
    $match = [regex]::Match($Source, "new\s+ClueEntry\s*\(\s*""$([regex]::Escape($ClueID))""[\s\S]*?\),", 'Singleline')
    Assert-True $match.Success "Missing clue entry block: $ClueID"
    return $match.Value
}

foreach ($path in @($introPath, $generatorPath, $scenePath, $journalUiPath, $storyManagerPath)) {
    Assert-True (Test-Path -LiteralPath $path) "Missing story file: $path"
}

$intro = Get-Content -LiteralPath $introPath -Raw -Encoding UTF8
$generator = Get-Content -LiteralPath $generatorPath -Raw -Encoding UTF8
$scene = Get-Content -LiteralPath $scenePath -Raw -Encoding UTF8
$journalUi = Get-Content -LiteralPath $journalUiPath -Raw -Encoding UTF8
$storyManager = Get-Content -LiteralPath $storyManagerPath -Raw -Encoding UTF8

$introLine1 = U 0xB208,0xC744,0x0020,0xB5A0,0xBCF4,0xB2C8,0x0020,0xCC28,0xAC00,0xC6B4,0x0020,0xBCD1,0xC6D0,0x0020,0xBCF5,0xB3C4,0xC600,0xB2E4,0x002E
$introLine2 = U 0xBB38,0xC740,0x0020,0xC7A0,0xACA8,0x0020,0xC788,0xACE0,0x002C,0x0020,0xBD88,0xBE5B,0xC740,0x0020,0xBD88,0xC548,0xD558,0xAC8C,0x0020,0xAE5C,0xBE61,0xC778,0xB2E4,0x002E,0x0020,0xB098,0xB294,0x0020,0xAC07,0xD614,0xB2E4,0x002E
$introLine3 = U 0xD0C8,0xCD9C,0xAD6C,0x0020,0xC5F4,0xC1E0,0xB294,0x0020,0xC774,0x0020,0xC548,0x0020,0xC5B4,0xB518,0xAC00,0xC5D0,0x0020,0xC788,0xB2E4,0x002E,0x0020,0xB2E8,0xC11C,0xB97C,0x0020,0xCC3E,0xC544,0xB77C,0x002E,0x0020,0xBC94,0xC778,0xC744,0x0020,0xBC1D,0xD600,0xB77C,0x002E,0x0020,0xADF8,0xB9AC,0xACE0,0x0020,0x2014,0x0020,0x0032,0x0030,0xBD84,0x0020,0xC548,0xC5D0,0x0020,0xC5EC,0xAE30,0xC11C,0x0020,0xB098,0xAC00,0xB77C,0x002E
foreach ($line in @($introLine1, $introLine2, $introLine3)) {
    Assert-True ($intro.Contains($line)) "Intro narration missing: $line"
}

$storyEntriesMatch = [regex]::Match($generator, 'CurrentStoryEntries\s*=\s*new\[\]\s*\{(?<body>[\s\S]*?)\};', 'Singleline')
Assert-True ($storyEntriesMatch.Success) 'ClueAssetGenerator must define CurrentStoryEntries for the 15-clue story.'
$storyEntries = $storyEntriesMatch.Groups['body'].Value
$entryCount = ([regex]::Matches($storyEntries, 'new\s+ClueEntry\s*\(')).Count
Assert-True ($entryCount -eq 15) "Story must use exactly 15 clues, found $entryCount."

foreach ($id in @(
    'normal_cast_notice',
    'normal_memorial_frame',
    'normal_conversation_memo',
    'normal_medical_certificate',
    'normal_ward_calendar',
    'clue_hasho_will',
    'key_clue_coldest_place',
    'key_clue_temperature_warning',
    'normal_bong_rebuttal',
    'key_clue_fridge_scratches',
    'normal_makeup_toolbox',
    'normal_sumi_memo',
    'clue_makeup_diary',
    'normal_under_table_space',
    'normal_mirror_message'
)) {
    Assert-True ($storyEntries.Contains($id)) "15-clue story missing clue ID: $id"
}

foreach ($removedID in @(
    'normal_security_log',
    'normal_production_plan',
    'normal_cctv_notice',
    'normal_deleted_entry_trace',
    'normal_poison_ampoule',
    'normal_hidden_camera',
    'normal_paint_footprints',
    'normal_nurse_inventory_log'
)) {
    Assert-True (-not $storyEntries.Contains($removedID)) "15-clue story must remove extra clue ID: $removedID"
}

$notebookPrefix = U 0xC218,0xCCA9,0x003A
$bong = U 0xBD09,0xD0DC,0xD604
$jin = U 0xC9C4,0xC138,0xC6C5
$common = U 0xACF5,0xD1B5
$hintHeader = U 0xC218,0xC9D1,0xB41C,0x0020,0xB2E8,0xC11C,0x0020,0xD78C,0xD2B8
$emptyHint = U 0xC544,0xC9C1,0x0020,0xD655,0xC778,0xB41C,0x0020,0xD78C,0xD2B8,0x0020,0xC5C6,0xC74C
$moon = U 0xBB38,0xC218,0xBBF8
$caseCommon = U 0xC0AC,0xAC74,0x0020,0xACF5,0xD1B5

$castNotice = Get-EntryBlock $storyEntries 'normal_cast_notice'
$underTable = Get-EntryBlock $storyEntries 'normal_under_table_space'
$fridgeInside = Get-EntryBlock $storyEntries 'key_clue_fridge_scratches'
Assert-True ($castNotice.Contains($notebookPrefix) -and $castNotice.Contains($bong) -and $castNotice.Contains($jin)) 'Cast notice must update Bong/Jin suspect notebook.'
Assert-True ($underTable.Contains($notebookPrefix) -and $underTable.Contains($jin)) 'Under-table clue must update Jin suspect notebook.'
Assert-True ($fridgeInside.Contains($notebookPrefix) -and $fridgeInside.Contains($common)) 'Fridge-inside clue must update common notebook with escape key acquisition.'

$sceneClueCount = ([regex]::Matches($scene, 'm_Name:\s+Clue_')).Count
$wiredClueCount = ([regex]::Matches($scene, 'clueData:\s*\{fileID:\s*11400000')).Count
Assert-True ($sceneClueCount -eq 15) "Show scene must contain exactly 15 clue boxes, found $sceneClueCount."
Assert-True ($wiredClueCount -eq 15) "Show scene must wire exactly 15 clue data refs, found $wiredClueCount."

Assert-True ($journalUi -match 'BuildSuspectCards\s*\(\s*\)' -and $journalUi -match 'GetCollectedHintsForPerson') 'Suspect notebook must rebuild from collected clue hints.'
Assert-True ($journalUi.Contains($hintHeader) -and $journalUi.Contains($emptyHint)) 'Suspect notebook must show collected hints and empty state.'
Assert-True ($journalUi.Contains($jin) -and $journalUi.Contains($bong) -and $journalUi.Contains($moon) -and $journalUi.Contains($caseCommon)) 'Suspect notebook must include suspects and common case notes.'

Assert-True ($storyManager -match 'requiredClueCount\s*=\s*10') 'StoryProgressManager must unlock suspect selection after 10 collected clues.'
Assert-True ($storyManager -match 'HasAllStoryClues') 'StoryProgressManager must expose all-story-clues completion.'
Assert-True ($storyManager -match 'collectedClueIDs\.Count\s*>=\s*requiredClueCount') 'StoryProgressManager must check collected clue count for suspect selection.'
Assert-True ($storyManager -notmatch 'hasEscapeKey\s*&&\s*HasAllStoryClues') 'Suspect selection must not require the escape key once 10 clues are collected.'
Assert-True ($storyManager -match 'CanSelectSuspect') 'StoryProgressManager must name the 10-clue suspect selection readiness explicitly.'

Write-Host 'Room02 15-clue story checks passed.'
