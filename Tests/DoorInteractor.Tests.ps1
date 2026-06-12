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

$doorInteractor = Get-Content -LiteralPath $doorInteractorPath -Raw
$playerMove = Get-Content -LiteralPath $playerMovePath -Raw

Assert-True ($doorInteractor -match 'public\s+class\s+DoorInteractor\s*:\s*MonoBehaviour') 'DoorInteractor must be a MonoBehaviour.'
Assert-True ($doorInteractor -match 'KeyCode\.E') 'DoorInteractor must use E as the default interaction key.'
Assert-True ($doorInteractor -match 'Physics\.Raycast') 'DoorInteractor must raycast from the player camera.'
Assert-True ($doorInteractor -match 'IsDoorName') 'DoorInteractor must filter targets by door-like names.'
Assert-True ($doorInteractor -match 'SetCollidersEnabled\s*\(\s*door\s*,\s*false\s*\)') 'DoorInteractor must disable door colliders after opening.'
Assert-True ($doorInteractor -match 'Quaternion\.Euler') 'DoorInteractor must rotate opened doors.'
Assert-True ($playerMove -match 'AddComponent<DoorInteractor>\s*\(') 'PlayerMove must auto-add DoorInteractor for existing player objects.'

Write-Host 'DoorInteractor checks passed.'
