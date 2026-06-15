$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$scenePath = Join-Path $root 'Assets/Room02_Operating/Scenes/Show.unity'
$setupPath = Join-Path $root 'Assets/Room02_Operating/Clues/Editor/ClueSceneSetupTool.cs'
$layoutPath = Join-Path $root 'Assets/Room02_Operating/Clues/CluePlacementLayout.cs'
$bootstrapperPath = Join-Path $root 'Assets/Room02_Operating/Clues/HudRuntimeBootstrapper.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function From-B64 {
    param([string] $Value)
    return [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($Value))
}

Assert-True (Test-Path -LiteralPath $scenePath) 'Missing Room02 Show gameplay scene.'
Assert-True (Test-Path -LiteralPath $setupPath) 'Missing Room02 clue setup tool.'
Assert-True (Test-Path -LiteralPath $layoutPath) 'Missing Room02 shared clue placement layout.'
Assert-True (Test-Path -LiteralPath $bootstrapperPath) 'Missing Room02 runtime bootstrapper.'

$scene = Get-Content -LiteralPath $scenePath -Raw -Encoding UTF8
$setup = Get-Content -LiteralPath $setupPath -Raw -Encoding UTF8
$layout = Get-Content -LiteralPath $layoutPath -Raw -Encoding UTF8
$bootstrapper = Get-Content -LiteralPath $bootstrapperPath -Raw -Encoding UTF8

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
Assert-True ($clueMatches.Count -eq 15) "Expected 15 clue transforms in Show, found $($clueMatches.Count)."

$requiredIDs = @(
    'normal_cast_notice',
    'normal_memorial_frame',
    'normal_conversation_memo',
    'normal_medical_certificate',
    'normal_ward_calendar',
    'clue_hasho_will',
    'key_clue_coldest_place',
    'key_clue_temperature_warning',
    'normal_bong_rebuttal',
    'key_clue_fridge_scratches',
    'normal_makeup_toolbox',
    'normal_sumi_memo',
    'clue_makeup_diary',
    'normal_under_table_space',
    'normal_mirror_message'
)

foreach ($id in $requiredIDs) {
    Assert-True ($layout -match "(?m)^\s*case\s+`"$([regex]::Escape($id))`"\s*:") "Placement layout missing active case label for clue ID: $id"
}

$cluePositions = @()
foreach ($match in $clueMatches) {
    $cluePositions += [pscustomobject]@{
        Name = $match.Groups[1].Value
        X = [double]$match.Groups[2].Value
        Y = [double]$match.Groups[3].Value
        Z = [double]$match.Groups[4].Value
    }
}

foreach ($position in $cluePositions) {
    Assert-True ($position.X -ge -40 -and $position.X -le 20 -and $position.Z -ge -36 -and $position.Z -le 5) "$($position.Name) is outside the Room02 map bounds."
}

$startVisibleClues = @($cluePositions | Where-Object { [math]::Abs($_.X) -le 4.5 -and $_.Z -ge 0 -and $_.Z -le 5.5 })
Assert-True ($startVisibleClues.Count -eq 2) "Exactly 2 clues should remain near the start view, found $($startVisibleClues.Count): $($startVisibleClues.Name -join ', ')"

$coarseCells = @{}
foreach ($position in $cluePositions) {
    $cell = "{0}:{1}" -f [math]::Floor($position.X / 5), [math]::Floor($position.Z / 5)
    $coarseCells[$cell] = $true
}
Assert-True ($coarseCells.Count -ge 8) "Clues must be distributed across the room, found only $($coarseCells.Count) coarse map cells."

foreach ($id in $requiredIDs) {
    $objectName = "Clue_$id"
    $scenePosition = $cluePositions | Where-Object { $_.Name -eq $objectName } | Select-Object -First 1
    Assert-True ($null -ne $scenePosition) "Missing scene clue object: $objectName"

    $layoutMatch = [regex]::Match(
        $layout,
        "(?m)^\s*case\s+`"$([regex]::Escape($id))`"\s*:\s*\r?\n\s*position\s*=\s*new\s+Vector3\s*\(\s*([-0-9.]+)f?\s*,\s*([-0-9.]+)f?\s*,\s*([-0-9.]+)f?\s*\)",
        'Singleline')
    Assert-True $layoutMatch.Success "Missing layout vector for clue ID: $id"

    $layoutX = [double]$layoutMatch.Groups[1].Value
    $layoutY = [double]$layoutMatch.Groups[2].Value
    $layoutZ = [double]$layoutMatch.Groups[3].Value
    $matchesLayout = [math]::Abs($scenePosition.X - $layoutX) -lt 0.01 `
        -and [math]::Abs($scenePosition.Y - $layoutY) -lt 0.01 `
        -and [math]::Abs($scenePosition.Z - $layoutZ) -lt 0.01
    Assert-True $matchesLayout "$objectName scene position must match CluePlacementLayout so Play mode does not shift it."
}

foreach ($zone in @(
    'Start area',
    'Back-left room',
    'Right-side room',
    'Mid-left room',
    'Far surgical room'
)) {
    Assert-True ($layout.Contains($zone)) "Placement layout must document spread area: $zone"
}

$vectorCount = ([regex]::Matches($layout, 'new\s+Vector3\s*\(')).Count
Assert-True ($vectorCount -ge 15) "Placement layout must define at least 15 world positions, found $vectorCount."
Assert-True ($layout -match 'TryGetPosition\s*\(') 'Placement layout must expose TryGetPosition.'
Assert-True ($setup -match 'CluePlacementLayout\.TryGetPosition') 'Editor clue setup must use the shared room-distributed clue positions.'
Assert-True ($setup -notmatch 'SceneNeedsPositionRepair') 'Editor auto-repair must not overwrite manually moved clue positions.'
Assert-True ($bootstrapper -notmatch 'CluePlacementLayout\.ApplyExistingSceneCluePositions') 'Runtime bootstrapper must preserve manually moved clue positions in Play mode.'
Assert-True ($setup -notmatch 'GetStartAreaClueWorldPosition') 'Clue setup must not place every clue back into the start-area grid.'
Assert-True ($setup -notmatch 'StartAreaGridColumns|StartAreaFirstX|StartAreaFirstZ') 'Start-area-only placement constants must be removed.'
Assert-True ($setup -match 'CluesRootPosition' -and $setup -match 'Vector3\.zero') 'Clue setup must keep the Clues root at the world origin.'

Write-Host 'Room02 distributed clue placement checks passed.'
