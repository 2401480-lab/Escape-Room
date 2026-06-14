$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$generatorPath = Join-Path $root 'Assets/Room02_Operating/Clues/Editor/ClueAssetGenerator.cs'
$legacyPath = Join-Path $root 'Assets/Clues/Normal'

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
$part2Ids = @(
    'hasho_will',
    'medical_certificate',
    'conversation_memo',
    'bong_rebuttal',
    'ward_calendar',
    'oseojin_memo',
    'record_deletion',
    'poison_ampoule',
    'hidden_camera',
    'jin_sneakers',
    'gloves',
    'locked_locker'
)

Assert-True ($generator -match 'MenuItem\("Tools/Room02/Generate Clues Part2"\)') 'Generator must expose Tools > Room02 > Generate Clues Part2.'
Assert-True ($generator -match 'GenerateCluesPart2\s*\(') 'Generator must implement GenerateCluesPart2().'
Assert-True ($generator -match 'GetPart2Entries\s*\(') 'Generator must keep Part2 entries separate.'
Assert-True ($generator -match 'ScriptableObject\.CreateInstance<ClueData>\s*\(' -and $generator -match 'AssetDatabase\.CreateAsset\s*\(') 'Part2 generator must create ClueData ScriptableObject assets with AssetDatabase.'
Assert-True ($generator -match 'Assets/Room02_Operating/Clues/Normal') 'Room02 Part2 generator must write under Room02 normal clue folder.'
Assert-True (-not (Test-Path -LiteralPath $legacyPath)) 'Room02 work must not recreate Assets/Clues/Normal.'

foreach ($id in $part2Ids) {
    Assert-True ($generator -match "new\s+ClueEntry\s*\(\s*""$id""" ) "Part2 generator missing clueId: $id"
    Assert-True ($generator -match "\{entry\.clueID\}\.asset|entry\.clueID\s*\+\s*""\.asset""" ) 'Part2 generator must save files as clueId.asset.'
}

$willText = U 0xD558,0xC2DC,0xD638,0x0020,0xBCF8,0xC778,0x0020,0xD544,0xCCB4
$turningText = U 0xC2A4,0xD1A0,0xB9AC,0x0020,0xCD5C,0xB300,0x0020,0xC804,0xD658,0xC810
$paintText = U 0xBC11,0xCC3D,0x0020,0xD770,0x0020,0xD398,0xC778,0xD2B8
$lockerText = U 0xC218,0xC220,0xC2E4,0xC5D0,0xC11C,0x0020,0xC218,0xCC28,0xB840,0x0020,0xB9AC,0xD5C8,0xC124

Assert-True ($generator.Contains($willText) -and $generator.Contains($turningText)) 'Part2 hasho will text must match the requested story turn.'
Assert-True ($generator.Contains($paintText)) 'Part2 jin sneakers evidence text must match the requested physical evidence.'
Assert-True ($generator.Contains($lockerText)) 'Part2 locked locker meaning must match the requested rehearsal evidence.'
Assert-True ($generator -match 'new\s+ClueEntry\s*\(\s*"hasho_will"[\s\S]*?ClueCategory\.General,\s*isRequired:\s*true' ) 'hasho_will must be required.'
Assert-True ($generator -match 'new\s+ClueEntry\s*\(\s*"poison_ampoule"[\s\S]*?ClueCategory\.General,\s*isRequired:\s*true' ) 'poison_ampoule must be required.'
Assert-True ($generator -match 'new\s+ClueEntry\s*\(\s*"jin_sneakers"[\s\S]*?ClueCategory\.General,\s*isRequired:\s*true' ) 'jin_sneakers must be required.'

Write-Host 'Clue Part2 generator checks passed.'
