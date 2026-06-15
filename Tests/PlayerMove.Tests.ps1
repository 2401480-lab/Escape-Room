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
Assert-True ($playerMove -match 'Transform\s+\w*camera\w*|Transform\s+\w*Camera\w*') 'PlayerMove must keep a player camera Transform reference instead of relying on Camera.main every frame.'
Assert-True ($playerMove -notmatch 'Camera\.main\.transform') 'PlayerMove must not dereference Camera.main.transform during Update because a missing or mismatched MainCamera stops looking.'
Assert-True ($playerMove -match 'float\s+yaw') 'PlayerMove must keep an accumulated yaw value for full 360-degree horizontal look.'
Assert-True ($playerMove -match 'float\s+pitch') 'PlayerMove must keep pitch separate from yaw so only vertical look is clamped.'
Assert-True ($playerMove -match 'yaw\s*\+=\s*mouseX') 'PlayerMove must accumulate horizontal mouse input into yaw.'
Assert-True ($playerMove -notmatch 'Mathf\.Clamp\s*\(\s*yaw') 'PlayerMove must not clamp horizontal yaw; the player must be able to turn 360 degrees.'
Assert-True ($playerMove -match 'pitch\s*=\s*Mathf\.Clamp\s*\(\s*pitch\s*,\s*-90f\s*,\s*90f\s*\)') 'PlayerMove must clamp only vertical pitch.'
Assert-True ($playerMove -match 'transform\.rotation\s*=\s*Quaternion\.Euler\s*\(\s*0f\s*,\s*yaw\s*,\s*0f\s*\)') 'PlayerMove must apply accumulated yaw directly to the player body.'

Write-Host 'PlayerMove collision checks passed.'
