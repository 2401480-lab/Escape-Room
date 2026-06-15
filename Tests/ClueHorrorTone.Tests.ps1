$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$generatorPath = Join-Path $root 'Assets/Room02_Operating/Clues/Editor/ClueAssetGenerator.cs'
$popupPath = Join-Path $root 'Assets/Room02_Operating/Clues/CluePickupPopupUI.cs'
$normalCluePath = Join-Path $root 'Assets/Room02_Operating/Clues/Normal'
$keyCluePath = Join-Path $root 'Assets/Room02_Operating/Clues/KeyClue'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function U {
    param([int[]] $CodePoints)
    return -join ($CodePoints | ForEach-Object { [char]$_ })
}

Assert-True (Test-Path -LiteralPath $generatorPath) 'Missing ClueAssetGenerator.cs'
Assert-True (Test-Path -LiteralPath $popupPath) 'Missing CluePickupPopupUI.cs'
Assert-True (Test-Path -LiteralPath $normalCluePath) 'Missing normal clue assets.'
Assert-True (Test-Path -LiteralPath $keyCluePath) 'Missing key clue assets.'

$generator = Get-Content -LiteralPath $generatorPath -Raw -Encoding UTF8
$popup = Get-Content -LiteralPath $popupPath -Raw -Encoding UTF8
$generatorDecoded = [regex]::Unescape($generator)
$popupDecoded = [regex]::Unescape($popup)

Assert-True ($popup -notmatch '利앷굅|뺣낫|諛|譚|꾩|덈떎') 'Clue pickup popup text must not contain mojibake.'
Assert-True ($popupDecoded.Contains((U 49552,50640,32,45823,50520,45796)) -and $popupDecoded.Contains((U 45224,51008,32,55124,51201))) 'Clue pickup popup must use atmospheric horror labels.'

$horrorWords = @(
    (U 52264,44049),
    (U 54588),
    (U 49704),
    (U 46504),
    (U 50620,47337),
    (U 45252,49352),
    (U 44545),
    (U 49549,49325),
    (U 48708,47749),
    (U 50612,46176),
    (U 52629,52629),
    (U 49548,47492),
    (U 52285,48177),
    (U 45216,52852),
    (U 49436,45720),
    (U 47785,45916,48120)
)

$generatorHits = 0
foreach ($word in $horrorWords) {
    $generatorHits += ([regex]::Matches($generatorDecoded, [regex]::Escape($word))).Count
}

Assert-True ($generatorHits -ge 25) "Clue generator should carry a stronger horror tone; found only $generatorHits horror-word hits."

$assetFiles = @(Get-ChildItem -LiteralPath $normalCluePath -Filter '*.asset' -File) + @(Get-ChildItem -LiteralPath $keyCluePath -Filter '*.asset' -File)
Assert-True ($assetFiles.Count -eq 31) "Expected 31 clue assets, found $($assetFiles.Count)."

foreach ($assetFile in $assetFiles) {
    $assetText = Get-Content -LiteralPath $assetFile.FullName -Raw -Encoding UTF8
    $assetDecoded = [regex]::Unescape($assetText)
    $hasAtmosphere = $false
    foreach ($word in $horrorWords) {
        if ($assetDecoded.Contains($word)) {
            $hasAtmosphere = $true
            break
        }
    }

    Assert-True $hasAtmosphere "Clue asset needs a more horror-like hint: $($assetFile.Name)"
}

Write-Host 'Clue horror tone checks passed.'
