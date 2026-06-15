$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$keyStatePath = Join-Path $root 'Assets/Room02_Operating/Clues/EscapeKeyState.cs'
$keyNoticePath = Join-Path $root 'Assets/Room02_Operating/Clues/EscapeKeyNoticeUI.cs'
$storyManagerPath = Join-Path $root 'Assets/Room02_Operating/Clues/StoryProgressManager.cs'
$adminGuidePath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueAdminGuideOverlay.cs'
$endingUIPath = Join-Path $root 'Assets/Room02_Operating/Clues/EndingUI.cs'
$exitDoorPath = Join-Path $root 'Assets/Room02_Operating/Clues/EscapeExitDoor.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function U {
    param([int[]] $CodePoints)
    -join ($CodePoints | ForEach-Object { [char]$_ })
}

Assert-True (Test-Path -LiteralPath $keyStatePath) 'EscapeKeyState.cs must exist for teammate QTE integration.'
Assert-True (Test-Path -LiteralPath $keyNoticePath) 'EscapeKeyNoticeUI.cs must exist to show the escape key acquisition notice.'
Assert-True (Test-Path -LiteralPath $storyManagerPath) 'Missing StoryProgressManager.'
Assert-True (Test-Path -LiteralPath $adminGuidePath) 'Missing ClueAdminGuideOverlay.'
Assert-True (Test-Path -LiteralPath $endingUIPath) 'Missing EndingUI.'
Assert-True (Test-Path -LiteralPath $exitDoorPath) 'Missing EscapeExitDoor.'

$keyState = Get-Content -LiteralPath $keyStatePath -Raw -Encoding UTF8
$keyNotice = Get-Content -LiteralPath $keyNoticePath -Raw -Encoding UTF8
$storyManager = Get-Content -LiteralPath $storyManagerPath -Raw -Encoding UTF8
$adminGuide = Get-Content -LiteralPath $adminGuidePath -Raw -Encoding UTF8
$endingUI = Get-Content -LiteralPath $endingUIPath -Raw -Encoding UTF8
$exitDoor = Get-Content -LiteralPath $exitDoorPath -Raw -Encoding UTF8

$keyAcquiredText = U 0xC5F4,0xC1E0,0xB97C,0x0020,0xC5BB,0xC5C8,0xC2B5,0xB2C8,0xB2E4

Assert-True ($keyState -match 'namespace\s+EscapeRoom') 'EscapeKeyState must use namespace EscapeRoom.'
Assert-True ($keyState -match 'static\s+class\s+EscapeKeyState') 'EscapeKeyState must be a static integration point.'
Assert-True ($keyState -match 'public\s+static\s+bool\s+HasKey') 'EscapeKeyState must expose public static HasKey.'
Assert-True ($keyState -match 'public\s+static\s+void\s+GrantKey\s*\(\s*\)') 'EscapeKeyState must expose public static GrantKey().'
Assert-True ($keyState -match 'RuntimeInitializeOnLoadMethod') 'EscapeKeyState must reset cleanly when Unity reloads the runtime.'

foreach ($method in @('CollectEscapeKey', 'GrantEscapeKeyFromCorrectSuspect', 'GrantEscapeKeyFromAdminSkip', 'TryAutoCollectEscapeKey')) {
    Assert-True ($storyManager -match "$method[\s\S]*?EscapeKeyState\.GrantKey\s*\(\s*\)") "$method must call EscapeKeyState.GrantKey() when granting the key."
    Assert-True ($storyManager -match "$method[\s\S]*?EscapeKeyNoticeUI\.ShowKeyAcquired\s*\(\s*\)") "$method must show the key acquisition notice."
}

Assert-True ($storyManager -match 'HasEscapeKey\s*=>[\s\S]*?EscapeKeyState\.HasKey') 'StoryProgressManager.HasEscapeKey must reflect EscapeKeyState.HasKey.'
Assert-True ($storyManager -match 'GrantEscapeKeyFromCorrectSuspect[\s\S]*?if\s*\(\s*HasEscapeKey\s*\)\s*\{\s*EscapeKeyNoticeUI\.ShowKeyAcquired\s*\(\s*\);\s*return\s*;\s*\}') 'Correct culprit selection must still show the key acquisition notice when the key was already granted.'
Assert-True ($adminGuide -match 'CollectAllCluesAndGrantKey\s*\(\s*\)[\s\S]*?GrantEscapeKeyFromAdminSkip\s*\(\s*\)') 'Y admin skip must still grant the key through the StoryProgressManager path.'
Assert-True ($endingUI -match 'GrantEscapeKeyFromCorrectSuspect\s*\(\s*\);[\s\S]*?BeginChase\s*\(\s*\)') 'Correct culprit selection must grant the key before chase/QTE handoff.'

Assert-True ($keyNotice.Contains($keyAcquiredText)) 'The key acquisition notice must use the requested Korean key-acquired text.'
Assert-True ($keyNotice -match 'ShowKeyAcquired\s*\(\s*\)' -and $keyNotice -match 'TextMeshProUGUI') 'EscapeKeyNoticeUI must expose a visible TMP notice.'
Assert-True ($keyNotice -match 'ScreenSpaceOverlay') 'EscapeKeyNoticeUI must use an overlay canvas so it works without scene edits.'

Assert-True ($exitDoor -match 'ExitDoorObjectName\s*=\s*"ExitDoor"') 'EscapeExitDoor must standardize the exit door object name as ExitDoor.'
Assert-True ($exitDoor -match 'gameObject\.name\s*=\s*ExitDoorObjectName') 'EscapeExitDoor must name its GameObject ExitDoor at runtime without editing Show.unity.'
Assert-True ($exitDoor -match 'KeyCode\.E' -and $exitDoor -match 'KeyCode\.F') 'EscapeExitDoor must support both E and F interaction keys.'
Assert-True ($exitDoor -match 'EscapeKeyState\.HasKey') 'EscapeExitDoor must accept the shared EscapeKeyState key state.'

Write-Host 'Escape key merge contract checks passed.'
