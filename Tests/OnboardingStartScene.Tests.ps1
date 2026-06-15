$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$buildSettingsPath = Join-Path $root 'ProjectSettings/EditorBuildSettings.asset'
$onboardingScenePath = Join-Path $root 'Assets/Onboarding.unity'
$onboardingScriptPath = Join-Path $root 'Assets/_Shared/Scripts/OnboardingUI.cs'
$playModeStartPath = Join-Path $root 'Assets/_Shared/Scripts/Editor/OnboardingPlayModeStartScene.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $onboardingScenePath) 'Onboarding scene must exist.'
Assert-True (Test-Path -LiteralPath $onboardingScriptPath) 'OnboardingUI script must exist.'
Assert-True (Test-Path -LiteralPath $playModeStartPath) 'Editor play mode start scene helper must exist.'

$buildSettings = Get-Content -LiteralPath $buildSettingsPath -Raw -Encoding UTF8
$onboardingScene = Get-Content -LiteralPath $onboardingScenePath -Raw -Encoding UTF8
$onboardingScript = Get-Content -LiteralPath $onboardingScriptPath -Raw -Encoding UTF8
$playModeStart = Get-Content -LiteralPath $playModeStartPath -Raw -Encoding UTF8

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

Assert-True ($playModeStart -match 'OnboardingScenePath\s*=\s*"Assets/Onboarding\.unity"') 'Editor helper must target Assets/Onboarding.unity.'
Assert-True ($playModeStart -match 'EditorSceneManager\.playModeStartScene') 'Editor helper must set the Play button start scene.'
Assert-True ($playModeStart -match 'AssetDatabase\.LoadAssetAtPath<SceneAsset>') 'Editor helper must load the onboarding SceneAsset safely.'

Write-Host 'Onboarding start scene checks passed.'
