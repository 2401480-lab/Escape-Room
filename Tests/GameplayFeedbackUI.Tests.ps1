$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$vignettePath = Join-Path $root 'Assets/Room02_Operating/Clues/ProximityVignetteUI.cs'
$popupPath = Join-Path $root 'Assets/Room02_Operating/Clues/CluePickupPopupUI.cs'
$confirmPath = Join-Path $root 'Assets/Room02_Operating/Clues/SuspectConfirmUI.cs'
$endingPath = Join-Path $root 'Assets/Room02_Operating/Clues/EndingUI.cs'
$chasePath = Join-Path $root 'Assets/Room02_Operating/Clues/ChaseController.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function U {
    param([int[]] $CodePoints)
    return -join ($CodePoints | ForEach-Object { [char] $_ })
}

foreach ($path in @($vignettePath, $popupPath, $confirmPath, $endingPath, $chasePath)) {
    Assert-True (Test-Path -LiteralPath $path) "Missing required gameplay feedback UI file: $path"
}

$vignette = Get-Content -LiteralPath $vignettePath -Raw -Encoding UTF8
$popup = Get-Content -LiteralPath $popupPath -Raw -Encoding UTF8
$confirm = Get-Content -LiteralPath $confirmPath -Raw -Encoding UTF8
$ending = Get-Content -LiteralPath $endingPath -Raw -Encoding UTF8
$chase = Get-Content -LiteralPath $chasePath -Raw -Encoding UTF8
$allFeedbackCode = "$vignette`n$popup`n$confirm`n$ending`n$chase"
$evidenceCollected = U 0xC99D,0xAC70,0xB97C,0x0020,0xD655,0xBCF4,0xD588,0xB2E4
$reallyText = U 0xC815,0xB9D0
$culpritQuestion = U 0xBC94,0xC778,0xC785,0xB2C8,0xAE4C

foreach ($code in @($vignette, $popup, $confirm)) {
    Assert-True ($code -match 'namespace\s+EscapeRoom') 'New gameplay feedback UI must use namespace EscapeRoom.'
    Assert-True ($code -match 'ScreenSpaceOverlay') 'New gameplay feedback UI must use Screen Space Overlay canvas.'
}

Assert-True ($vignette -match 'class\s+ProximityVignetteUI\s*:\s*MonoBehaviour') 'ProximityVignetteUI must be a MonoBehaviour.'
Assert-True ($vignette -match 'Image\s+vignetteImage') 'ProximityVignetteUI must render through an Image component.'
Assert-True ($vignette -match 'OnVignetteChanged\.AddListener') 'ProximityVignetteUI must subscribe to ChaseController vignette distance/intensity updates.'
Assert-True ($vignette -match 'UpdateAlpha\s*\(\s*float\s+\w+\s*\)') 'ProximityVignetteUI must update alpha from ChaseController values.'
Assert-True ($vignette -match '0\.3f' -and $vignette -match '0\.6f') 'ProximityVignetteUI must support 3m alpha 0.3 and 2m alpha 0.6 levels.'
Assert-True ($vignette -match 'Radial360|radial360|preserveAspect') 'ProximityVignetteUI must configure the image as a circular edge mask.'
Assert-True ($chase -match 'OnVignetteChanged\?\.Invoke\s*\(\s*0\.3f\s*\)' -and $chase -match 'OnVignetteChanged\?\.Invoke\s*\(\s*0\.6f\s*\)') 'ChaseController must emit exact vignette alpha levels for proximity thresholds.'

Assert-True ($popup -match 'class\s+CluePickupPopupUI\s*:\s*MonoBehaviour') 'CluePickupPopupUI must be a MonoBehaviour.'
Assert-True ($popup -match 'OnClueAdded\s*\+=' -and $popup -match 'OnClueAdded\s*-=' ) 'CluePickupPopupUI must subscribe and unsubscribe from ClueJournalManager.OnClueAdded.'
Assert-True ($popup.Contains($evidenceCollected)) 'CluePickupPopupUI must show the collected evidence text.'
Assert-True ($popup -match 'displayDuration\s*=\s*2f') 'CluePickupPopupUI must display for 2 seconds.'
Assert-True ($popup -match 'CanvasGroup' -and $popup -match 'alpha') 'CluePickupPopupUI must fade out through CanvasGroup alpha.'
Assert-True ($popup -match 'anchorMin\s*=\s*new\s+Vector2\s*\(\s*0\.5f\s*,\s*0f\s*\)' -and $popup -match 'anchorMax\s*=\s*new\s+Vector2\s*\(\s*0\.5f\s*,\s*0f\s*\)') 'CluePickupPopupUI must appear at bottom center.'

Assert-True ($confirm -match 'class\s+SuspectConfirmUI\s*:\s*MonoBehaviour') 'SuspectConfirmUI must be a MonoBehaviour.'
Assert-True ($confirm -match 'Show\s*\(\s*EndingUI\s+\w+\s*,\s*SuspectChoice\s+\w+\s*,\s*string\s+\w+\s*\)') 'SuspectConfirmUI must be callable from EndingUI with selected suspect.'
Assert-True ($confirm -match 'ConfirmSuspect\s*\(') 'SuspectConfirmUI yes button must call EndingUI.ConfirmSuspect().'
Assert-True ($confirm -match 'Button\s+yesButton' -and $confirm -match 'Button\s+noButton') 'SuspectConfirmUI must expose yes/no buttons.'
Assert-True ($confirm.Contains($reallyText) -and $confirm.Contains($culpritQuestion)) 'SuspectConfirmUI must display the confirmation question.'

Assert-True ($ending -match 'SuspectConfirmUI') 'EndingUI must call SuspectConfirmUI before resolving a suspect choice.'
Assert-True ($ending -match 'ConfirmSuspect\s*\(') 'EndingUI must expose ConfirmSuspect() for the confirmation popup.'
Assert-True ($ending -match 'pendingSuspect') 'EndingUI must remember the selected suspect until confirmation.'
Assert-True ($ending -match 'wrongAnswerUsed') 'EndingUI must own the one-wrong-answer limit.'
Assert-True ($ending -match 'SuspectChoice') 'EndingUI must use a suspect choice enum for confirmation flow.'

Assert-True ($allFeedbackCode -notmatch 'Time\.timeScale') 'Gameplay feedback UI must not change Time.timeScale.'
Assert-True ($allFeedbackCode -notmatch 'CursorController') 'Gameplay feedback UI must not touch CursorController.'

Write-Host 'Gameplay feedback UI checks passed.'
