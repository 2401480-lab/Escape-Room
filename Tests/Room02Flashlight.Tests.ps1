$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$scenePath = Join-Path $root 'Assets/Room02_Operating/Scenes/Show.unity'
$flashlightPath = Join-Path $root 'Assets/Room02_Operating/Clues/Room02FlashlightController.cs'
$flashlightMetaPath = Join-Path $root 'Assets/Room02_Operating/Clues/Room02FlashlightController.cs.meta'
$bootstrapperPath = Join-Path $root 'Assets/Room02_Operating/Clues/HudRuntimeBootstrapper.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $scenePath) 'Missing Show gameplay scene.'
Assert-True (Test-Path -LiteralPath $flashlightPath) 'Missing Room02 flashlight controller script.'
Assert-True (Test-Path -LiteralPath $flashlightMetaPath) 'Missing Room02 flashlight controller meta file.'
Assert-True (Test-Path -LiteralPath $bootstrapperPath) 'Missing HUD runtime bootstrapper.'

$scene = Get-Content -LiteralPath $scenePath -Raw -Encoding UTF8
$flashlight = Get-Content -LiteralPath $flashlightPath -Raw -Encoding UTF8
$bootstrapper = Get-Content -LiteralPath $bootstrapperPath -Raw -Encoding UTF8
$flashlightGuid = ((Select-String -LiteralPath $flashlightMetaPath -Pattern '^guid:').Line -replace '^guid:\s*', '').Trim()

Assert-True ($flashlight -match 'namespace\s+EscapeRoom') 'Room02 flashlight must stay in the EscapeRoom namespace.'
Assert-True ($flashlight -match 'class\s+Room02FlashlightController\s*:\s*MonoBehaviour') 'Room02 flashlight controller must be a MonoBehaviour.'
Assert-True ($flashlight -match 'LightType\.Spot') 'Room02 flashlight must use a real spot light.'
Assert-True ($flashlight -match 'spotAngle\s*=\s*36f' -and $flashlight -match 'innerSpotAngle\s*=\s*18f') 'Room02 flashlight must use a tight circular beam.'
Assert-True ($flashlight -match 'Camera\.main' -and $flashlight -match 'LateUpdate') 'Room02 flashlight must follow the active game camera.'
Assert-True ($flashlight -match 'RawImage\s+flashlightMask' -and $flashlight -match 'Texture2D' -and $flashlight -match 'CreateMaskTexture') 'Room02 flashlight must draw a screen-space circular darkness mask.'
Assert-True ($flashlight -match 'raycastTarget\s*=\s*false') 'Flashlight overlay must not block clue popup clicks.'
Assert-True ($flashlight -match 'RenderSettings\.ambientMode' -and $flashlight -match 'RenderSettings\.ambientLight' -and $flashlight -match 'RenderSettings\.fog') 'Room02 flashlight must darken the room atmosphere.'
Assert-True ($flashlight -match 'directionalLightIntensity\s*=\s*0\.03f') 'Room02 directional lighting must be reduced for flashlight play.'

Assert-True ($bootstrapper -match 'EnsureRuntimeObject<Room02FlashlightController>\s*\(\s*"Room02_FlashlightController"\s*\)') 'HUD bootstrapper must create the Room02 flashlight controller during play.'
Assert-True ($scene -match 'm_Name:\s+Room02_FlashlightController') 'Show must contain a visible Room02 flashlight controller object.'
Assert-True ($scene -match [regex]::Escape("guid: $flashlightGuid")) 'Show must reference Room02FlashlightController.'
Assert-True ($scene -match 'm_Fog:\s+1' -and $scene -match 'm_AmbientIntensity:\s+0\.15') 'Show must start with a dark atmosphere.'
Assert-True ($scene -match 'm_Name:\s+Directional Light[\s\S]*?m_Intensity:\s+0\.03') 'Scene directional light must be dim enough for the flashlight to matter.'

Write-Host 'Room02 flashlight checks passed.'
