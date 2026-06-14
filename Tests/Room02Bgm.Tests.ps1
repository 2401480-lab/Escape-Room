$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$scenePath = Join-Path $root 'Assets/Room02_Operating/Scenes/Scene_OperatingRoom.unity'
$bgmScriptPath = Join-Path $root 'Assets/Room02_Operating/Audio/Room02BgmPlayer.cs'
$bgmScriptMetaPath = Join-Path $root 'Assets/Room02_Operating/Audio/Room02BgmPlayer.cs.meta'
$clipPath = Join-Path $root 'Assets/Room02_Operating/Audio/music/darkness/dk-theroom.aif'
$clipMetaPath = Join-Path $root 'Assets/Room02_Operating/Audio/music/darkness/dk-theroom.aif.meta'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $scenePath) 'Missing Scene_OperatingRoom scene.'
Assert-True (Test-Path -LiteralPath $bgmScriptPath) 'Missing Room02 BGM player script.'
Assert-True (Test-Path -LiteralPath $bgmScriptMetaPath) 'Missing Room02 BGM player script meta file.'
Assert-True (Test-Path -LiteralPath $clipPath) 'Missing selected Room02 BGM audio clip.'
Assert-True (Test-Path -LiteralPath $clipMetaPath) 'Missing selected Room02 BGM audio clip meta file.'

$scene = Get-Content -LiteralPath $scenePath -Raw -Encoding UTF8
$script = Get-Content -LiteralPath $bgmScriptPath -Raw -Encoding UTF8
$scriptGuid = ((Select-String -LiteralPath $bgmScriptMetaPath -Pattern '^guid:').Line -replace '^guid:\s*', '').Trim()
$clipGuid = ((Select-String -LiteralPath $clipMetaPath -Pattern '^guid:').Line -replace '^guid:\s*', '').Trim()

Assert-True ($script -match 'namespace\s+EscapeRoom') 'Room02 BGM player must stay in the EscapeRoom namespace.'
Assert-True ($script -match 'class\s+Room02BgmPlayer\s*:\s*MonoBehaviour') 'Room02 BGM player must be a MonoBehaviour.'
Assert-True ($script -match 'AudioClip\s+bgmClip') 'Room02 BGM player must expose a BGM AudioClip.'
Assert-True ($script -match 'AddComponent<AudioSource>\s*\(') 'Room02 BGM player must create an AudioSource when one is missing.'
Assert-True ($script -match '\.loop\s*=\s*loop' -and $script -match '\.spatialBlend\s*=\s*0f') 'Room02 BGM must play as looping 2D background audio.'
Assert-True ($script -match '\.Play\s*\(') 'Room02 BGM player must start playback.'

Assert-True ($scene -match 'm_Name:\s+Room02_BGM') 'Scene_OperatingRoom must contain the Room02_BGM object.'
Assert-True ($scene -match [regex]::Escape("guid: $scriptGuid")) 'Scene_OperatingRoom must reference Room02BgmPlayer.'
Assert-True ($scene -match [regex]::Escape("guid: $clipGuid")) 'Scene_OperatingRoom must reference the selected BGM clip.'
Assert-True ($scene -match 'volume:\s+0\.35') 'Room02 BGM volume must be set to a restrained default.'
Assert-True ($scene -match 'loop:\s+1') 'Room02 BGM must loop.'

Write-Host 'Room02 BGM checks passed.'
