$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$doorInteractorPath = Join-Path $root 'Assets/_Shared/Scripts/DoorInteractor.cs'
$playerMovePath = Join-Path $root 'Assets/PlayerMove.cs'

function Assert-True {
    param(
        [bool] $Condition,
        [string] $Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

Assert-True (Test-Path -LiteralPath $doorInteractorPath) 'Missing DoorInteractor.cs'

$doorInteractor = Get-Content -LiteralPath $doorInteractorPath -Raw -Encoding UTF8
$playerMove = Get-Content -LiteralPath $playerMovePath -Raw -Encoding UTF8
$koreanDoorNameCheck = 'normalized.Contains("' + [string][char]0xBB38 + '")'

Assert-True ($doorInteractor -match 'public\s+class\s+DoorInteractor\s*:\s*MonoBehaviour') 'DoorInteractor must be a MonoBehaviour.'
Assert-True ($doorInteractor -match 'KeyCode\.E') 'DoorInteractor must use E as the default interaction key.'
Assert-True ($doorInteractor -match 'Physics\.Raycast') 'DoorInteractor must raycast from the player camera.'
Assert-True ($doorInteractor -match 'SphereCastNonAlloc|SphereCast') 'DoorInteractor must use a small sphere cast fallback so slightly off-center doors can still open.'
Assert-True ($doorInteractor -match 'OverlapSphereNonAlloc' -and $doorInteractor -match 'TryFindNearbyDoor') 'DoorInteractor must detect nearby doors for the E 문열기 prompt even when they are not perfectly centered.'
Assert-True ($doorInteractor -match 'IsDoorName') 'DoorInteractor must filter targets by door-like names.'
Assert-True ($doorInteractor -match 'normalized\.Contains\("gate"\)' -and $doorInteractor.Contains($koreanDoorNameCheck)) 'DoorInteractor must recognize imported or Korean door-like names, not only names containing door.'
Assert-True ($doorInteractor -match 'LooksDoorSized' -and $doorInteractor -match 'horizontalMin' -and $doorInteractor -match 'horizontalMax') 'DoorInteractor must recognize door-sized mesh panels when imported mesh names are generic.'
Assert-True ($doorInteractor -match 'SetCollidersEnabled\s*\(\s*door\s*,\s*false\s*\)') 'DoorInteractor must disable door colliders after opening.'
Assert-True ($doorInteractor -match 'GetHingePoint') 'DoorInteractor must calculate a hinge point at the door edge.'
Assert-True ($doorInteractor -match 'RotateAround\s*\(') 'DoorInteractor must rotate around the hinge edge, not the door center.'
Assert-True ($doorInteractor -match 'GetDoorBounds') 'DoorInteractor must calculate door bounds before choosing a hinge.'
Assert-True ($doorInteractor -notmatch 'door\.localRotation\s*\*=') 'DoorInteractor must not open doors around their center transform.'
Assert-True ($playerMove -match 'AddComponent<DoorInteractor>\s*\(') 'PlayerMove must auto-add DoorInteractor for existing player objects.'

Write-Host 'DoorInteractor checks passed.'
