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

$cluesRoot = Get-NamedPosition $scene 'Clues'
Assert-True ([math]::Abs($cluesRoot.X) -lt 0.001 -and [math]::Abs($cluesRoot.Y) -lt 0.001 -and [math]::Abs($cluesRoot.Z) -lt 0.001) 'Clues root must stay at world origin so clue local positions match room coordinates.'

$clueMatches = [regex]::Matches($scene, 'm_Name:\s+(Clue_[^\r\n]+).*?--- !u!4 &\d+\s*\r?\nTransform:.*?m_LocalPosition:\s*\{x:\s*([-0-9.]+), y:\s*([-0-9.]+), z:\s*([-0-9.]+)\}', 'Singleline')
Assert-True ($clueMatches.Count -eq 15) "Expected 15 clue transforms, found $($clueMatches.Count)."

$camera = Get-NamedPosition $scene 'Main Camera'
$uniquePositions = @{}
$positions = @{}

foreach ($match in $clueMatches) {
    $name = $match.Groups[1].Value
    $x = [double]$match.Groups[2].Value
    $y = [double]$match.Groups[3].Value
    $z = [double]$match.Groups[4].Value

    $uniquePositions["$x,$y,$z"] = $true
    $positions[$name] = [pscustomobject]@{
        Name = $name
        X = $x
        Y = $y
        Z = $z
    }

    Assert-True ($y -ge 0.3 -and $y -le 1.1) "$name must sit near floor height; found y=$y."
}

Assert-True ($uniquePositions.Count -eq 15) "All 15 clues must be individually visible, not stacked; found only $($uniquePositions.Count) unique positions."

foreach ($position in $positions.Values) {
    Assert-True ($position.X -ge -2.7 -and $position.X -le 2.7) "$($position.Name) must be near the player start in X; found x=$($position.X)."
    Assert-True ($position.Z -ge 2.2 -and $position.Z -le 5.0) "$($position.Name) must be in front of the player start; found z=$($position.Z)."
}

Assert-True ($scene -match 'Culprit_StartPosition') 'Scene must contain the culprit start-position object.'
$culpritPositionMatch = [regex]::Match($scene, 'Culprit_StartPosition.*?m_LocalPosition:\s*\{x:\s*([-0-9.]+), y:\s*([-0-9.]+), z:\s*([-0-9.]+)\}', 'Singleline')
if ($culpritPositionMatch.Success) {
    $culpritX = [double]$culpritPositionMatch.Groups[1].Value
    $culpritY = [double]$culpritPositionMatch.Groups[2].Value
    $culpritZ = [double]$culpritPositionMatch.Groups[3].Value
    Assert-True ($culpritZ -gt $camera.Z) "Culprit must be in front of Main Camera; found z=$culpritZ, camera z=$($camera.Z)."
    Assert-True ($culpritX -ge 3.0 -and $culpritX -le 6.0) "Culprit must remain near the start-side visible placement; found x=$culpritX."
    Assert-True ([math]::Abs($culpritY) -lt 0.001) "Culprit must stand on the floor; found y=$culpritY."
}

Assert-True ($setup -match 'GetStartAreaClueWorldPosition') 'Clue setup must place clues into the start-area visible grid.'
Assert-True ($setup -match 'CluesRootPosition' -and $setup -match 'Vector3\.zero') 'Clue setup must keep the Clues root at the world origin.'
Assert-True ($setup -notmatch 'IntegratedCluePositions' -and $setup -notmatch 'GetRoomDistributedClueWorldPosition') 'Clue setup must not send clues back to room-distributed coordinates.'

Write-Host 'Room02 start placement checks passed.'
