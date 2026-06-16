$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$hintPath = Join-Path $root 'Assets/Room02_Operating/Clues/ControlHintUI.cs'
$bootstrapperPath = Join-Path $root 'Assets/Room02_Operating/Clues/HudRuntimeBootstrapper.cs'
$doorInteractorPath = Join-Path $root 'Assets/_Shared/Scripts/DoorInteractor.cs'
$escapeExitDoorPath = Join-Path $root 'Assets/Room02_Operating/Clues/EscapeExitDoor.cs'
$settingsPath = Join-Path $root 'Assets/Room02_Operating/Clues/SettingsUI.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function U {
    param([int[]] $CodePoints)
    return -join ($CodePoints | ForEach-Object { [char] $_ })
}

Assert-True (Test-Path -LiteralPath $hintPath) 'Missing Room02 ControlHintUI.'
Assert-True (Test-Path -LiteralPath $bootstrapperPath) 'Missing Room02 HUD runtime bootstrapper.'
Assert-True (Test-Path -LiteralPath $doorInteractorPath) 'Missing shared DoorInteractor.'
Assert-True (Test-Path -LiteralPath $escapeExitDoorPath) 'Missing Room02 EscapeExitDoor.'
Assert-True (Test-Path -LiteralPath $settingsPath) 'Missing Room02 SettingsUI.'

$hint = Get-Content -LiteralPath $hintPath -Raw -Encoding UTF8
$bootstrapper = Get-Content -LiteralPath $bootstrapperPath -Raw -Encoding UTF8
$doorInteractor = Get-Content -LiteralPath $doorInteractorPath -Raw -Encoding UTF8
$escapeExitDoor = Get-Content -LiteralPath $escapeExitDoorPath -Raw -Encoding UTF8
$settings = Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8
$runFastText = (U 0xBE68,0xB9AC) + ' ' + (U 0xB2EC,0xB9AC,0xAE30)
$openDoorText = (U 0xBB38) + (U 0xC5F4,0xAE30)
$openDoorActionText = '- ' + $openDoorText

Assert-True ($hint -match 'namespace\s+EscapeRoom' -and $hint -match 'class\s+ControlHintUI\s*:\s*MonoBehaviour') 'ControlHintUI must be an EscapeRoom MonoBehaviour.'
Assert-True ($hint -match 'HUD_Canvas' -and $hint -match 'ScreenSpaceOverlay') 'ControlHintUI must attach to HUD_Canvas as a Screen Space Overlay HUD.'
Assert-True ($hint -match 'KeyboardControlHintPanel' -and $hint -match 'ControlKeycap') 'ControlHintUI must render hints as keyboard keycap rows.'
Assert-True ($hint.Contains('SHIFT') -and $hint.Contains($runFastText)) 'ControlHintUI must always show SHIFT run-fast guidance.'
Assert-True ($hint.Contains('E') -and $hint.Contains($openDoorActionText)) 'ControlHintUI must include an E - open-door prompt.'
Assert-True ($hint -match 'DoorOpenHintRow' -and $hint -match 'SetDoorPromptVisible\s*\(') 'ControlHintUI must expose a toggle for the door prompt row.'
Assert-True ($hint -match 'EnsureDoorOpenHintRow\s*\(' -and $hint -match 'panelRoot\.transform\.Find\s*\(\s*"DoorOpenHintRow"\s*\)') 'ControlHintUI must repair existing SHIFT-only hint panels by finding or creating DoorOpenHintRow.'
Assert-True ($hint -match 'EnsureUI[\s\S]*?EnsureDoorOpenHintRow\s*\(\s*\)[\s\S]*?RefreshDoorPromptVisibility\s*\(\s*\)') 'ControlHintUI must ensure the E - 문열기 row even when the hint panel already exists.'
Assert-True ($hint.Contains("CreateHintRow(panelRoot.transform, `"E`", `"$openDoorActionText`", true)")) 'ControlHintUI must always show E - 문열기 beside the SHIFT guidance.'
Assert-True ($hint -match 'GetOrCreateInstance\s*\(' -and $hint -match 'new\s+GameObject\s*\(\s*"ControlHintUI"\s*\)') 'ControlHintUI.SetDoorPromptVisible must create the HUD if the scene did not bootstrap it yet.'
Assert-True ($hint -match 'ConfigureHintRow\s*\(' -and $hint -match 'SetHintRowText\s*\(' -and $hint -match 'doorOpenHintRow\.SetActive\s*\(\s*true\s*\)') 'ControlHintUI must repair existing rows by forcing E - 문열기 text and active state.'
Assert-True ($hint -match 'RefreshDoorPromptVisibility[\s\S]*?doorOpenHintRow\.SetActive\s*\(\s*true\s*\)' -and $hint -notmatch 'doorPromptVisibleUntilTime') 'ControlHintUI must not hide E - 문열기 behind a timer-based prompt.'
Assert-True ($hint -match 'FontHelper\.Apply' -or $hint -match 'HorrorUITheme\.ApplyText') 'ControlHintUI runtime text must use the Galmuri font helper.'
Assert-True ($hint -notmatch 'Time\.timeScale' -and $hint -notmatch 'CursorController') 'ControlHintUI must not alter gameplay time or cursor behavior.'

Assert-True ($bootstrapper -match 'EnsureRuntimeObject<ControlHintUI>\s*\(\s*"ControlHintUI"\s*\)') 'HUD bootstrapper must create ControlHintUI in Room02.'
Assert-True ($doorInteractor -match 'ControlHintUI\.SetDoorPromptVisible\s*\(') 'DoorInteractor must toggle E 문열기 when a door is in range.'
Assert-True ($doorInteractor -match 'TryFindDoor\s*\(\s*ray\s*,\s*out\s+Transform\s+\w+\s*,\s*out\s+Vector3\s+\w+\s*\)') 'DoorInteractor must reuse its door detection for the prompt.'
Assert-True ($doorInteractor -match 'OverlapSphereNonAlloc' -and $doorInteractor -match 'TryFindNearbyDoor') 'DoorInteractor must also show/open E 문열기 for nearby doors, not only centered raycast doors.'
Assert-True ($escapeExitDoor -match 'ControlHintUI\.SetDoorPromptVisible\s*\(\s*inRange\s*\)') 'EscapeExitDoor must show E 문열기 while the exit door is in range.'
Assert-True ($settings.Contains("CreateControlRow(rect, `"E`", `"$openDoorText`"")) 'Settings controls must list E as open door.'
Assert-True ($settings.Contains("CreateControlRow(rect, `"Left Shift`", `"$runFastText`"")) 'Settings controls must list Shift as run fast.'

Write-Host 'Control hint UI checks passed.'
