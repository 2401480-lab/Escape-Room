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
Assert-True (Test-Path -LiteralPath $layoutPath) 'Missing Room02 runtime clue placement layout.'
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
    Assert-True ($layout.Contains("""$id""")) "Placement layout missing clue ID: $id"
}

foreach ($zone in @(
    (From-B64 '67O164+E'),
    (From-B64 '67OR7Iuk'),
    (From-B64 '67O06rSA7Iuk'),
    (From-B64 '67aE7J6l7Iuk'),
    (From-B64 '7IiY7Iig7Iuk')
)) {
    Assert-True ($layout.Contains($zone)) "Placement layout must document zone: $zone"
}

$vectorCount = ([regex]::Matches($layout, 'new\s+Vector3\s*\(')).Count
Assert-True ($vectorCount -ge 15) "Placement layout must define at least 15 world positions, found $vectorCount."
Assert-True ($layout -match 'TryGetPosition\s*\(') 'Placement layout must expose TryGetPosition.'
Assert-True ($layout -match 'ApplyExistingSceneCluePositions\s*\(') 'Placement layout must be able to reposition existing Show clue objects at runtime.'
Assert-True ($setup -match 'CluePlacementLayout\.TryGetPosition') 'Editor clue setup must use the shared room-distributed clue positions.'
Assert-True ($bootstrapper -match 'CluePlacementLayout\.ApplyExistingSceneCluePositions') 'Runtime bootstrapper must repair clue positions after Onboarding loads Show.'
Assert-True ($setup -notmatch 'GetStartAreaClueWorldPosition') 'Clue setup must not place every clue back into the start-area grid.'
Assert-True ($setup -notmatch 'StartAreaGridColumns|StartAreaFirstX|StartAreaFirstZ') 'Start-area-only placement constants must be removed.'
Assert-True ($setup -match 'CluesRootPosition' -and $setup -match 'Vector3\.zero') 'Clue setup must keep the Clues root at the world origin.'

Write-Host 'Room02 distributed clue placement checks passed.'
