$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
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

Assert-True (Test-Path -LiteralPath $playerMovePath) 'Missing PlayerMove.cs'

$playerMove = Get-Content -LiteralPath $playerMovePath -Raw

Assert-True ($playerMove -match '\[RequireComponent\(typeof\(CharacterController\)\)\]') 'PlayerMove must require CharacterController for collision-aware movement.'
Assert-True ($playerMove -match 'CharacterController\s+\w+') 'PlayerMove must keep a CharacterController reference.'
Assert-True ($playerMove -match 'AddComponent<CharacterController>\s*\(') 'PlayerMove must add CharacterController at runtime for existing scene objects.'
Assert-True ($playerMove -match 'characterController\.center\s*=\s*new\s+Vector3\s*\(\s*0f\s*,\s*1f\s*,\s*0f\s*\)') 'Runtime CharacterController center must keep the controller bottom on the floor.'
Assert-True ($playerMove -match 'public\s+float\s+gravity\s*=') 'PlayerMove must expose gravity so the player can descend stairs and ledges.'
Assert-True ($playerMove -match 'public\s+float\s+walkSpeed\s*=\s*3f') 'PlayerMove normal movement speed must default to 3.0.'
Assert-True ($playerMove -match 'public\s+float\s+runSpeed\s*=\s*5f') 'PlayerMove Shift running speed must default to 5.0.'
Assert-True ($playerMove -match 'KeyCode\.LeftShift|KeyCode\.RightShift') 'PlayerMove must use Shift to select running speed.'
Assert-True ($playerMove -match 'currentSpeed\s*=\s*isRunning\s*\?\s*runSpeed\s*:\s*walkSpeed') 'PlayerMove must choose runSpeed only while Shift is held.'
Assert-True ($playerMove -match 'Vector3\s+verticalVelocity') 'PlayerMove must track vertical velocity for gravity.'
Assert-True ($playerMove -match 'characterController\.isGrounded') 'PlayerMove must reset downward velocity while grounded.'
Assert-True ($playerMove -match 'verticalVelocity\.y\s*\+=\s*gravity\s*\*\s*Time\.deltaTime') 'PlayerMove must integrate gravity over time.'
Assert-True ($playerMove -match 'characterController\.Move\s*\(\s*verticalVelocity\s*\*\s*Time\.deltaTime\s*\)') 'PlayerMove must apply vertical movement through CharacterController.Move().'
Assert-True ($playerMove -match '\.Move\s*\(') 'PlayerMove must move through CharacterController.Move().'
Assert-True ($playerMove -notmatch 'transform\.position\s*\+=') 'PlayerMove must not bypass collisions with transform.position += movement.'

Write-Host 'PlayerMove collision checks passed.'
