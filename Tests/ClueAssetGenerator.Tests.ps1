$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$clueDataPath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueData.cs'
$generatorPath = Join-Path $root 'Assets/Room02_Operating/Clues/Editor/ClueAssetGenerator.cs'
$legacyPathText = 'Assets/' + 'Clues'
$legacyCluesPath = Join-Path $root $legacyPathText

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $clueDataPath) 'Missing ClueData.cs'
Assert-True (Test-Path -LiteralPath $generatorPath) 'Missing ClueAssetGenerator.cs'
Assert-True (-not (Test-Path -LiteralPath $legacyCluesPath)) 'Room02 clue files must not be created under the legacy clue folder.'

$clueData = Get-Content -LiteralPath $clueDataPath -Raw -Encoding UTF8
$generator = Get-Content -LiteralPath $generatorPath -Raw -Encoding UTF8

Assert-True ($clueData -match 'bool\s+isRequired\s*;') 'ClueData must include bool isRequired.'
Assert-True ($clueData -match 'string\s+areaName\s*;') 'ClueData must keep zone/areaName.'

Assert-True ($generator -match 'AssetDatabase\.CreateAsset') 'Generator must create ScriptableObject assets with AssetDatabase.CreateAsset.'
Assert-True ($generator -match 'Assets/Room02_Operating/Clues/Normal') 'Generator must create normal clue assets under Assets/Room02_Operating/Clues/Normal.'
Assert-True ($generator -match 'Assets/Room02_Operating/Clues/KeyClue') 'Generator must create key clue assets under Assets/Room02_Operating/Clues/KeyClue.'
Assert-True ($generator -notmatch $legacyPathText -and $generator -notmatch 'CreateFolder\("Assets",\s*"Clues"\)') 'Generator must not recreate legacy clue folders.'
Assert-True ($generator -match 'MenuItem\("Tools/Room02/Clues/Generate Story Clue Assets"\)') 'Generator must expose the requested editor menu.'
Assert-True ($generator -notmatch 'new\s+GameObject') 'Generator must not create scene objects.'

$mainEntriesMatch = [regex]::Match($generator, 'CurrentStoryEntries\s*=\s*new\[\]\s*\{(?<body>[\s\S]*?)\};', 'Singleline')
Assert-True ($mainEntriesMatch.Success) 'Generator must keep the 15-clue story entries separate from legacy part entries.'
$mainEntriesSource = $mainEntriesMatch.Value

$entries = [regex]::Matches($mainEntriesSource, 'new\s+ClueEntry\s*\(')
$normalEntries = [regex]::Matches($mainEntriesSource, 'ClueCategory\.General,\s*isRequired:')
$keyEntries = [regex]::Matches($mainEntriesSource, 'ClueCategory\.KeyClue,\s*isRequired:')
$requiredTrue = [regex]::Matches($mainEntriesSource, 'isRequired:\s*true')
$normalFileNames = [regex]::Matches($mainEntriesSource, '"Clue_[^"]+"')
$keyFileNames = [regex]::Matches($mainEntriesSource, '"KeyClue_[^"]+"')

Assert-True ($entries.Count -eq 15) "Generator must define 15 clue entries, found $($entries.Count)."
Assert-True ($normalEntries.Count -eq 12) "Generator must define 12 normal clues, found $($normalEntries.Count)."
Assert-True ($keyEntries.Count -eq 3) "Generator must define 3 key clues, found $($keyEntries.Count)."
Assert-True ($requiredTrue.Count -eq 15) "Generator must mark all 15 story clues required, found $($requiredTrue.Count)."
Assert-True ($normalFileNames.Count -ge 12) 'Generator must include Clue_ filenames for normal clues.'
Assert-True ($keyFileNames.Count -ge 3) 'Generator must include KeyClue_ filenames for key clues.'

foreach ($zone in @('Hallway', 'Ward', 'Storage', 'DressingRoom', 'OperatingRoom')) {
    Assert-True ($generator.Contains($zone)) "Generator missing zone: $zone"
}

foreach ($id in @(
    'normal_cast_notice',
    'normal_memorial_frame',
    'normal_conversation_memo',
    'normal_medical_certificate',
    'normal_ward_calendar',
    'clue_hasho_will',
    'normal_bong_rebuttal',
    'normal_makeup_toolbox',
    'normal_sumi_memo',
    'clue_makeup_diary',
    'normal_under_table_space',
    'normal_mirror_message',
    'key_clue_coldest_place',
    'key_clue_temperature_warning',
    'key_clue_fridge_scratches'
)) {
    Assert-True ($generator.Contains($id)) "Generator missing clue ID: $id"
}

Write-Host 'Clue asset generator checks passed.'
