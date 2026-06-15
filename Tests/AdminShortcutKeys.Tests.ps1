$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$guidePath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueAdminGuideOverlay.cs'
$storyManagerPath = Join-Path $root 'Assets/Room02_Operating/Clues/StoryProgressManager.cs'
$endingUIPath = Join-Path $root 'Assets/Room02_Operating/Clues/EndingUI.cs'
$settingsPath = Join-Path $root 'Assets/Room02_Operating/Clues/SettingsUI.cs'
$bootstrapperPath = Join-Path $root 'Assets/Room02_Operating/Clues/HudRuntimeBootstrapper.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function U {
    param([int[]] $CodePoints)
    return -join ($CodePoints | ForEach-Object { [char] $_ })
}

Assert-True (Test-Path -LiteralPath $guidePath) 'Missing Room02 admin guide overlay script.'
Assert-True (Test-Path -LiteralPath $storyManagerPath) 'Missing Room02 story progress manager script.'
Assert-True (Test-Path -LiteralPath $endingUIPath) 'Missing Room02 ending UI script.'
Assert-True (Test-Path -LiteralPath $settingsPath) 'Missing Room02 settings UI script.'
Assert-True (Test-Path -LiteralPath $bootstrapperPath) 'Missing Room02 HUD runtime bootstrapper script.'

$guide = Get-Content -LiteralPath $guidePath -Raw -Encoding UTF8
$storyManager = Get-Content -LiteralPath $storyManagerPath -Raw -Encoding UTF8
$endingUI = Get-Content -LiteralPath $endingUIPath -Raw -Encoding UTF8
$settings = Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8
$bootstrapper = Get-Content -LiteralPath $bootstrapperPath -Raw -Encoding UTF8
$culpritChaseText = (U 0xBC94,0xC778) + (U 0xCC3E,0xAE30) + ' (G)'

Assert-True ($guide -match 'Input\.GetKeyDown\s*\(\s*KeyCode\.U\s*\)') 'Admin guide overlay must trigger the failure shortcut with the U key.'
Assert-True ($guide -match 'TriggerFiveSecondFailure\s*\(') 'Admin guide overlay must isolate the U-key failure shortcut.'
Assert-True ($guide -match 'TriggerAdminFailureCountdown\s*\(') 'Admin failure shortcut must use StoryProgressManager instead of duplicating GameOver logic.'

Assert-True ($storyManager -match 'adminFailureCountdownSeconds\s*=\s*5f') 'StoryProgressManager must use a 5-second admin failure countdown.'
Assert-True ($storyManager -match 'public\s+void\s+TriggerAdminFailureCountdown\s*\(') 'StoryProgressManager must expose an admin failure countdown method.'
Assert-True ($storyManager -match 'TriggerAdminFailureCountdown[\s\S]*?deductionTimeRemaining\s*=\s*adminFailureCountdownSeconds') 'Admin failure countdown must set the timer to 5 seconds.'
Assert-True ($storyManager -match 'forceDeductionFailureCountdown\s*=\s*true') 'Admin failure countdown must keep ticking even if normal deduction timer gates are bypassed.'
Assert-True ($storyManager -match 'CurrentTimerRemaining\s*=>\s*forceDeductionFailureCountdown\s*\?\s*deductionTimeRemaining') 'Timer UI must show the 5-second admin failure countdown before any chase timer.'
Assert-True ($storyManager -match 'TickDeductionTimer\s*\(') 'StoryProgressManager must share normal and admin timer expiry logic.'
Assert-True ($storyManager -match 'DeductionTimerExpired\s*\(\s*\)') 'Admin failure countdown must reuse the existing deduction failure GameOver path.'
Assert-True ($storyManager -match 'GetOrCreateGameOverUI\s*\(\s*\)\?\.PlayGameOver\s*\(\s*GameOverReason\.DeductionTimerExpired\s*\)') 'Deduction timer expiry must show GameOver through a guaranteed GameOverUI instance.'
Assert-True ($storyManager -match 'new\s+GameObject\s*\(\s*"GameOverUI"\s*\)[\s\S]*?AddComponent<GameOverUI>\s*\(\s*\)') 'StoryProgressManager must create GameOverUI when the scene did not bootstrap one.'
Assert-True ($storyManager -notmatch 'GameOverUI\.Instance\?\.PlayGameOver\s*\(\s*GameOverReason\.DeductionTimerExpired\s*\)') 'Deduction timer expiry must not silently skip visible GameOver when GameOverUI.Instance is null.'
Assert-True ($bootstrapper -match 'EnsureRuntimeObject<GameOverUI>\s*\(\s*"GameOverUI"\s*\)') 'Room02 runtime bootstrapper must create GameOverUI for visible admin U failures.'

Assert-True ($endingUI -match 'Input\.GetKeyDown\s*\(\s*KeyCode\.G\s*\)') 'EndingUI must open culprit finding with the G key.'
Assert-True ($endingUI -match 'TryShowCulpritSelectionShortcut\s*\(') 'EndingUI must isolate the G-key culprit finding shortcut.'
Assert-True ($endingUI -match 'TryShowCulpritSelectionShortcut[\s\S]*?CanSelectSuspect') 'G shortcut must respect the existing suspect-selection unlock condition.'
Assert-True ($endingUI -match 'TryShowCulpritSelectionShortcut[\s\S]*?Show\s*\(\s*\)') 'G shortcut must open the same culprit selection UI as the HUD button.'
Assert-True ($endingUI.Contains($culpritChaseText)) 'EndingUI HUD button must show the culprit finding shortcut as 범인찾기 (G).'
Assert-True ($endingUI -match 'HandleSuspectNumberShortcuts\s*\(') 'EndingUI must isolate keyboard number selection for suspects.'
foreach ($key in @('Alpha1', 'Alpha2', 'Alpha3', 'Alpha4', 'Keypad1', 'Keypad2', 'Keypad3', 'Keypad4')) {
    Assert-True ($endingUI -match "KeyCode\.$key") "EndingUI must support $key for cursor-free suspect selection."
}
Assert-True ($endingUI -match 'ConfirmSuspect\s*\(\s*SuspectChoice\.JinSewoong\s*\)' -and
             $endingUI -match 'ConfirmSuspect\s*\(\s*SuspectChoice\.BongTaehyeon\s*\)' -and
             $endingUI -match 'ConfirmSuspect\s*\(\s*SuspectChoice\.MoonSumi\s*\)' -and
             $endingUI -match 'ConfirmSuspect\s*\(\s*SuspectChoice\.OhSejin\s*\)') 'Number keys must immediately confirm the selected suspect without mouse input.'
foreach ($prefix in @('1. ', '2. ', '3. ', '4. ')) {
    Assert-True ($endingUI.Contains($prefix)) "EndingUI suspect buttons must show numeric prefix $prefix."
}
Assert-True ($settings.Contains('CreateControlRow(rect, "G",') -and $settings.Contains($culpritChaseText)) 'Settings controls must list G as 범인찾기 (G).'

Write-Host 'Admin shortcut key checks passed.'
