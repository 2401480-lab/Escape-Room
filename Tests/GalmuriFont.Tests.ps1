$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$fontHelperPath = Join-Path $root 'Assets/Room02_Operating/Clues/FontHelper.cs'
$setupPath = Join-Path $root 'Assets/Room02_Operating/Scripts/Editor/KoreanFontSetup.cs'
$runtimeTextPaths = @(
    'Assets/Room02_Operating/Clues/ClueInteractable.cs',
    'Assets/Room02_Operating/Clues/ClueBoxInteractable.cs',
    'Assets/Room02_Operating/Clues/ClueJournalUI.cs',
    'Assets/Room02_Operating/Clues/CluePickupPopupUI.cs',
    'Assets/Room02_Operating/Clues/ControlHintUI.cs',
    'Assets/Room02_Operating/Clues/EndingUI.cs',
    'Assets/Room02_Operating/Clues/GameOverUI.cs',
    'Assets/Room02_Operating/Clues/IntroScenarioUI.cs',
    'Assets/Room02_Operating/Clues/SettingsUI.cs',
    'Assets/Room02_Operating/Clues/SuspectConfirmUI.cs',
    'Assets/Room02_Operating/Clues/TimerUI.cs'
) | ForEach-Object { Join-Path $root $_ }
$fontPaths = @(
    'Assets/Fonts/Galmuri11.ttf',
    'Assets/Fonts/Galmuri11-Bold.ttf',
    'Assets/Resources/Fonts/Galmuri11.ttf',
    'Assets/Resources/Fonts/Galmuri11-Bold.ttf'
) | ForEach-Object { Join-Path $root $_ }

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $fontHelperPath) 'Missing FontHelper.'
Assert-True (Test-Path -LiteralPath $setupPath) 'Missing KoreanFontSetup editor script.'
foreach ($path in $runtimeTextPaths) {
    Assert-True (Test-Path -LiteralPath $path) "Missing runtime text file: $path"
}

foreach ($path in $fontPaths) {
    Assert-True (Test-Path -LiteralPath $path) "Missing Galmuri font asset: $path"
    Assert-True (Test-Path -LiteralPath "$path.meta") "Missing Galmuri font meta: $path.meta"
}

$fontHelper = Get-Content -LiteralPath $fontHelperPath -Raw -Encoding UTF8
$setup = Get-Content -LiteralPath $setupPath -Raw -Encoding UTF8

Assert-True ($fontHelper -match 'Fonts/Galmuri11_TMP') 'FontHelper must prefer the Galmuri TMP font asset.'
Assert-True ($fontHelper -match 'Fonts/Galmuri11') 'FontHelper must be able to load the Galmuri TTF resource.'
Assert-True ($fontHelper -match 'TMP_FontAsset\.CreateFontAsset') 'FontHelper must create a runtime TMP asset from Galmuri when the TMP asset is not generated yet.'
Assert-True ($fontHelper -match 'RuntimeInitializeOnLoadMethod' -and $fontHelper -match 'SceneManager\.sceneLoaded') 'FontHelper must apply Galmuri after scene loads.'
Assert-True ($fontHelper -match 'FindObjectsByType<TextMeshProUGUI>' -and $fontHelper -match 'ApplyToLoadedTextObjects') 'FontHelper must sweep existing TMP UI text objects.'
Assert-True ($fontHelper -notmatch 'MalgunGothic_TMP') 'FontHelper must not load the old Malgun Gothic TMP asset.'

Assert-True ($setup -match 'Galmuri11\.ttf' -and $setup -match 'Galmuri11-Bold\.ttf') 'Font setup must use the Galmuri regular and bold TTF files.'
Assert-True ($setup -match 'Galmuri11_TMP\.asset' -and $setup -match 'Galmuri11-Bold_TMP\.asset') 'Font setup must generate Galmuri TMP font assets.'
Assert-True ($setup -match 'TMP_Settings' -and $setup -match 'm_defaultFontAsset') 'Font setup must update TMP default font settings.'
Assert-True ($setup -notmatch 'malgun\.ttf' -and $setup -notmatch 'MalgunGothic_TMP') 'Font setup must not point at the old Malgun Gothic assets.'

foreach ($path in $runtimeTextPaths) {
    $code = Get-Content -LiteralPath $path -Raw -Encoding UTF8
    Assert-True ($code -match 'FontHelper\.Apply' -or $code -match 'HorrorUITheme\.ApplyText') "Runtime-created TMP text must use Galmuri through FontHelper or HorrorUITheme in $path"
}

Write-Host 'Galmuri font checks passed.'
