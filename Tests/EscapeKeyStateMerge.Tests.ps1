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
$escapeKeyAcquiredText = U 0xD0C8,0xCD9C,0x0020,0xC5F4,0xC1E0,0xB97C,0x0020,0xC5BB,0xC5C8,0xC2B5,0xB2C8,0xB2E4

$collectEscapeKey = [regex]::Match($storyManager, 'public\s+void\s+CollectEscapeKey\s*\(\s*\)(?<body>[\s\S]*?)public\s+void\s+GrantEscapeKeyFromCorrectSuspect').Groups['body'].Value
$correctSuspectGrant = [regex]::Match($storyManager, 'public\s+void\s+GrantEscapeKeyFromCorrectSuspect\s*\(\s*\)(?<body>[\s\S]*?)public\s+void\s+GrantEscapeKeyFromAdminSkip').Groups['body'].Value
$adminSkipGrant = [regex]::Match($storyManager, 'public\s+void\s+GrantEscapeKeyFromAdminSkip\s*\(\s*\)(?<body>[\s\S]*?)public\s+void\s+TriggerAdminFailureCountdown').Groups['body'].Value
$autoCollectEscapeKey = [regex]::Match($storyManager, 'private\s+void\s+TryAutoCollectEscapeKey\s*\(\s*\)(?<body>[\s\S]*?)private\s+void\s+EnsureJournalSubscription').Groups['body'].Value

Assert-True ($keyState -match 'namespace\s+EscapeRoom') 'EscapeKeyState must use namespace EscapeRoom.'
Assert-True ($keyState -match 'static\s+class\s+EscapeKeyState') 'EscapeKeyState must be a static integration point.'
Assert-True ($keyState -match 'public\s+static\s+bool\s+HasKey') 'EscapeKeyState must expose public static HasKey.'
Assert-True ($keyState -match 'public\s+static\s+void\s+GrantKey\s*\(\s*\)') 'EscapeKeyState must expose public static GrantKey().'
Assert-True ($keyState -match 'RuntimeInitializeOnLoadMethod') 'EscapeKeyState must reset cleanly when Unity reloads the runtime.'

foreach ($method in @('CollectEscapeKey', 'GrantEscapeKeyFromCorrectSuspect', 'GrantEscapeKeyFromAdminSkip', 'TryAutoCollectEscapeKey')) {
    Assert-True ($storyManager -match "$method[\s\S]*?EscapeKeyState\.GrantKey\s*\(\s*\)") "$method must call EscapeKeyState.GrantKey() when granting the key."
}

Assert-True ($storyManager -match 'HasEscapeKey\s*=>[\s\S]*?EscapeKeyState\.HasKey') 'StoryProgressManager.HasEscapeKey must reflect EscapeKeyState.HasKey.'
Assert-True ($collectEscapeKey -match 'ShowKeyAcquiredNotice\s*\(\s*\)') 'Manual key clue collection must still show the general key acquisition notice through the suppressible notice helper.'
Assert-True ($autoCollectEscapeKey -match 'ShowKeyAcquiredNotice\s*\(\s*\)') 'Collecting all 15 clues naturally must route the general key acquisition notice through the suppressible notice helper.'
Assert-True ($storyManager -match 'ShowKeyAcquiredNotice[\s\S]*?!suppressKeyAcquiredNotice[\s\S]*?EscapeKeyNoticeUI\.ShowKeyAcquired\s*\(\s*\)') 'General key acquisition notices must be suppressible for Y admin skip.'
Assert-True ($correctSuspectGrant -match 'EscapeKeyNoticeUI\.ShowEscapeKeyAcquired\s*\(\s*\)') 'Correct culprit selection must show the escape-key acquisition notice.'
Assert-True ($correctSuspectGrant -match 'if\s*\(\s*HasEscapeKey\s*\)\s*\{\s*EscapeKeyNoticeUI\.ShowEscapeKeyAcquired\s*\(\s*\);\s*return\s*;\s*\}') 'Correct culprit selection must still show the escape-key notice when the key was already granted.'
Assert-True ($adminSkipGrant -notmatch 'EscapeKeyNoticeUI\.Show') 'Y admin skip must grant the key silently without showing a key-acquired subtitle.'
Assert-True ($adminGuide -match 'CollectAllCluesAndGrantKey\s*\(\s*\)[\s\S]*?GrantEscapeKeyFromAdminSkip\s*\(\s*\)') 'Y admin skip must still grant the key through the StoryProgressManager path.'
Assert-True ($endingUI -match 'GrantEscapeKeyFromCorrectSuspect\s*\(\s*\);[\s\S]*?BeginChase\s*\(\s*\)') 'Correct culprit selection must grant the key before chase/QTE handoff.'

Assert-True ($keyNotice.Contains($keyAcquiredText)) 'The key acquisition notice must use the requested Korean key-acquired text.'
Assert-True ($keyNotice.Contains($escapeKeyAcquiredText)) 'The correct-culprit key notice must say the escape key was acquired.'
Assert-True ($keyNotice -match 'ShowKeyAcquired\s*\(\s*\)' -and $keyNotice -match 'ShowEscapeKeyAcquired\s*\(\s*\)' -and $keyNotice -match 'TextMeshProUGUI') 'EscapeKeyNoticeUI must expose visible TMP notices for general and escape-key acquisition.'
Assert-True ($keyNotice -match 'ScreenSpaceOverlay') 'EscapeKeyNoticeUI must use an overlay canvas so it works without scene edits.'

Assert-True ($exitDoor -match 'ExitDoorObjectName\s*=\s*"ExitDoor"') 'EscapeExitDoor must standardize the exit door object name as ExitDoor.'
Assert-True ($exitDoor -match 'gameObject\.name\s*=\s*ExitDoorObjectName') 'EscapeExitDoor must name its GameObject ExitDoor at runtime without editing Show.unity.'
Assert-True ($exitDoor -match 'KeyCode\.E' -and $exitDoor -match 'KeyCode\.F') 'EscapeExitDoor must support both E and F interaction keys.'
Assert-True ($exitDoor -match 'EscapeKeyState\.HasKey') 'EscapeExitDoor must accept the shared EscapeKeyState key state.'

Write-Host 'Escape key merge contract checks passed.'
