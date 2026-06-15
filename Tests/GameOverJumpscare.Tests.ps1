$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$gameOverPath = Join-Path $root 'Assets/Room02_Operating/Clues/GameOverUI.cs'
$modelPath = Join-Path $root 'Assets/Room02_Operating/Models/Ch45_nonPBR.fbx'
$modelMetaPath = Join-Path $root 'Assets/Room02_Operating/Models/Ch45_nonPBR.fbx.meta'
$runAnimationPath = Join-Path $root 'Assets/Room02_Operating/Models/Fast Run.fbx'
$runAnimationMetaPath = Join-Path $root 'Assets/Room02_Operating/Models/Fast Run.fbx.meta'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $gameOverPath) 'Missing Room02 GameOverUI.'
Assert-True (Test-Path -LiteralPath $modelPath) 'Ch45 jumpscare FBX must live under Assets/Room02_Operating/Models.'
Assert-True (Test-Path -LiteralPath $modelMetaPath) 'Ch45 jumpscare FBX meta must be committed with the model.'
Assert-True (Test-Path -LiteralPath $runAnimationPath) 'Fast Run FBX must live under Assets/Room02_Operating/Models for the lunge animation.'
Assert-True (Test-Path -LiteralPath $runAnimationMetaPath) 'Fast Run FBX meta must be committed with the run animation.'

$gameOver = Get-Content -LiteralPath $gameOverPath -Raw -Encoding UTF8
$modelMeta = Get-Content -LiteralPath $modelMetaPath -Raw -Encoding UTF8
$runAnimationMeta = Get-Content -LiteralPath $runAnimationMetaPath -Raw -Encoding UTF8

Assert-True ($gameOver -match 'namespace\s+EscapeRoom') 'GameOverUI must remain in namespace EscapeRoom.'
Assert-True ($gameOver -match 'Ch45_nonPBR\.fbx') 'GameOverUI must reference the Ch45 jumpscare model asset.'
Assert-True ($gameOver -match 'Fast Run\.fbx') 'GameOverUI must reference the Fast Run animation asset.'
Assert-True ($gameOver -match 'GameOverJumpscareModel') 'GameOverUI must create a clearly named jumpscare model object.'
Assert-True ($gameOver -match 'PlayJumpscareSequence') 'GameOverUI must run a jumpscare sequence for fatal deduction failures.'
Assert-True ($gameOver -match 'StartCoroutine\s*\(\s*PlayJumpscareSequence\s*\(') 'GameOverUI must start the jumpscare as a coroutine.'
Assert-True ($gameOver -match 'StartCoroutine\s*\(\s*LungeJumpscareModel\s*\(') 'GameOverUI must lunge the model before showing the final Game Over panel.'
Assert-True ($gameOver -match 'GameOverReason\.WrongAnswer' -and $gameOver -match 'GameOverReason\.DeductionTimerExpired') 'Wrong answer and deduction timer expiry must trigger the jumpscare.'
Assert-True ($gameOver -match 'ShouldPlayJumpscare') 'GameOverUI must isolate which GameOver reasons use the jumpscare.'
Assert-True ($gameOver -match 'SpawnJumpscareModel') 'GameOverUI must spawn the model in front of the player camera.'
Assert-True ($gameOver -match 'LungeJumpscareModel') 'GameOverUI must move the jumpscare model toward the player camera.'
Assert-True ($gameOver -match 'Vector3\.Lerp') 'The lunge must visibly interpolate from far away to close range.'
Assert-True ($gameOver -match 'lungeStartDistance' -and $gameOver -match 'lungeImpactDistance' -and $gameOver -match 'lungeDuration') 'The lunge distance and duration must be configurable.'
Assert-True ($gameOver -notmatch 'jumpscareDistance') 'GameOverUI must not reference the removed jumpscareDistance field.'
Assert-True ($gameOver -match 'Camera\.main') 'GameOverUI must position the jumpscare from the active camera.'
Assert-True ($gameOver -match 'AssetDatabase\.LoadAssetAtPath<GameObject>') 'GameOverUI must load the Room02 model asset in the Unity editor.'
Assert-True ($gameOver -match 'Resources\.Load<GameObject>') 'GameOverUI must keep a runtime-safe prefab fallback path.'
Assert-True ($gameOver -match 'ResolveJumpscareRunAnimation') 'GameOverUI must resolve the Fast Run clip for the jumpscare model.'
Assert-True ($gameOver -match 'AnimationClip') 'GameOverUI must load the Fast Run FBX as an animation clip.'
Assert-True ($gameOver -match 'AssetDatabase\.LoadAllAssetsAtPath') 'GameOverUI must find the animation clip inside the imported FBX in the Unity editor.'
Assert-True ($gameOver -match 'Resources\.Load<AnimationClip>') 'GameOverUI must keep a runtime-safe animation fallback path.'
Assert-True ($gameOver -match 'AddComponent<Animation>' -and $gameOver -match '\.Play\s*\(') 'GameOverUI must play the Fast Run animation on the spawned model.'
Assert-True ($gameOver -match 'AudioClip\.Create' -and $gameOver -match 'PlayOneShot') 'GameOverUI must play a procedural dudung-tak impact sound.'
Assert-True ($gameOver -match 'blackoutImage' -and $gameOver -match 'Color\.black') 'GameOverUI must cover the screen with black darkness.'
Assert-True ($gameOver -match 'GAME OVER') 'GameOverUI must display GAME OVER text.'
Assert-True ($gameOver -match 'Color\.red' -or $gameOver -match 'new\s+Color\s*\(\s*1f\s*,\s*0f\s*,\s*0f') 'GAME OVER text must be red.'
Assert-True ($gameOver -match 'WaitForSecondsRealtime') 'The jumpscare timing must not depend on Time.timeScale.'
Assert-True ($gameOver -notmatch 'Time\.timeScale') 'GameOverUI must not change Time.timeScale.'
Assert-True ($modelMeta -match '(?m)^guid:\s*[a-f0-9]{32}') 'Ch45 jumpscare model meta must contain a stable Unity GUID.'
Assert-True ($runAnimationMeta -match '(?m)^guid:\s*[a-f0-9]{32}') 'Fast Run animation meta must contain a stable Unity GUID.'

Write-Host 'GameOver jumpscare checks passed.'
