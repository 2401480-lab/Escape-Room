$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$generatorPath = Join-Path $root 'Assets/Room02_Operating/Scripts/Editor/ClueDataGenerator.cs'
$managerPath = Join-Path $root 'Assets/Room02_Operating/Scripts/Core/RoomGameManager.cs'
$setupToolPath = Join-Path $root 'Assets/Room02_Operating/Scripts/Editor/SceneSetupTool.cs'

function Assert-True {
    param(
        [bool] $Condition,
        [string] $Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

Assert-True (Test-Path -LiteralPath $generatorPath) 'Missing ClueDataGenerator.cs'
Assert-True (Test-Path -LiteralPath $managerPath) 'Missing RoomGameManager.cs'
Assert-True (Test-Path -LiteralPath $setupToolPath) 'Missing SceneSetupTool.cs'

$generator = Get-Content -LiteralPath $generatorPath -Raw -Encoding UTF8
$manager = Get-Content -LiteralPath $managerPath -Raw -Encoding UTF8
$setupTool = Get-Content -LiteralPath $setupToolPath -Raw -Encoding UTF8

$expectedClues = @(
    'clue_cast_notice',
    'clue_memorial_frame',
    'clue_visitor_log',
    'clue_security_log',
    'clue_torn_letter_piece_a',
    'clue_torn_letter_piece_b',
    'clue_yoanna_note',
    'clue_nurse_log',
    'clue_cctv_memo',
    'clue_phone_memo',
    'clue_hasho_will',
    'clue_medical_certificate',
    'clue_conversation_memo_a',
    'clue_isolation_bloodstain',
    'clue_sumi_memo',
    'clue_bong_rebuttal',
    'clue_ward_calendar',
    'clue_poison_ampoule',
    'clue_hidden_camera',
    'clue_jin_sneakers',
    'clue_gloves',
    'clue_locked_locker',
    'clue_paint_footprints',
    'clue_makeup_diary',
    'clue_mirror_message',
    'clue_paint_toolbox',
    'clue_under_table_space',
    'clue_yoanna_relic'
)

$generatorIDs = [regex]::Matches($generator, 'new\s+ClueEntry\s*\(\s*"([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
$setupIDs = [regex]::Matches($setupTool, 'CreateClueObject\s*\([^;]+,\s*"([^"]+)"\s*,') | ForEach-Object { $_.Groups[1].Value }

Assert-True ($generatorIDs.Count -eq 28) "ClueDataGenerator must define 28 clues, found $($generatorIDs.Count)."
Assert-True (($generatorIDs | Select-Object -Unique).Count -eq 28) 'ClueDataGenerator must not contain duplicate clue IDs.'

foreach ($id in $expectedClues) {
    Assert-True ($generatorIDs -contains $id) "ClueDataGenerator is missing clue ID: $id"
    Assert-True ($setupIDs -contains $id) "SceneSetupTool must place clue object for: $id"
    Assert-True ($manager -match [regex]::Escape($id)) "RoomGameManager progression must reference clue ID: $id"
}

Assert-True ($manager -match 'MisdirectionPhase') 'RoomGameManager must include a misdirection phase for Bong Taehyeon suspicion.'
Assert-True ($manager -match 'MotivePhase') 'RoomGameManager must include a motive phase for Jin Sewoong.'
Assert-True ($manager -match 'EvidencePhase') 'RoomGameManager must include an evidence phase before final selection.'
Assert-True ($manager -match 'clue_medical_certificate' -and $manager -match 'clue_bong_rebuttal') 'RoomGameManager must connect Bong Taehyeon misdirection clues.'
Assert-True ($manager -match 'clue_torn_letter_piece_a' -and $manager -match 'clue_torn_letter_piece_b' -and $manager -match 'clue_makeup_diary') 'RoomGameManager must connect Jin Sewoong motive clues.'
Assert-True ($manager -match 'clue_poison_ampoule' -and $manager -match 'clue_jin_sneakers' -and $manager -match 'clue_paint_footprints' -and $manager -match 'clue_under_table_space') 'RoomGameManager must connect physical evidence clues.'

Assert-True ($setupTool -match 'SetupLobbyClues') 'SceneSetupTool must group lobby clues.'
Assert-True ($setupTool -match 'SetupWardClues') 'SceneSetupTool must group ward clues.'
Assert-True ($setupTool -match 'SetupStorageClues') 'SceneSetupTool must group storage clues.'
Assert-True ($setupTool -match 'SetupMakeupClues') 'SceneSetupTool must group makeup room clues.'
Assert-True ($setupTool -match 'SetupOperatingZoneClues') 'SceneSetupTool must group operating room clues.'

Write-Host 'Room02 expanded clue checks passed.'
