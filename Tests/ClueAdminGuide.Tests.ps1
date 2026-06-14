$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$guidePath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueAdminGuideOverlay.cs'
$guideMetaPath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueAdminGuideOverlay.cs.meta'
$scenePath = Join-Path $root 'Assets/Room02_Operating/Scenes/Scene_OperatingRoom.unity'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $guidePath) 'Missing Room02 admin clue guide overlay script.'
Assert-True (Test-Path -LiteralPath $guideMetaPath) 'Missing Room02 admin clue guide overlay meta file.'
Assert-True (Test-Path -LiteralPath $scenePath) 'Missing Scene_OperatingRoom scene.'

$guide = Get-Content -LiteralPath $guidePath -Raw -Encoding UTF8
$scene = Get-Content -LiteralPath $scenePath -Raw -Encoding UTF8
$guideGuid = ((Select-String -LiteralPath $guideMetaPath -Pattern '^guid:').Line -replace '^guid:\s*', '').Trim()

Assert-True ($guide -match 'class\s+ClueAdminGuideOverlay\s*:\s*MonoBehaviour') 'ClueAdminGuideOverlay must be a MonoBehaviour.'
Assert-True ($guide -match 'FindObjectsOfType<ClueBoxInteractable>\s*\(') 'Admin guide overlay must discover Room02 clue boxes automatically.'
Assert-True ($guide -match 'WorldToViewportPoint' -and $guide -match 'Mathf\.Clamp') 'Admin guide overlay must clamp arrows to the visible screen edge for off-screen clues.'
Assert-True ($guide -match 'Screen\.width' -and $guide -match 'Screen\.height') 'Admin guide overlay must use screen bounds so guide arrows remain visible.'
Assert-True ($guide -match 'AdminGuideArrow_' -and $guide -match 'TextMeshProUGUI') 'Admin guide overlay must create visible arrow labels for each clue.'
Assert-True ($guide -match '#if\s+UNITY_EDITOR' -and $guide -match 'OnDrawGizmos') 'Admin guide overlay must also draw editor-only Scene view guides.'
Assert-True ($guide -notmatch 'Handles\.Label') 'Scene view guide must not use Handles.Label because it can trigger Unity 6 editor assertions.'
Assert-True ($guide -match 'Gizmos\.DrawLine' -and $guide -match 'Gizmos\.DrawSphere') 'Scene view guide must draw obvious clue arrows with editor-safe Gizmos.'
Assert-True ($guide -match 'Gizmos\.DrawWireSphere') 'Scene view guide must circle each clue location so admins can find clue boxes easily.'
Assert-True ($guide -notmatch 'Application\.isEditor') 'Admin guide overlay must not disappear outside the Unity editor when used for admin playtesting.'
Assert-True ($guide -notmatch 'Time\.timeScale' -and $guide -notmatch 'CursorController') 'Admin guide overlay must not alter gameplay time or cursor behavior.'

Assert-True ($scene -match 'm_Name:\s+Admin_ClueGuideOverlay') 'Scene_OperatingRoom must contain the admin clue guide overlay object.'
Assert-True ($scene -match [regex]::Escape("guid: $guideGuid")) 'Scene_OperatingRoom must reference ClueAdminGuideOverlay.'
Assert-True ($scene -match 'm_TagString:\s+Untagged') 'Admin clue guide overlay scene object must remain loadable during admin playtesting.'
Assert-True ($scene -match 'drawEditorSceneGuides:\s+1') 'Admin scene guide must be explicitly enabled in Scene_OperatingRoom.'

Write-Host 'Clue admin guide checks passed.'
