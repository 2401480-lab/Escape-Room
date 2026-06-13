$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$settingsPath = Join-Path $root 'Assets/Room02_Operating/Clues/SettingsUI.cs'
$journalPath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueJournalUI.cs'
$timerPath = Join-Path $root 'Assets/Room02_Operating/Clues/TimerUI.cs'
$interactablePath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueInteractable.cs'
$bootstrapperPath = Join-Path $root 'Assets/Room02_Operating/Clues/HudRuntimeBootstrapper.cs'
$setupPath = Join-Path $root 'Assets/Room02_Operating/Clues/Editor/ClueSceneSetupTool.cs'
$scenePath = Join-Path $root 'Assets/Scenes/Scene_OperatingRoom.unity'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function U {
    param([int[]] $CodePoints)
    return -join ($CodePoints | ForEach-Object { [char] $_ })
}

foreach ($path in @($settingsPath, $journalPath, $timerPath, $interactablePath, $bootstrapperPath, $setupPath, $scenePath)) {
    Assert-True (Test-Path -LiteralPath $path) "Missing HUD file: $path"
}

$settings = Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8
$journal = Get-Content -LiteralPath $journalPath -Raw -Encoding UTF8
$timer = Get-Content -LiteralPath $timerPath -Raw -Encoding UTF8
$interactable = Get-Content -LiteralPath $interactablePath -Raw -Encoding UTF8
$bootstrapper = Get-Content -LiteralPath $bootstrapperPath -Raw -Encoding UTF8
$setup = Get-Content -LiteralPath $setupPath -Raw -Encoding UTF8
$scene = Get-Content -LiteralPath $scenePath -Raw -Encoding UTF8
$allHudCode = "$settings`n$journal`n$timer`n$interactable`n$bootstrapper"

$investigatePrompt = '[F] ' + (U 0xC870,0xC0AC,0xD558,0xAE30)
$settingsText = U 0xC124,0xC815
$volumeText = U 0xBCFC,0xB968
$sensitivityText = U 0xAC10,0xB3C4
$controlsText = U 0xC870,0xC791,0xBC95

Assert-True ($settings -match 'namespace\s+EscapeRoom' -and $settings -match 'class\s+SettingsUI\s*:\s*MonoBehaviour') 'SettingsUI must be an EscapeRoom MonoBehaviour.'
Assert-True ($settings -match 'ScreenSpaceOverlay' -and $settings -match 'HUD_Canvas') 'SettingsUI must use HUD_Canvas as Screen Space Overlay.'
Assert-True ($settings -match 'KeyCode\.Escape' -and $settings -match 'SettingsHudButton') 'SettingsUI must toggle from ESC and a top-right settings button.'
Assert-True ($settings -match 'Slider\s+volumeSlider' -and $settings -match 'Slider\s+sensitivitySlider') 'SettingsUI must include volume and sensitivity sliders.'
Assert-True ($settings.Contains($settingsText) -and $settings.Contains($volumeText) -and $settings.Contains($sensitivityText) -and $settings.Contains($controlsText)) 'SettingsUI must show Korean settings, volume, sensitivity, and controls labels.'
Assert-True ($settings -match 'VolumeSensitivityTabRoot' -and $settings -match 'ControlsTabRoot') 'SettingsUI must split settings into volume/sensitivity and controls tabs.'

Assert-True ($journal -match 'EvidenceHudButton' -and $journal -match 'SuspectHudButton') 'ClueJournalUI must create top-left evidence and suspect buttons.'
Assert-True ($journal -match 'KeyCode\.J' -and $journal -match 'KeyCode\.K') 'ClueJournalUI must toggle evidence with J and suspects with K.'
Assert-True ($journal -match 'HUD_Canvas') 'ClueJournalUI must attach UI to HUD_Canvas.'
Assert-True ($timer -match 'HUD_Canvas' -and $timer -match 'urgentThresholdSeconds\s*=\s*180f') 'TimerUI must live in HUD_Canvas and turn urgent after 3 minutes remain.'
Assert-True ($interactable.Contains($investigatePrompt) -and $interactable -match 'HUD_Canvas') 'ClueInteractable must show [F] investigate on HUD_Canvas.'
Assert-True ($bootstrapper -match 'RuntimeInitializeOnLoadMethod' -and $bootstrapper -match 'RuntimeInitializeLoadType\.AfterSceneLoad') 'HUD bootstrapper must run after any scene loads.'
Assert-True ($bootstrapper -match 'EnsureRuntimeObject<ClueJournalUI>' -and $bootstrapper -match 'EnsureRuntimeObject<TimerUI>' -and $bootstrapper -match 'EnsureRuntimeObject<SettingsUI>') 'HUD bootstrapper must create missing HUD runtime UI objects in any played scene.'
Assert-True ($bootstrapper -match 'EnsureRuntimeObject<StoryProgressManager>' -and $bootstrapper -match 'EnsureRuntimeObject<ClueJournalManager>') 'HUD bootstrapper must create required managers when testing a scene directly.'
Assert-True ($bootstrapper -match 'InputSystemUIInputModule' -and $bootstrapper -match 'EventSystem') 'HUD bootstrapper must create an EventSystem for HUD buttons.'

Assert-True ($setup -match 'EnsureRuntimeObject<SettingsUI>' -and $setup -match 'EnsureRuntimeObject<TimerUI>') 'Scene setup tool must ensure HUD runtime UI objects.'
Assert-True ($scene -match 'm_Name:\s+SettingsUI' -and $scene -match '8b158d7cbad6487e9163f6a33c4797b7') 'Scene_OperatingRoom must contain SettingsUI runtime object.'
Assert-True ($scene -match 'm_Name:\s+HUD_Canvas') 'Scene_OperatingRoom must contain HUD_Canvas.'

Assert-True ($allHudCode -notmatch 'Time\.timeScale') 'HUD UI must not change Time.timeScale.'
Assert-True ($allHudCode -notmatch 'CursorController') 'HUD UI must not touch CursorController.'

Write-Host 'HUD overlay UI checks passed.'
