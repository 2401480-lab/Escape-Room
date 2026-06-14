$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$timerUIPath = Join-Path $root 'Assets/Room02_Operating/Clues/TimerUI.cs'
$storyManagerPath = Join-Path $root 'Assets/Room02_Operating/Clues/StoryProgressManager.cs'
$chasePath = Join-Path $root 'Assets/Room02_Operating/Clues/ChaseController.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $timerUIPath) 'Missing Assets/Room02_Operating/Clues/TimerUI.cs'
Assert-True (Test-Path -LiteralPath $storyManagerPath) 'Missing Assets/Room02_Operating/Clues/StoryProgressManager.cs'
Assert-True (Test-Path -LiteralPath $chasePath) 'Missing Assets/Room02_Operating/Clues/ChaseController.cs'

$timerUI = Get-Content -LiteralPath $timerUIPath -Raw -Encoding UTF8
$storyManager = Get-Content -LiteralPath $storyManagerPath -Raw -Encoding UTF8
$chase = Get-Content -LiteralPath $chasePath -Raw -Encoding UTF8
$allTimerCode = "$timerUI`n$storyManager`n$chase"

Assert-True ($timerUI -match 'namespace\s+EscapeRoom') 'TimerUI must use namespace EscapeRoom.'
Assert-True ($timerUI -match 'class\s+TimerUI\s*:\s*MonoBehaviour') 'TimerUI must be a MonoBehaviour.'
Assert-True ($timerUI -match 'ScreenSpaceOverlay') 'TimerUI must create/use a Screen Space Overlay canvas.'
Assert-True ($timerUI -match 'anchorMin\s*=\s*new\s+Vector2\s*\(\s*1f\s*,\s*1f\s*\)' -and $timerUI -match 'anchorMax\s*=\s*new\s+Vector2\s*\(\s*1f\s*,\s*1f\s*\)') 'TimerUI must position the timer in the top-right corner.'
Assert-True ($timerUI -match 'FormatTime\s*\(' -and $timerUI -match 'Mathf\.FloorToInt' -and $timerUI -match '"D2"') 'TimerUI must format the timer as MM:SS.'
Assert-True ($timerUI -match 'deductionColor\s*=\s*HorrorUITheme\.TextMain') 'TimerUI must display deduction timer with the Room02 horror text color.'
Assert-True ($timerUI -match 'chaseColor\s*=\s*HorrorUITheme\.BloodRed') 'TimerUI must display chase timer in blood red.'
Assert-True ($timerUI -match 'urgentThresholdSeconds\s*=\s*180f' -and $timerUI -match 'urgentColor\s*=\s*HorrorUITheme\.SickYellow') 'TimerUI must turn urgent with the Room02 horror warning color when 3 minutes or less remain.'
Assert-True ($timerUI -match 'StoryProgressManager\.Instance' -and $timerUI -match 'CurrentTimerRemaining') 'TimerUI must read timer values from StoryProgressManager.'
Assert-True ($timerUI -match 'IsChaseTimerActive') 'TimerUI must switch color when the chase timer is active.'

Assert-True ($storyManager -match 'CurrentTimerRemaining') 'StoryProgressManager must expose the current timer value for UI.'
Assert-True ($storyManager -match 'IsChaseTimerActive') 'StoryProgressManager must expose whether the chase timer is active.'
Assert-True ($storyManager -match 'deductionTimer\s*=\s*1200f') 'StoryProgressManager deduction timer must start at 20:00.'
Assert-True ($storyManager -match 'RegisterChaseController\s*\(') 'StoryProgressManager must allow ChaseController to register its timer.'
Assert-True ($storyManager -match 'DeductionTimerExpired' -and $storyManager -match 'PlayGameOver\s*\(\s*GameOverReason\.DeductionTimerExpired\s*\)') 'Deduction timer expiry must activate GameOverUI.'
Assert-True ($chase -match 'RegisterChaseController\s*\(\s*this\s*\)') 'ChaseController must register with StoryProgressManager for TimerUI.'
Assert-True ($chase -match 'ChaseTimerExpired' -and $chase -match 'PlayGameOver\s*\(\s*GameOverReason\.ChaseTimerExpired\s*\)') 'Chase timer expiry must activate GameOverUI.'

Assert-True ($allTimerCode -notmatch 'Time\.timeScale') 'Timer systems must not change Time.timeScale.'
Assert-True ($allTimerCode -notmatch 'CursorController') 'Timer systems must not touch CursorController.'

Write-Host 'TimerUI checks passed.'
