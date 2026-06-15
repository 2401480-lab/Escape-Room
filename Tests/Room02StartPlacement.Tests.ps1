$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$scenePath = Join-Path $root 'Assets/Room02_Operating/Scenes/Show.unity'
$setupPath = Join-Path $root 'Assets/Room02_Operating/Clues/Editor/ClueSceneSetupTool.cs'

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

Assert-True (Test-Path -LiteralPath $scenePath) 'Missing Room02 Show gameplay scene.'
Assert-True (Test-Path -LiteralPath $setupPath) 'Missing Room02 clue setup tool.'

$scene = Get-Content -LiteralPath $scenePath -Raw -Encoding UTF8
$setup = Get-Content -LiteralPath $setupPath -Raw -Encoding UTF8

function Get-NamedPosition {
    param([string] $SceneText, [string] $ObjectName)

    $block = [regex]::Match($SceneText, "m_Name:\s+$([regex]::Escape($ObjectName)).*?--- !u!4 &\d+\s*\r?\nTransform:.*?m_LocalPosition:\s*\{x:\s*([-0-9.]+), y:\s*([-0-9.]+), z:\s*([-0-9.]+)\}", 'Singleline')
    Assert-True $block.Success "Missing transform position for $ObjectName."
    return [pscustomobject]@{
        X = [double]$block.Groups[1].Value
        Y = [double]$block.Groups[2].Value
        Z = [double]$block.Groups[3].Value
    }
}

$clueMatches = [regex]::Matches($scene, 'm_Name:\s+Clue_[^\r\n]+.*?--- !u!4 &\d+\s*\r?\nTransform:.*?m_LocalPosition:\s*\{x:\s*([-0-9.]+), y:\s*([-0-9.]+), z:\s*([-0-9.]+)\}', 'Singleline')
Assert-True ($clueMatches.Count -eq 31) "Expected 31 clue transforms, found $($clueMatches.Count)."

$camera = Get-NamedPosition $scene 'Main Camera'
$uniquePositions = @{}
$uniqueX = @{}
$uniqueY = @{}
$minX = [double]::PositiveInfinity
$maxX = [double]::NegativeInfinity

foreach ($match in $clueMatches) {
    $x = [double]$match.Groups[1].Value
    $y = [double]$match.Groups[2].Value
    $z = [double]$match.Groups[3].Value

    $uniquePositions["$x,$y,$z"] = $true
    $uniqueX["$x"] = $true
    $uniqueY["$y"] = $true
    $minX = [math]::Min($minX, $x)
    $maxX = [math]::Max($maxX, $x)

    Assert-True ($z -gt $camera.Z) "Every clue must be in front of Main Camera; found z=$z, camera z=$($camera.Z)."
    Assert-True ([math]::Abs($z - 2.0) -lt 0.001) "Every clue must stay on the original Show clue plane; found z=$z."
    Assert-True ($x -ge 0.3 -and $x -le 4.0) "Every clue must stay inside the original Show visible clue width; found x=$x."
    Assert-True ($y -ge 3.2 -and $y -le 4.6) "Every clue must stay inside the original Show visible clue height; found y=$y."
}

Assert-True ($uniquePositions.Count -eq 31) "All 31 clues must be individually visible, not stacked; found only $($uniquePositions.Count) unique positions."
Assert-True ($uniqueX.Count -ge 8 -and $uniqueY.Count -ge 4) "Clues must be spread into a visible grid, found $($uniqueX.Count) columns and $($uniqueY.Count) rows."

Assert-True ($scene -match 'Culprit_StartPosition') 'Scene must contain the culprit start-position object.'
$culpritPositionMatch = [regex]::Match($scene, 'Culprit_StartPosition.*?m_LocalPosition:\s*\{x:\s*([-0-9.]+), y:\s*([-0-9.]+), z:\s*([-0-9.]+)\}', 'Singleline')
if ($culpritPositionMatch.Success) {
    $culpritX = [double]$culpritPositionMatch.Groups[1].Value
    $culpritY = [double]$culpritPositionMatch.Groups[2].Value
    $culpritZ = [double]$culpritPositionMatch.Groups[3].Value
    Assert-True ($culpritZ -gt $camera.Z) "Culprit must be in front of Main Camera; found z=$culpritZ, camera z=$($camera.Z)."
    Assert-True ($culpritX -gt $maxX) "Culprit must stand beside the clue grid, not inside or behind it; found x=$culpritX, clue max x=$maxX."
    Assert-True ([math]::Abs($culpritY) -lt 0.001) "Culprit must stand on the floor; found y=$culpritY."
}

Assert-True ($setup -match 'GetCameraVisibleClueWorldPosition' -and $setup -match 'Main Camera') 'Clue setup must place clues from the Main Camera visible area.'
Assert-True ($setup -notmatch 'GetVisibleClueStackWorldPosition') 'Clue setup must not stack every clue at one visible box position.'

Write-Host 'Room02 start placement checks passed.'
