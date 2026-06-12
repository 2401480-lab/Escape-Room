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
Assert-True ($playerMove -match '\.Move\s*\(') 'PlayerMove must move through CharacterController.Move().'
Assert-True ($playerMove -notmatch 'transform\.position\s*\+=') 'PlayerMove must not bypass collisions with transform.position += movement.'

Write-Host 'PlayerMove collision checks passed.'
