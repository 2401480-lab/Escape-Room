$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$scenePath = Join-Path $root 'Assets/Room02_Operating/Scenes/Scene_OperatingRoom.unity'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function Read-Vector3 {
    param([string] $Text)

    $match = [regex]::Match($Text, 'm_LocalPosition:\s*\{x:\s*([-0-9.]+), y:\s*([-0-9.]+), z:\s*([-0-9.]+)\}')
    Assert-True $match.Success "Missing vector in block: $Text"
    return [pscustomobject]@{
        X = [double]$match.Groups[1].Value
        Y = [double]$match.Groups[2].Value
        Z = [double]$match.Groups[3].Value
    }
}

Assert-True (Test-Path -LiteralPath $scenePath) 'Missing Room02 operating scene.'

$scene = Get-Content -LiteralPath $scenePath -Raw -Encoding UTF8

$clueMatches = [regex]::Matches($scene, 'm_Name:\s+Clue_[^\r\n]+.*?--- !u!4 &\d+\s*\r?\nTransform:.*?m_LocalPosition:\s*\{x:\s*([-0-9.]+), y:\s*([-0-9.]+), z:\s*([-0-9.]+)\}', 'Singleline')
Assert-True ($clueMatches.Count -eq 31) "Expected 31 clue transforms, found $($clueMatches.Count)."

$anchorX = [double]$clueMatches[0].Groups[1].Value
$anchorY = [double]$clueMatches[0].Groups[2].Value
$anchorZ = [double]$clueMatches[0].Groups[3].Value

foreach ($match in $clueMatches) {
    $x = [double]$match.Groups[1].Value
    $y = [double]$match.Groups[2].Value
    $z = [double]$match.Groups[3].Value
    Assert-True ([math]::Abs($x - $anchorX) -lt 0.001 -and [math]::Abs($y - $anchorY) -lt 0.001 -and [math]::Abs($z - $anchorZ) -lt 0.001) "Every clue must be stacked at the same visible box position; found x=$x y=$y z=$z, expected x=$anchorX y=$anchorY z=$anchorZ."
}

Assert-True ($scene -match 'Culprit_StartPosition') 'Scene must contain the culprit start-position object.'
$culpritPositionMatch = [regex]::Match($scene, 'Culprit_StartPosition.*?m_LocalPosition:\s*\{x:\s*([-0-9.]+), y:\s*([-0-9.]+), z:\s*([-0-9.]+)\}', 'Singleline')
if ($culpritPositionMatch.Success) {
    $culpritX = [double]$culpritPositionMatch.Groups[1].Value
    $culpritZ = [double]$culpritPositionMatch.Groups[3].Value
    Assert-True ([math]::Abs($culpritX - $anchorX) -lt 0.001 -and [math]::Abs($culpritZ - $anchorZ) -lt 0.001) "Culprit must share the visible box x/z position; found x=$culpritX z=$culpritZ, expected x=$anchorX z=$anchorZ."
}

Write-Host 'Room02 start placement checks passed.'
