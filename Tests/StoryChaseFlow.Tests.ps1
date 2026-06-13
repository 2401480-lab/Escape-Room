$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$storyManagerPath = Join-Path $root 'Assets/Clues/StoryProgressManager.cs'
$lockSystemPath = Join-Path $root 'Assets/Clues/LockSystem.cs'
$endingUIPath = Join-Path $root 'Assets/Clues/EndingUI.cs'
$silhouettePath = Join-Path $root 'Assets/Clues/SilhouetteController.cs'
$chasePath = Join-Path $root 'Assets/Clues/ChaseController.cs'
$exitDoorPath = Join-Path $root 'Assets/Clues/EscapeExitDoor.cs'
$gameOverPath = Join-Path $root 'Assets/Clues/GameOverUI.cs'
$clueDataPath = Join-Path $root 'Assets/Clues/ClueData.cs'
$journalManagerPath = Join-Path $root 'Assets/Clues/ClueJournalManager.cs'
$journalUIPath = Join-Path $root 'Assets/Clues/ClueJournalUI.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

foreach ($path in @($storyManagerPath, $lockSystemPath, $endingUIPath, $silhouettePath, $chasePath, $exitDoorPath, $gameOverPath)) {
    Assert-True (Test-Path -LiteralPath $path) "Missing required story system file: $path"
}

$storyManager = Get-Content -LiteralPath $storyManagerPath -Raw -Encoding UTF8
$lockSystem = Get-Content -LiteralPath $lockSystemPath -Raw -Encoding UTF8
$endingUI = Get-Content -LiteralPath $endingUIPath -Raw -Encoding UTF8
$silhouette = Get-Content -LiteralPath $silhouettePath -Raw -Encoding UTF8
$chase = Get-Content -LiteralPath $chasePath -Raw -Encoding UTF8
$exitDoor = Get-Content -LiteralPath $exitDoorPath -Raw -Encoding UTF8
$gameOver = Get-Content -LiteralPath $gameOverPath -Raw -Encoding UTF8
$clueData = Get-Content -LiteralPath $clueDataPath -Raw -Encoding UTF8
$journalManager = Get-Content -LiteralPath $journalManagerPath -Raw -Encoding UTF8
$journalUI = Get-Content -LiteralPath $journalUIPath -Raw -Encoding UTF8
$allNewCode = "$storyManager`n$lockSystem`n$endingUI`n$silhouette`n$chase`n$exitDoor`n$gameOver"
$allClueCode = "$clueData`n$journalManager`n$journalUI"

foreach ($code in @($storyManager, $lockSystem, $endingUI, $silhouette, $chase, $exitDoor, $gameOver)) {
    Assert-True ($code -match 'namespace\s+EscapeRoom') 'Every new story system must use namespace EscapeRoom.'
}

foreach ($phase in @('ClueCollection', 'TrueCulpritRevealed', 'KeyClueCollection', 'SuspectSelection', 'ChaseEscape')) {
    Assert-True ($storyManager -match $phase) "StoryProgressManager missing phase: $phase"
}

Assert-True ($storyManager -match 'clue_hasho_will' -and $storyManager -match 'clue_makeup_diary') 'StoryProgressManager must reveal true culprit from Hasho will and Jin diary.'
Assert-True ($storyManager -match 'key_clue_coldest_place' -and $storyManager -match 'key_clue_temperature_warning' -and $storyManager -match 'key_clue_fridge_scratches') 'StoryProgressManager must track all three key clues.'
Assert-True ($storyManager -match 'HasAllKeyClues') 'StoryProgressManager must expose key clue completion.'
Assert-True ($storyManager -match 'HasEscapeKey') 'StoryProgressManager must track escape key ownership.'
Assert-True ($storyManager -match 'OnPhaseChanged' -and $storyManager -match 'OnTrueCulpritRevealed' -and $storyManager -match 'OnEscapeKeyReady') 'StoryProgressManager must expose story events.'
Assert-True ($storyManager -match 'deductionTimer' -and $storyManager -match 'DeductionTimerExpired') 'StoryProgressManager must support deduction timer GameOver.'

Assert-True ($clueData -match 'clueID') 'ClueData must include a clueID field for story progression IDs.'
Assert-True ($clueData -match 'ClueCategory' -and $clueData -match 'KeyClue') 'ClueData must distinguish general clues and key clues.'
Assert-True ($journalManager -match 'keyClues' -and $journalManager -match 'KeyClues') 'ClueJournalManager must expose key clues separately.'
Assert-True ($journalUI -match 'KeyClue') 'ClueJournalUI must separate key clues in the collected evidence tab.'

Assert-True ($lockSystem -match 'storageLockerCode' -and $lockSystem -match 'fridgeMedicineBox') 'LockSystem must cover storage locker code and fridge medicine box.'
Assert-True ($lockSystem -match 'TryEnterCode' -and $lockSystem -match 'TryCollectEscapeKey') 'LockSystem must support code entry and escape key collection.'
Assert-True ($lockSystem -match 'HasAllKeyClues') 'LockSystem must gate escape key by all key clues.'

Assert-True ($endingUI -match 'JinSewoong' -and $endingUI -match 'BongTaehyeon' -and $endingUI -match 'MoonSumi' -and $endingUI -match 'OhSejin') 'EndingUI must include all four suspect choices.'
Assert-True ($endingUI -match 'OnCorrectSuspectSelected' -and $endingUI -match 'OnWrongSuspectSelected') 'EndingUI must expose correct/wrong selection events.'
Assert-True ($endingUI -match 'StartChase') 'EndingUI must start chase on correct answer.'
Assert-True ($endingUI -match 'WrongAnswer') 'EndingUI must route wrong answer to GameOver.'

Assert-True ($silhouette -match 'StageOne' -and $silhouette -match 'StageTwo' -and $silhouette -match 'StageThree' -and $silhouette -match 'Jumpscare') 'SilhouetteController must support staged silhouette behavior and jumpscare.'
Assert-True ($silhouette -match 'OnFlickerRequested') 'SilhouetteController must integrate with FlickerLight by event.'
Assert-True ($silhouette -match 'behindPlayer') 'SilhouetteController must support behind-player following before chase.'
Assert-True ($silhouette -match 'Collider\[\]\s+silhouetteColliders') 'SilhouetteController must manage silhouette colliders explicitly.'
Assert-True ($silhouette -match 'SetSilhouetteColliders\s*\(\s*false\s*\)') 'SilhouetteController must keep silhouette colliders disabled during deduction phases.'
Assert-True ($silhouette -match 'SetSilhouetteColliders\s*\(\s*true\s*\)') 'SilhouetteController must enable silhouette colliders only when materialized for chase.'

Assert-True ($chase -match 'NavMeshAgent') 'ChaseController must use NavMeshAgent.'
Assert-True ($chase -match 'chaseTimer' -and $chase -match 'ChaseTimerExpired') 'ChaseController must implement chase timer failure.'
Assert-True ($chase -match 'OnCaughtPlayer' -and $chase -match 'OnEscapeSucceeded') 'ChaseController must expose caught and escape events.'
Assert-True ($chase -match 'hasEscapeKey') 'ChaseController must require escape key for successful exit.'
Assert-True ($chase -match 'ShowSurvivalEnding') 'ChaseController must route successful escape to survival ending.'
Assert-True ($chase -match 'catchDistance\s*=\s*1f') 'ChaseController catch distance must be exactly 1m.'
Assert-True ($chase -match 'Vector3\.Distance\s*\([^\)]*\)\s*<\s*catchDistance') 'ChaseController must catch only when distance is less than 1m.'
Assert-True ($chase -match 'nearVignetteDistance\s*=\s*3f') 'ChaseController must start danger vignette within 3m.'
Assert-True ($chase -match 'heartbeatDistance\s*=\s*2f') 'ChaseController must request heartbeat within 2m.'
Assert-True ($chase -match 'chaseMoveSpeed\s*=\s*3\.8f' -and $chase -match '\.speed\s*=\s*chaseMoveSpeed') 'ChaseController must set Jin Sewoong NavMesh chase speed to 3.8.'
Assert-True ($chase -match 'OnVignetteChanged' -and $chase -match 'OnHeartbeatRequested' -and $chase -match 'OnBlackoutRequested') 'ChaseController must expose vignette, heartbeat, and blackout events.'
Assert-True ($chase -match 'PlayHeartbeat') 'ChaseController must request AudioManager heartbeat playback.'

Assert-True ($exitDoor -match 'class\s+EscapeExitDoor\s*:\s*MonoBehaviour') 'EscapeExitDoor must be a scene-placeable exit door component.'
Assert-True ($exitDoor -match 'TryOpenExit' -and $exitDoor -match 'HasEscapeKey' -and $exitDoor -match 'TryEscape') 'EscapeExitDoor must open only with the escape key and trigger escape.'

Assert-True ($gameOver -match 'PlayGameOver' -and $gameOver -match 'Restart' -and $gameOver -match 'MainMenu') 'GameOverUI must support game over, restart, and main menu.'
Assert-True ($gameOver -match 'ScreenSpaceOverlay') 'GameOverUI must use Screen Space Overlay canvas.'

Assert-True ($allNewCode -notmatch 'CursorController') 'New story systems must not reference CursorController.'
Assert-True ($allNewCode -notmatch 'Time\.timeScale') 'New story systems must not change Time.timeScale.'
Assert-True ($allNewCode -notmatch 'class\s+FlickerLight' -and $allNewCode -notmatch 'class\s+AudioManager') 'New story systems must not implement teammate systems.'
Assert-True ($allClueCode -notmatch 'Time\.timeScale') 'Clue systems must not change Time.timeScale.'

Write-Host 'Story chase flow checks passed.'
