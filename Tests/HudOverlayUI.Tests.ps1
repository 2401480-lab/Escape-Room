$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$settingsPath = Join-Path $root 'Assets/Room02_Operating/Clues/SettingsUI.cs'
$journalPath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueJournalUI.cs'
$timerPath = Join-Path $root 'Assets/Room02_Operating/Clues/TimerUI.cs'
$controlHintPath = Join-Path $root 'Assets/Room02_Operating/Clues/ControlHintUI.cs'
$interactablePath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueInteractable.cs'
$bootstrapperPath = Join-Path $root 'Assets/Room02_Operating/Clues/HudRuntimeBootstrapper.cs'
$setupPath = Join-Path $root 'Assets/Room02_Operating/Clues/Editor/ClueSceneSetupTool.cs'
$scenePath = Join-Path $root 'Assets/Room02_Operating/Scenes/Show.unity'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function U {
    param([int[]] $CodePoints)
    return -join ($CodePoints | ForEach-Object { [char] $_ })
}

foreach ($path in @($settingsPath, $journalPath, $timerPath, $controlHintPath, $interactablePath, $bootstrapperPath, $setupPath, $scenePath)) {
    Assert-True (Test-Path -LiteralPath $path) "Missing HUD file: $path"
}

$settings = Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8
$journal = Get-Content -LiteralPath $journalPath -Raw -Encoding UTF8
$timer = Get-Content -LiteralPath $timerPath -Raw -Encoding UTF8
$controlHint = Get-Content -LiteralPath $controlHintPath -Raw -Encoding UTF8
$interactable = Get-Content -LiteralPath $interactablePath -Raw -Encoding UTF8
$bootstrapper = Get-Content -LiteralPath $bootstrapperPath -Raw -Encoding UTF8
$setup = Get-Content -LiteralPath $setupPath -Raw -Encoding UTF8
$scene = Get-Content -LiteralPath $scenePath -Raw -Encoding UTF8
$allHudCode = "$settings`n$journal`n$timer`n$controlHint`n$interactable`n$bootstrapper"

$investigatePrompt = '[F] ' + (U 0xC870,0xC0AC,0xD558,0xAE30)
$settingsText = U 0xC124,0xC815
$volumeText = U 0xBCFC,0xB968
$sensitivityText = U 0xAC10,0xB3C4
$controlsText = U 0xC870,0xC791,0xBC95
$investigationNoteText = U 0xC218,0xC0AC,0x0020,0xB178,0xD2B8
$suspectNotebookText = (U 0xC6A9,0xC758,0xC790) + ' ' + (U 0xC218,0xCCA9)
$culpritChaseText = (U 0xBC94,0xC778) + (U 0xCC3E,0xAE30) + ' (G)'
$popupDismissText = (U 0xD31D,0xC5C5) + ' ' + (U 0xB2EB,0xAE30)
$openDoorText = (U 0xBB38) + (U 0xC5F4,0xAE30)
$runFastText = (U 0xBE68,0xB9AC) + ' ' + (U 0xB2EC,0xB9AC,0xAE30)

Assert-True ($settings -match 'namespace\s+EscapeRoom' -and $settings -match 'class\s+SettingsUI\s*:\s*MonoBehaviour') 'SettingsUI must be an EscapeRoom MonoBehaviour.'
Assert-True ($settings -match 'ScreenSpaceOverlay' -and $settings -match 'HUD_Canvas') 'SettingsUI must use HUD_Canvas as Screen Space Overlay.'
Assert-True ($settings -match 'KeyCode\.Escape' -and $settings -match 'SettingsHudButton') 'SettingsUI must toggle from ESC and a top-right settings button.'
Assert-True ($settings -match 'IsClueJournalHandlingEscape\s*\(' -and $settings -match 'ClueJournalPanel' -and $settings -match 'LastJournalCloseFrame') 'SettingsUI must not open settings on the same ESC used to close the clue journal.'
Assert-True ($settings -match 'Slider\s+volumeSlider' -and $settings -match 'Slider\s+sensitivitySlider') 'SettingsUI must include volume and sensitivity sliders.'
Assert-True ($settings.Contains($settingsText) -and $settings.Contains($volumeText) -and $settings.Contains($sensitivityText) -and $settings.Contains($controlsText)) 'SettingsUI must show Korean settings, volume, sensitivity, and controls labels.'
Assert-True ($settings -match 'VolumeSensitivityTabRoot' -and $settings -match 'ControlsTabRoot') 'SettingsUI must split settings into volume/sensitivity and controls tabs.'
Assert-True ($settings -match 'CreateControlRow' -and $settings.Contains($investigationNoteText) -and $settings.Contains($suspectNotebookText) -and $settings.Contains($culpritChaseText) -and $settings.Contains($popupDismissText) -and $settings.Contains($openDoorText) -and $settings.Contains($runFastText)) 'SettingsUI controls tab must organize keyboard controls into readable rows.'
foreach ($controlToken in @('WASD', 'Left Shift', 'Mouse', 'E', 'F', 'J / Tab', 'K', 'G', 'ESC')) {
    Assert-True ($settings.Contains($controlToken)) "SettingsUI controls tab missing control token: $controlToken"
}

Assert-True ($journal -match 'EvidenceHudButton' -and $journal -match 'SuspectHudButton') 'ClueJournalUI must create top-left evidence and suspect buttons.'
Assert-True ($journal -match 'KeyCode\.J' -and $journal -match 'KeyCode\.K') 'ClueJournalUI must toggle evidence with J and suspects with K.'
Assert-True ($journal -match 'HUD_Canvas') 'ClueJournalUI must attach UI to HUD_Canvas.'
Assert-True ($timer -match 'HUD_Canvas' -and $timer -match 'urgentThresholdSeconds\s*=\s*180f') 'TimerUI must live in HUD_Canvas and turn urgent after 3 minutes remain.'
Assert-True ($controlHint -match 'KeyboardControlHintPanel' -and $controlHint.Contains($openDoorText) -and $controlHint.Contains($runFastText)) 'ControlHintUI must show keyboard-style run and door prompts on the HUD.'
Assert-True ($interactable.Contains($investigatePrompt) -and $interactable -match 'HUD_Canvas') 'ClueInteractable must show [F] investigate on HUD_Canvas.'
Assert-True ($bootstrapper -match 'RuntimeInitializeOnLoadMethod' -and $bootstrapper -match 'RuntimeInitializeLoadType\.AfterSceneLoad') 'HUD bootstrapper must initialize after the first scene loads.'
Assert-True ($bootstrapper -match 'SceneManager\.sceneLoaded' -and $bootstrapper -match 'HandleSceneLoaded') 'HUD bootstrapper must listen for later scene loads so Onboarding can transition into Room02.'
Assert-True ($bootstrapper -match 'IsRoom02Scene' -and $bootstrapper -match 'scene\.name\s*==\s*"Show"') 'HUD bootstrapper must limit Room02 HUD creation to the Show scene.'
Assert-True ($bootstrapper -match 'BootstrapRoom02Runtime' -and $bootstrapper -match 'if\s*\(\s*!IsRoom02Scene') 'HUD bootstrapper must skip Onboarding and other non-Room02 scenes.'
Assert-True ($bootstrapper -match 'EnsureRuntimeObject<ClueJournalUI>' -and $bootstrapper -match 'EnsureRuntimeObject<TimerUI>' -and $bootstrapper -match 'EnsureRuntimeObject<ControlHintUI>' -and $bootstrapper -match 'EnsureRuntimeObject<SettingsUI>') 'HUD bootstrapper must create missing HUD runtime UI objects in the Room02 Show scene.'
Assert-True ($bootstrapper -match 'EnsureRuntimeObject<EndingUI>') 'HUD bootstrapper must create EndingUI so the culprit guess button can appear in Show.'
Assert-True ($bootstrapper -match 'EnsureRuntimeObject<StoryProgressManager>' -and $bootstrapper -match 'EnsureRuntimeObject<ClueJournalManager>') 'HUD bootstrapper must create required managers when testing a scene directly.'
Assert-True ($bootstrapper -match 'InputSystemUIInputModule' -and $bootstrapper -match 'EventSystem') 'HUD bootstrapper must create an EventSystem for HUD buttons.'

Assert-True ($setup -match 'EnsureRuntimeObject<SettingsUI>' -and $setup -match 'EnsureRuntimeObject<TimerUI>') 'Scene setup tool must ensure HUD runtime UI objects.'
Assert-True ($scene -match 'm_Name:\s+Main Camera') 'Show must keep its original playable camera.'
Assert-True ($scene -notmatch 'm_Name:\s+SettingsUI' -and $scene -notmatch 'm_Name:\s+HUD_Canvas') 'Show must rely on runtime HUD creation instead of embedding temporary HUD objects.'

Assert-True ($allHudCode -notmatch 'Time\.timeScale') 'HUD UI must not change Time.timeScale.'
Assert-True ($allHudCode -notmatch 'CursorController') 'HUD UI must not touch CursorController.'

Write-Host 'HUD overlay UI checks passed.'
