$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$buildSettingsPath = Join-Path $root 'ProjectSettings/EditorBuildSettings.asset'
$onboardingScenePath = Join-Path $root 'Assets/Onboarding.unity'
$onboardingScriptPath = Join-Path $root 'Assets/_Shared/Scripts/OnboardingUI.cs'
$roomLoaderPath = Join-Path $root 'Assets/_Shared/Scripts/RoomLoader.cs'
$playModeStartPath = Join-Path $root 'Assets/_Shared/Scripts/Editor/OnboardingPlayModeStartScene.cs'
$backgroundPath = Join-Path $root 'Assets/_Shared/Resources/Onboarding/HospitalHorrorBackground.png'
$backgroundMetaPath = Join-Path $root 'Assets/_Shared/Resources/Onboarding/HospitalHorrorBackground.png.meta'
$onboardingBgmPath = Join-Path $root 'Assets/_Shared/Resources/Audio/BGM/OnboardingHospitalAmbience.aif'
$onboardingBgmMetaPath = Join-Path $root 'Assets/_Shared/Resources/Audio/BGM/OnboardingHospitalAmbience.aif.meta'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function U {
    param([int[]] $CodePoints)
    return -join ($CodePoints | ForEach-Object { [char] $_ })
}

Assert-True (Test-Path -LiteralPath $onboardingScenePath) 'Onboarding scene must exist.'
Assert-True (Test-Path -LiteralPath $onboardingScriptPath) 'OnboardingUI script must exist.'
Assert-True (Test-Path -LiteralPath $roomLoaderPath) 'RoomLoader script must exist.'
Assert-True (Test-Path -LiteralPath $playModeStartPath) 'Editor play mode start scene helper must exist.'
Assert-True (Test-Path -LiteralPath $backgroundPath) 'Onboarding hospital horror background image must exist under _Shared Resources.'
Assert-True (Test-Path -LiteralPath $backgroundMetaPath) 'Onboarding hospital horror background image meta must exist.'
Assert-True (Test-Path -LiteralPath $onboardingBgmPath) 'Onboarding looping BGM clip must exist under _Shared Resources.'
Assert-True (Test-Path -LiteralPath $onboardingBgmMetaPath) 'Onboarding looping BGM clip meta must exist.'

$buildSettings = Get-Content -LiteralPath $buildSettingsPath -Raw -Encoding UTF8
$onboardingScene = Get-Content -LiteralPath $onboardingScenePath -Raw -Encoding UTF8
$onboardingScript = Get-Content -LiteralPath $onboardingScriptPath -Raw -Encoding UTF8
$roomLoader = Get-Content -LiteralPath $roomLoaderPath -Raw -Encoding UTF8
$playModeStart = Get-Content -LiteralPath $playModeStartPath -Raw -Encoding UTF8
$backgroundMeta = Get-Content -LiteralPath $backgroundMetaPath -Raw -Encoding UTF8
$onboardingBgmMeta = Get-Content -LiteralPath $onboardingBgmMetaPath -Raw -Encoding UTF8
$gameStart = U 0xAC8C,0xC784,0x0020,0xC2DC,0xC791
$gameSettings = U 0xAC8C,0xC784,0x0020,0xC124,0xC815
$gameDescription = U 0xAC8C,0xC784,0x0020,0xC124,0xBA85

$onboardingBuildIndex = $buildSettings.IndexOf('path: Assets/Onboarding.unity')
$showBuildIndex = $buildSettings.IndexOf('path: Assets/Room02_Operating/Scenes/Show.unity')

Assert-True ($onboardingBuildIndex -ge 0) 'Build Settings must include Assets/Onboarding.unity.'
Assert-True ($showBuildIndex -ge 0) 'Build Settings must include the Room02 Show scene so Onboarding can load it.'
Assert-True ($onboardingBuildIndex -lt $showBuildIndex) 'Build Settings must place Onboarding before Show so the game starts on the start page.'
Assert-True ($buildSettings -notmatch 'Scene_OperatingRoom') 'Build Settings must not point to Scene_OperatingRoom.'

Assert-True ($onboardingScene -match 'roomSceneName:\s*Show') 'Onboarding scene must serialize roomSceneName as Show.'
Assert-True ($onboardingScript -match 'private\s+string\s+roomSceneName\s*=\s*"Show"') 'OnboardingUI must load Show by default.'
Assert-True ($onboardingScript -match 'SceneManager\.LoadScene\s*\(\s*roomSceneName\s*\)') 'OnboardingUI must load the configured room scene.'
Assert-True ($onboardingScript -notmatch 'Scene_OperatingRoom') 'OnboardingUI must not load Scene_OperatingRoom.'
Assert-True ($onboardingScript -match 'OnStartButtonClicked\s*\(\s*\)[\s\S]*?LoadRoom\s*\(\s*\)') 'Onboarding start button must load Show directly so Room02 owns the intro.'
Assert-True ($onboardingScript -notmatch 'GeneratedIntroPanel' -and $onboardingScript -notmatch 'ShowGeneratedIntro' -and $onboardingScript -notmatch 'BuildGeneratedIntroPanel') 'OnboardingUI must not create its own intro panel; Room02 IntroScenarioUI owns the story intro.'
Assert-True ($onboardingScript -notmatch 'IntroScenarioUI' -and $onboardingScript -notmatch 'IntroScenarioPanel') 'OnboardingUI must not hide or manage Room02 intro UI objects.'
Assert-True ($roomLoader -match 'ROOM02_SCENE\s*=\s*"Show"') 'Shared RoomLoader room 2 route must also load Show, the Room02 scene with flashlight.'
Assert-True ($roomLoader -notmatch 'ROOM02_SCENE\s*=\s*"Scene_OperatingRoom"') 'Shared RoomLoader must not send room 2 to Scene_OperatingRoom.'
Assert-True ($onboardingScript -match 'BackgroundResourcePath\s*=\s*"Onboarding/HospitalHorrorBackground"') 'OnboardingUI must load the hospital horror background from Resources.'
Assert-True ($onboardingScript -match 'Resources\.Load<Sprite>\s*\(\s*BackgroundResourcePath\s*\)') 'OnboardingUI must load the onboarding background sprite.'
Assert-True ($onboardingScript -match 'OnboardingHospitalHorrorBackground') 'OnboardingUI must create a named hospital horror background object.'
Assert-True ($onboardingScript -match 'OnboardingDarkVignetteOverlay') 'OnboardingUI must add a dark overlay over the background image.'
Assert-True ($onboardingScript -match 'BackgroundMusicResourcePath\s*=\s*"Audio/BGM/OnboardingHospitalAmbience"') 'OnboardingUI must load its menu BGM from a shared Resources audio path.'
Assert-True ($onboardingScript -match 'Resources\.Load<AudioClip>\s*\(\s*BackgroundMusicResourcePath\s*\)') 'OnboardingUI must load the onboarding BGM AudioClip.'
Assert-True ($onboardingScript -match 'AudioSource\s+menuAudioSource') 'OnboardingUI must keep a dedicated menu AudioSource for onboarding BGM.'
Assert-True ($onboardingScript -match '\.loop\s*=\s*true' -and $onboardingScript -match '\.spatialBlend\s*=\s*0f') 'Onboarding menu BGM must loop as 2D background audio.'
Assert-True ($onboardingScript -match 'PlayOnboardingMusic\s*\(' -and $onboardingScript -match 'StopOnboardingMusic\s*\(') 'OnboardingUI must start menu BGM on the menu and stop it when leaving onboarding.'
Assert-True ($onboardingScript -notmatch 'Audio/Intro/guts_and_gore_19' -and $onboardingScript -notmatch 'guts_and_gore') 'Onboarding menu BGM must not use the intro-only SFX.'
Assert-True ($onboardingScript.Contains($gameStart) -or $onboardingScript -match '\\uAC8C\\uC784\s+\\uC2DC\\uC791') 'OnboardingUI must label the start button as game start.'
Assert-True ($onboardingScript.Contains($gameSettings) -or $onboardingScript -match '\\uAC8C\\uC784\s+\\uC124\\uC815') 'OnboardingUI must label the settings button as game settings.'
Assert-True (-not $onboardingScript.Contains($gameDescription) -and $onboardingScript -notmatch '\\uAC8C\\uC784\s+\\uC124\\uBA85') 'OnboardingUI main menu must not keep the old game description label.'
Assert-True ($onboardingScript -match 'characterSpacing' -and $onboardingScript -match 'EnsureOutline') 'OnboardingUI must apply stronger horror typography styling.'

Assert-True ($playModeStart -match 'OnboardingScenePath\s*=\s*"Assets/Onboarding\.unity"') 'Editor helper must target Assets/Onboarding.unity.'
Assert-True ($playModeStart -match 'EditorSceneManager\.playModeStartScene') 'Editor helper must set the Play button start scene.'
Assert-True ($playModeStart -match 'AssetDatabase\.LoadAssetAtPath<SceneAsset>') 'Editor helper must load the onboarding SceneAsset safely.'

Assert-True ($backgroundMeta -match 'textureType:\s*8') 'Onboarding background texture must import as a Sprite.'
Assert-True ($backgroundMeta -match 'spriteMode:\s*1') 'Onboarding background texture must be a single sprite.'
Assert-True ($onboardingBgmMeta -match 'AudioImporter:' -and $onboardingBgmMeta -match 'preloadAudioData:\s*1') 'Onboarding BGM must import as an AudioClip with preloaded audio data.'

Write-Host 'Onboarding start scene checks passed.'
