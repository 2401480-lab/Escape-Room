$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$generatorPath = Join-Path $root 'Assets/Room02_Operating/Clues/Editor/ClueAssetGenerator.cs'
$normalPath = Join-Path $root 'Assets/Room02_Operating/Clues/Normal'
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

$part1Ids = @(
    'cast_notice',
    'memorial_frame',
    'security_log',
    'event_plan',
    'torn_letter_a',
    'torn_letter_b',
    'yoanna_note',
    'cctv_memo',
    'sumi_memo'
)

Assert-True ($generator -match 'MenuItem\("Tools/Room02/Generate Clues Part1"\)') 'Generator must expose Tools > Room02 > Generate Clues Part1.'
Assert-True ($generator -match 'GenerateCluesPart1\s*\(') 'Generator must implement GenerateCluesPart1().'
Assert-True ($generator -match 'ScriptableObject\.CreateInstance<ClueData>\s*\(') 'Generator must create ClueData ScriptableObject assets.'
Assert-True ($generator -match 'AssetDatabase\.CreateAsset\s*\(') 'Generator must use AssetDatabase.CreateAsset.'
Assert-True ($generator -match 'EditorUtility\.SetDirty\s*\(' -and $generator -match 'AssetDatabase\.SaveAssets\s*\(') 'Generator must overwrite/update existing assets and save changes.'
Assert-True ($generator -match 'Assets/Room02_Operating/Clues/Normal') 'Room02 generator must write normal clues under Room02.'
Assert-True ($generator -notmatch 'private\s+class\s+ClueData|class\s+ClueData\s*:') 'Generator must not define a new ClueData class.'
Assert-True (-not (Test-Path -LiteralPath $legacyPath)) 'Room02 work must not recreate Assets/Clues/Normal.'

foreach ($id in $part1Ids) {
    Assert-True ($generator -match "new\s+ClueEntry\s*\(\s*""$id""" ) "Part1 generator missing clueId: $id"
    Assert-True ($generator -match "\{entry\.clueID\}\.asset|entry\.clueID\s*\+\s*""\.asset""" ) 'Part1 generator must save files as clueId.asset.'
}

$castNoticeText = U 0xBC30,0xC5ED,0x0020,0xC548,0xB0B4,0xBB38
$jinPlannedText = U 0xACF5,0xC5F0,0x0020,0xC790,0xCCB4,0xB97C,0x0020,0xC9C4,0xC138,0xC6C5,0xC774,0x0020,0xAE30,0xD68D,0xD588,0xB2E4
$tornLetterBText = U 0xCC22,0xAE34,0x0020,0xD3B8,0xC9C0,0x0020,0xC870,0xAC01,0x0020,0x0042
$sumiMemoText = U 0xBB38,0xC218,0xBBF8,0xC758,0x0020,0xBA54,0xBAA8
$longPlanText = U 0xC7A5,0xAE30,0x0020,0xACC4,0xD68D,0xC758,0x0020,0xBCF5,0xC120

Assert-True ($generator.Contains($castNoticeText) -and $generator.Contains($jinPlannedText)) 'Part1 cast notice text must match the requested scenario.'
Assert-True ($generator.Contains($tornLetterBText) -and $generator -match 'ClueCategory\.General,\s*isRequired:\s*true') 'Part1 must mark required clues correctly.'
Assert-True ($generator.Contains($sumiMemoText) -and $generator.Contains($longPlanText)) 'Part1 sumi memo text must match the requested scenario.'

Write-Host 'Clue Part1 generator checks passed.'
