$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$scenePath = Join-Path $root 'Assets/Room02_Operating/Scenes/Show.unity'
$bgmScriptPath = Join-Path $root 'Assets/Room02_Operating/Audio/Room02BgmPlayer.cs'
$bgmScriptMetaPath = Join-Path $root 'Assets/Room02_Operating/Audio/Room02BgmPlayer.cs.meta'
$bootstrapperPath = Join-Path $root 'Assets/Room02_Operating/Clues/HudRuntimeBootstrapper.cs'
$clipPath = Join-Path $root 'Assets/Room02_Operating/Audio/music/darkness/dk-atmosphere.aif'
$clipMetaPath = Join-Path $root 'Assets/Room02_Operating/Audio/music/darkness/dk-atmosphere.aif.meta'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $scenePath) 'Missing Show gameplay scene.'
Assert-True (Test-Path -LiteralPath $bgmScriptPath) 'Missing Room02 BGM player script.'
Assert-True (Test-Path -LiteralPath $bgmScriptMetaPath) 'Missing Room02 BGM player script meta file.'
Assert-True (Test-Path -LiteralPath $bootstrapperPath) 'Missing HUD runtime bootstrapper.'
Assert-True (Test-Path -LiteralPath $clipPath) 'Missing selected Room02 BGM audio clip.'
Assert-True (Test-Path -LiteralPath $clipMetaPath) 'Missing selected Room02 BGM audio clip meta file.'

$scene = Get-Content -LiteralPath $scenePath -Raw -Encoding UTF8
$script = Get-Content -LiteralPath $bgmScriptPath -Raw -Encoding UTF8
$bootstrapper = Get-Content -LiteralPath $bootstrapperPath -Raw -Encoding UTF8
$scriptGuid = ((Select-String -LiteralPath $bgmScriptMetaPath -Pattern '^guid:').Line -replace '^guid:\s*', '').Trim()
$clipGuid = ((Select-String -LiteralPath $clipMetaPath -Pattern '^guid:').Line -replace '^guid:\s*', '').Trim()

Assert-True ($script -match 'namespace\s+EscapeRoom') 'Room02 BGM player must stay in the EscapeRoom namespace.'
Assert-True ($script -match 'class\s+Room02BgmPlayer\s*:\s*MonoBehaviour') 'Room02 BGM player must be a MonoBehaviour.'
Assert-True ($script -match 'AudioClip\s+bgmClip') 'Room02 BGM player must expose a BGM AudioClip.'
Assert-True ($script -match 'resourcesClipPath\s*=\s*"Room02_Audio/dk-atmosphere"') 'Room02 BGM player must know the Resources fallback clip path.'
Assert-True ($script -match 'editorClipAssetPath\s*=\s*"Assets/Room02_Operating/Audio/music/darkness/dk-atmosphere\.aif"') 'Room02 BGM player must know the editor asset fallback path.'
Assert-True ($script -match 'ResolveBgmClip\s*\(' -and $script -match 'Resources\.Load<AudioClip>' -and $script -match 'AssetDatabase\.LoadAssetAtPath<AudioClip>') 'Room02 BGM player must resolve its clip at Play time even if the scene reference is missing.'
Assert-True ($script -match 'AddComponent<AudioSource>\s*\(') 'Room02 BGM player must create an AudioSource when one is missing.'
Assert-True ($script -match '\.loop\s*=\s*loop' -and $script -match '\.spatialBlend\s*=\s*0f') 'Room02 BGM must play as looping 2D background audio.'
Assert-True ($script -match '\.mute\s*=\s*false' -and $script -match '\.enabled\s*=\s*true') 'Room02 BGM player must force the runtime AudioSource into an audible enabled state.'
Assert-True ($script -match '\.Play\s*\(') 'Room02 BGM player must start playback.'
Assert-True ($script -match 'OnEnable\s*\(' -and $script -match 'TryPlay\s*\(') 'Room02 BGM player must retry playback when enabled.'
Assert-True ($script -match 'Update\s*\(' -and $script -match '!audioSource\.isPlaying') 'Room02 BGM player must recover if playback stops during play.'
Assert-True ($script -match 'volume\s*=\s*0\.65f') 'Room02 BGM default volume must be audible when Play starts.'
Assert-True ($bootstrapper -match 'EnsureRuntimeObject<Room02BgmPlayer>\s*\(\s*"Room02_BGM"\s*\)') 'HUD bootstrapper must create Room02_BGM automatically when Play starts.'

Assert-True ($scene -match 'm_Name:\s+Room02_BGM') 'Show gameplay scene must contain the Room02_BGM object.'
Assert-True ($scene -match [regex]::Escape("guid: $scriptGuid")) 'Show gameplay scene must reference Room02BgmPlayer.'
Assert-True ($scene -match [regex]::Escape("guid: $clipGuid")) 'Show gameplay scene must reference the selected BGM clip.'
Assert-True ($scene -match 'volume:\s+0\.65') 'Room02 BGM volume must be high enough to hear when Play starts.'
Assert-True ($scene -match 'loop:\s+1') 'Room02 BGM must loop.'
Assert-True ($scene -match 'AudioSource:[\s\S]*?m_GameObject:\s*\{fileID:\s*2003000000\}[\s\S]*?m_audioClip:\s*\{fileID:\s*8300000,\s*guid:\s*' + [regex]::Escape($clipGuid)) 'Room02_BGM must have an AudioSource with the selected clip in the scene.'
Assert-True ($scene -match 'AudioSource:[\s\S]*?m_PlayOnAwake:\s+1' -and $scene -match 'AudioSource:[\s\S]*?m_Volume:\s+0\.65' -and $scene -match 'AudioSource:[\s\S]*?Loop:\s+1') 'Room02_BGM AudioSource must play on awake, stay audible, and loop.'

Write-Host 'Room02 BGM checks passed.'
