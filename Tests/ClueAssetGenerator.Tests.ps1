$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$clueDataPath = Join-Path $root 'Assets/Clues/ClueData.cs'
$generatorPath = Join-Path $root 'Assets/Clues/Editor/ClueAssetGenerator.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $clueDataPath) 'Missing ClueData.cs'
Assert-True (Test-Path -LiteralPath $generatorPath) 'Missing ClueAssetGenerator.cs'

$clueData = Get-Content -LiteralPath $clueDataPath -Raw -Encoding UTF8
$generator = Get-Content -LiteralPath $generatorPath -Raw -Encoding UTF8

Assert-True ($clueData -match 'bool\s+isRequired\s*;') 'ClueData must include bool isRequired.'
Assert-True ($clueData -match 'string\s+areaName\s*;') 'ClueData must keep zone/areaName.'

Assert-True ($generator -match 'AssetDatabase\.CreateAsset') 'Generator must create ScriptableObject assets with AssetDatabase.CreateAsset.'
Assert-True ($generator -match 'Assets/Clues/Normal') 'Generator must create normal clue assets under Assets/Clues/Normal.'
Assert-True ($generator -match 'Assets/Clues/KeyClue') 'Generator must create key clue assets under Assets/Clues/KeyClue.'
Assert-True ($generator -match 'MenuItem\("Tools/Clues/Generate Story Clue Assets"\)') 'Generator must expose the requested editor menu.'
Assert-True ($generator -notmatch 'new\s+GameObject') 'Generator must not create scene objects.'

$entries = [regex]::Matches($generator, 'new\s+ClueEntry\s*\(')
$normalEntries = [regex]::Matches($generator, 'ClueCategory\.General,\s*isRequired:')
$keyEntries = [regex]::Matches($generator, 'ClueCategory\.KeyClue,\s*isRequired:')
$requiredTrue = [regex]::Matches($generator, 'isRequired:\s*true')
$normalFileNames = [regex]::Matches($generator, '"Clue_[^"]+"')
$keyFileNames = [regex]::Matches($generator, '"KeyClue_[^"]+"')

Assert-True ($entries.Count -eq 31) "Generator must define 31 clue entries, found $($entries.Count)."
Assert-True ($normalEntries.Count -eq 28) "Generator must define 28 normal clues, found $($normalEntries.Count)."
Assert-True ($keyEntries.Count -eq 3) "Generator must define 3 key clues, found $($keyEntries.Count)."
Assert-True ($requiredTrue.Count -eq 8) "Generator must mark 8 required clues, found $($requiredTrue.Count)."
Assert-True ($normalFileNames.Count -ge 28) 'Generator must include Clue_ filenames for normal clues.'
Assert-True ($keyFileNames.Count -ge 3) 'Generator must include KeyClue_ filenames for key clues.'

foreach ($zone in @('Lobby', 'Hallway', 'Ward', 'Storage', 'DressingRoom', 'OperatingRoom')) {
    Assert-True ($generator.Contains($zone)) "Generator missing zone: $zone"
}

foreach ($id in @(
    'normal_cast_notice',
    'normal_memorial_frame',
    'normal_security_log',
    'normal_production_plan',
    'clue_hasho_will',
    'normal_jin_sneakers',
    'normal_paint_footprints',
    'clue_makeup_diary',
    'normal_under_table_space',
    'key_clue_coldest_place',
    'key_clue_temperature_warning',
    'key_clue_fridge_scratches'
)) {
    Assert-True ($generator.Contains($id)) "Generator missing clue ID: $id"
}

Write-Host 'Clue asset generator checks passed.'
