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

$playerStartMatch = [regex]::Match($scene, 'm_Name:\s*PlayerStart.*?--- !u!4 &\d+\s*\r?\nTransform:.*?m_LocalPosition:\s*\{x:\s*([-0-9.]+), y:\s*([-0-9.]+), z:\s*([-0-9.]+)\}', 'Singleline')
Assert-True $playerStartMatch.Success 'Scene must contain PlayerStart with a position.'
$startX = [double]$playerStartMatch.Groups[1].Value
$startZ = [double]$playerStartMatch.Groups[3].Value

$clueMatches = [regex]::Matches($scene, 'm_Name:\s+Clue_[^\r\n]+.*?--- !u!4 &\d+\s*\r?\nTransform:.*?m_LocalPosition:\s*\{x:\s*([-0-9.]+), y:\s*([-0-9.]+), z:\s*([-0-9.]+)\}', 'Singleline')
Assert-True ($clueMatches.Count -eq 31) "Expected 31 clue transforms, found $($clueMatches.Count)."

foreach ($match in $clueMatches) {
    $x = [double]$match.Groups[1].Value
    $z = [double]$match.Groups[3].Value
    $distance = [math]::Sqrt([math]::Pow($x - $startX, 2) + [math]::Pow($z - $startZ, 2))
    Assert-True ($distance -le 2.25) "Every clue must be placed within 2.25m of PlayerStart; found distance $distance at x=$x z=$z."
}

Assert-True ($scene -match 'Culprit_StartPosition') 'Scene must contain the culprit start-position object.'
$culpritPositionMatch = [regex]::Match($scene, 'Culprit_StartPosition.*?m_LocalPosition:\s*\{x:\s*([-0-9.]+), y:\s*([-0-9.]+), z:\s*([-0-9.]+)\}', 'Singleline')
if ($culpritPositionMatch.Success) {
    $culpritX = [double]$culpritPositionMatch.Groups[1].Value
    $culpritZ = [double]$culpritPositionMatch.Groups[3].Value
    $culpritDistance = [math]::Sqrt([math]::Pow($culpritX - $startX, 2) + [math]::Pow($culpritZ - $startZ, 2))
    Assert-True ($culpritDistance -le 2.25) "Culprit must be placed within 2.25m of PlayerStart; found distance $culpritDistance."
}

Write-Host 'Room02 start placement checks passed.'
