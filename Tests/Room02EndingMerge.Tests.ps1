$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$endingFolder = Join-Path $root 'Assets/Room02_Operating/Ending'
$existingKeyStatePath = Join-Path $root 'Assets/Room02_Operating/Clues/EscapeKeyState.cs'
$duplicateKeyStatePath = Join-Path $endingFolder 'EscapeKeyState.cs'
$qtePath = Join-Path $endingFolder 'EscapeChaseQTE.cs'
$bootstrapperPath = Join-Path $endingFolder 'EscapeEndingBootstrapper.cs'
$exitControllerPath = Join-Path $endingFolder 'EscapeExitController.cs'
$debugGrantPath = Join-Path $endingFolder 'EscapeKeyDebugGrant.cs'
$culpritResourcePath = Join-Path $endingFolder 'Resources/Room02_CulpritChaser.fbx'
$legacyExitDoorPath = Join-Path $root 'Assets/Room02_Operating/Clues/EscapeExitDoor.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $endingFolder) 'Room02 Ending folder must be merged from the ending branch.'
Assert-True (Test-Path -LiteralPath $qtePath) 'Ending folder must include EscapeChaseQTE.'
Assert-True (Test-Path -LiteralPath $bootstrapperPath) 'Ending folder must include EscapeEndingBootstrapper.'
Assert-True (Test-Path -LiteralPath $exitControllerPath) 'Ending folder must include EscapeExitController.'
Assert-True (Test-Path -LiteralPath $debugGrantPath) 'Ending folder must include EscapeKeyDebugGrant for editor testing.'
Assert-True (Test-Path -LiteralPath $culpritResourcePath) 'Ending folder must include the culprit chaser resource.'
Assert-True (Test-Path -LiteralPath $existingKeyStatePath) 'Existing shared EscapeKeyState must remain in the Clues integration folder.'
Assert-True (Test-Path -LiteralPath $legacyExitDoorPath) 'Existing EscapeExitDoor must remain available for scene door interactions.'
Assert-True (-not (Test-Path -LiteralPath $duplicateKeyStatePath)) 'Ending merge must not add a duplicate EscapeKeyState class.'

$qte = Get-Content -LiteralPath $qtePath -Raw -Encoding UTF8
$bootstrapper = Get-Content -LiteralPath $bootstrapperPath -Raw -Encoding UTF8
$exitController = Get-Content -LiteralPath $exitControllerPath -Raw -Encoding UTF8
$debugGrant = Get-Content -LiteralPath $debugGrantPath -Raw -Encoding UTF8
$legacyExitDoor = Get-Content -LiteralPath $legacyExitDoorPath -Raw -Encoding UTF8

Assert-True ($qte -match 'class\s+EscapeChaseQTE\s*:\s*MonoBehaviour') 'EscapeChaseQTE must be a runtime MonoBehaviour.'
Assert-True ($qte -match 'StartQTE\s*\(' -and $qte -match 'KeyCode\.Space') 'EscapeChaseQTE must start a spacebar QTE.'
Assert-True ($qte -match 'GAME OVER' -and $qte -match 'FINISH!') 'EscapeChaseQTE must include success and failure ending titles.'
Assert-True ($qte -match 'Room02_CulpritChaser') 'EscapeChaseQTE must load the merged culprit chaser resource.'
Assert-True ($qte -match 'FontHelper\.Apply\s*\(\s*tmp\s*\)') 'EscapeChaseQTE runtime text must use the project font helper.'

Assert-True ($bootstrapper -match 'RuntimeInitializeOnLoadMethod') 'EscapeEndingBootstrapper must attach ending components at runtime.'
Assert-True ($bootstrapper -match 'EnsureComponent<EscapeChaseQTE>' -and $bootstrapper -match 'EnsureComponent<EscapeExitController>') 'Bootstrapper must create QTE and exit interaction controllers.'
Assert-True ($bootstrapper -match 'Scene_OperatingRoom' -and $bootstrapper -match 'Show') 'Bootstrapper must run in the Room02 operating scenes.'

Assert-True ($exitController -match 'ExitDoor' -and $exitController -match 'KeyCode\.E' -and $exitController -match 'KeyCode\.F') 'EscapeExitController must listen for E/F on ExitDoor.'
Assert-True ($exitController -match 'EscapeKeyState\.HasKey') 'EscapeExitController must use the shared escape-key state.'
Assert-True ($exitController -match 'StartQTE\s*\(\s*\)') 'EscapeExitController must start the chase QTE after door interaction.'
Assert-True ($exitController -match '"Doors"' -and $exitController -match 'Contains\s*\(\s*"door"\s*\)') 'EscapeExitController must recognize existing scene door naming patterns.'
Assert-True ($exitController -match 'HasKnownExitDoor\s*\(\s*\)' -and $exitController -match 'return\s+true;\s*// Prototype fallback') 'EscapeExitController must allow E/F to start QTE in prototype scenes without a named door.'

Assert-True ($legacyExitDoor -match 'EscapeChaseQTE' -and $legacyExitDoor -match 'StartQTE\s*\(\s*\)') 'Existing EscapeExitDoor must be wired to start the spacebar QTE.'

Assert-True ($debugGrant -match 'KeyCode\.F9' -and $debugGrant -match 'EscapeKeyState\.GrantKey\s*\(\s*\)') 'EscapeKeyDebugGrant must grant the shared key for editor testing.'

Write-Host 'Room02 ending merge checks passed.'
