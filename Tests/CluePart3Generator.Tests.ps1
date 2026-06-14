$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$generatorPath = Join-Path $root 'Assets/Room02_Operating/Clues/Editor/ClueAssetGenerator.cs'
$legacyNormalPath = Join-Path $root 'Assets/Clues/Normal'
$legacyKeyPath = Join-Path $root 'Assets/Clues/KeyClue'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function U {
    param([int[]] $CodePoints)
    return -join ($CodePoints | ForEach-Object { [char] $_ })
}

Assert-True (Test-Path -LiteralPath $generatorPath) 'Missing Room02 ClueAssetGenerator.cs'

$generator = Get-Content -LiteralPath $generatorPath -Raw -Encoding UTF8
$normalIds = @(
    'paint_footprints',
    'makeup_diary',
    'mirror_message',
    'paint_toolbox',
    'under_table_space',
    'yoanna_relic',
    'nurse_log'
)
$keyIds = @(
    'key_hint_note',
    'key_hint_sticker',
    'key_hint_scratch'
)

Assert-True ($generator -match 'MenuItem\("Tools/Room02/Generate Clues Part3"\)') 'Generator must expose Tools > Room02 > Generate Clues Part3.'
Assert-True ($generator -match 'GenerateCluesPart3\s*\(') 'Generator must implement GenerateCluesPart3().'
Assert-True ($generator -match 'GetPart3Entries\s*\(') 'Generator must keep Part3 entries separate.'
Assert-True ($generator -match 'Assets/Room02_Operating/Clues/Normal' -and $generator -match 'Assets/Room02_Operating/Clues/KeyClue') 'Part3 generator must write normal and key clues under Room02.'
Assert-True (-not (Test-Path -LiteralPath $legacyNormalPath)) 'Room02 work must not recreate Assets/Clues/Normal.'
Assert-True (-not (Test-Path -LiteralPath $legacyKeyPath)) 'Room02 work must not recreate Assets/Clues/KeyClue.'

foreach ($id in $normalIds + $keyIds) {
    Assert-True ($generator -match "new\s+ClueEntry\s*\(\s*""$id""" ) "Part3 generator missing clueId: $id"
    Assert-True ($generator -match "\{entry\.clueID\}\.asset|entry\.clueID\s*\+\s*""\.asset""" ) 'Part3 generator must save files as clueId.asset.'
}

foreach ($id in $keyIds) {
    Assert-True ($generator -match "new\s+ClueEntry\s*\(\s*""$id""[\s\S]*?ClueCategory\.KeyClue,\s*isRequired:\s*true") "Part3 key clue must be KeyClue and required: $id"
}

$paintFootprintsText = U 0xBD84,0xC7A5,0xC2E4,0x0020,0xBC14,0xB2E5,0x0020,0xD770,0x0020,0xD398,0xC778,0xD2B8,0x0020,0xBC1C,0xC790,0xAD6D
$underTableText = U 0xC218,0xC220,0xB300,0x0020,0xC544,0xB798,0x0020,0xC131,0xC778,0x0020,0xD55C,0x0020,0xBA85
$coldPlaceText = U 0xC81C,0xC77C,0x0020,0xCC28,0xAC00,0xC6B4,0x0020,0xACF3
$keyAvailableText = U 0xD0C8,0xCD9C,0x0020,0xC5F4,0xC1E0,0x0020,0xD68D,0xB4DD,0x0020,0xAC00,0xB2A5

Assert-True ($generator.Contains($paintFootprintsText)) 'Part3 paint footprints description must match the requested evidence.'
Assert-True ($generator.Contains($underTableText)) 'Part3 under table space description must match the requested evidence.'
Assert-True ($generator.Contains($coldPlaceText)) 'Part3 key note must mention the coldest place.'
Assert-True ($generator.Contains($keyAvailableText)) 'Part3 scratch clue meaning must mention escape key acquisition.'
Assert-True ($generator -match 'new\s+ClueEntry\s*\(\s*"paint_footprints"[\s\S]*?ClueCategory\.General,\s*isRequired:\s*true') 'paint_footprints must be required.'
Assert-True ($generator -match 'new\s+ClueEntry\s*\(\s*"makeup_diary"[\s\S]*?ClueCategory\.General,\s*isRequired:\s*true') 'makeup_diary must be required.'
Assert-True ($generator -match 'new\s+ClueEntry\s*\(\s*"under_table_space"[\s\S]*?ClueCategory\.General,\s*isRequired:\s*true') 'under_table_space must be required.'

Write-Host 'Clue Part3 generator checks passed.'
