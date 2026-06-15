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

function Assert-InRoom {
    param(
        [object] $Position,
        [string] $RoomName,
        [double] $MinX,
        [double] $MaxX,
        [double] $MinZ,
        [double] $MaxZ
    )

    Assert-True ($Position.X -ge $MinX -and $Position.X -le $MaxX -and $Position.Z -ge $MinZ -and $Position.Z -le $MaxZ) "$($Position.Name) must be in $RoomName; found x=$($Position.X), z=$($Position.Z)."
}

$cluesRoot = Get-NamedPosition $scene 'Clues'
Assert-True ([math]::Abs($cluesRoot.X) -lt 0.001 -and [math]::Abs($cluesRoot.Y) -lt 0.001 -and [math]::Abs($cluesRoot.Z) -lt 0.001) 'Clues root must stay at world origin so clue local positions match room coordinates.'

$clueMatches = [regex]::Matches($scene, 'm_Name:\s+(Clue_[^\r\n]+).*?--- !u!4 &\d+\s*\r?\nTransform:.*?m_LocalPosition:\s*\{x:\s*([-0-9.]+), y:\s*([-0-9.]+), z:\s*([-0-9.]+)\}', 'Singleline')
Assert-True ($clueMatches.Count -eq 31) "Expected 31 clue transforms, found $($clueMatches.Count)."

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

Assert-True ($uniquePositions.Count -eq 31) "All 31 clues must be individually visible, not stacked; found only $($uniquePositions.Count) unique positions."

foreach ($name in @('Clue_normal_cast_notice', 'Clue_normal_production_plan', 'Clue_normal_memorial_frame', 'Clue_normal_conversation_memo', 'Clue_clue_hasho_will')) {
    Assert-InRoom $positions[$name] 'Lobby/notice room' -2.5 3.5 -5.5 -1.0
}

foreach ($name in @('Clue_normal_security_log', 'Clue_normal_cctv_notice', 'Clue_normal_deleted_entry_trace', 'Clue_normal_hidden_camera', 'Clue_normal_torn_letter_a', 'Clue_normal_torn_letter_b')) {
    Assert-InRoom $positions[$name] 'Corridor/security room' -16.0 -6.0 -25.0 -18.0
}

foreach ($name in @('Clue_normal_ward_calendar', 'Clue_normal_medical_certificate', 'Clue_normal_poison_ampoule', 'Clue_normal_nurse_inventory_log', 'Clue_normal_under_table_space', 'Clue_normal_yoanna_relic')) {
    Assert-InRoom $positions[$name] 'Ward room' -39.0 -28.0 -29.0 -20.0
}

foreach ($name in @('Clue_key_clue_coldest_place', 'Clue_normal_gloves', 'Clue_key_clue_fridge_scratches', 'Clue_key_clue_temperature_warning', 'Clue_normal_locker_document')) {
    Assert-InRoom $positions[$name] 'Storage/cold room' 9.0 17.5 -18.5 -10.0
}

foreach ($name in @('Clue_normal_mirror_message', 'Clue_normal_paint_footprints', 'Clue_normal_makeup_toolbox', 'Clue_clue_makeup_diary', 'Clue_normal_jin_sneakers')) {
    Assert-InRoom $positions[$name] 'Dressing room' -16.0 -6.0 -14.0 -6.0
}

foreach ($name in @('Clue_normal_yoanna_memo', 'Clue_normal_sumi_memo', 'Clue_normal_bong_rebuttal', 'Clue_normal_oh_threat_memo')) {
    Assert-InRoom $positions[$name] 'Operating room' 5.0 15.5 -28.5 -20.0
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

Assert-True ($setup -match 'GetRoomDistributedClueWorldPosition' -and $setup -match 'IntegratedCluePositions') 'Clue setup must place clues into room-distributed coordinates.'
Assert-True ($setup -match 'CluesRootPosition' -and $setup -match 'Vector3\.zero') 'Clue setup must keep the Clues root at the world origin.'
Assert-True ($setup -notmatch 'GetVisibleClueStackWorldPosition' -and $setup -notmatch 'GetCameraVisibleClueWorldPosition') 'Clue setup must not stack every clue into the start-camera visible grid.'

Write-Host 'Room02 start placement checks passed.'
