$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$guidePath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueAdminGuideOverlay.cs'
$interactablePath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueBoxInteractable.cs'
$storyManagerPath = Join-Path $root 'Assets/Room02_Operating/Clues/StoryProgressManager.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $guidePath) 'Missing Room02 admin guide overlay script.'
Assert-True (Test-Path -LiteralPath $interactablePath) 'Missing Room02 clue box interactable script.'
Assert-True (Test-Path -LiteralPath $storyManagerPath) 'Missing Room02 story progress manager script.'

$guide = Get-Content -LiteralPath $guidePath -Raw -Encoding UTF8
$interactable = Get-Content -LiteralPath $interactablePath -Raw -Encoding UTF8
$storyManager = Get-Content -LiteralPath $storyManagerPath -Raw -Encoding UTF8

Assert-True ($guide -match 'Input\.GetKeyDown\s*\(\s*KeyCode\.Y\s*\)') 'Admin guide overlay must trigger the skip with the Y key during play.'
Assert-True ($guide -match 'CollectAllCluesAndGrantKey\s*\(') 'Admin guide overlay must have a dedicated all-clue skip method.'
Assert-True ($guide -match 'FindObjectsOfType<ClueBoxInteractable>\s*\(\s*true\s*\)') 'Admin skip must include inactive clue boxes so all 15 story clues can be collected.'
Assert-True ($guide -match 'AdminCollectClue\s*\(') 'Admin skip must collect each clue through the clue box path so journal state stays consistent.'
Assert-True ($guide -match 'GrantEscapeKeyFromAdminSkip\s*\(') 'Admin skip must immediately grant the escape key after collecting all clues.'
Assert-True ($guide -notmatch 'allowRuntimeAdminGuide[\s\S]{0,120}KeyCode\.Y') 'Y skip must not be gated by the runtime admin guide visibility toggle.'

Assert-True ($interactable -match 'public\s+bool\s+AdminCollectClue\s*\(') 'Clue boxes must expose an admin collection path.'
Assert-True ($interactable -match 'AdminCollectClue[\s\S]*?ClueJournalManager\.Instance\.AddClue\s*\(\s*clueData\s*\)') 'Admin collection must add clue data to the journal.'
Assert-True ($interactable -match 'AdminCollectClue[\s\S]*?MarkSearchedVisual\s*\(') 'Admin collection must mark clue boxes searched visually.'
Assert-True ($interactable -match 'AdminCollectClue[\s\S]*?HidePrompt\s*\(') 'Admin collection must hide the interaction prompt after admin collection.'

Assert-True ($storyManager -match 'public\s+void\s+GrantEscapeKeyFromAdminSkip\s*\(') 'StoryProgressManager must expose a clear admin skip key grant method.'
Assert-True ($storyManager -match 'GrantEscapeKeyFromAdminSkip[\s\S]*?hasEscapeKey\s*=\s*true') 'Admin skip key grant must set hasEscapeKey.'
Assert-True ($storyManager -match 'GrantEscapeKeyFromAdminSkip[\s\S]*?OnEscapeKeyCollected\?\.Invoke\s*\(\s*\)') 'Admin skip key grant must notify existing key listeners.'
Assert-True ($storyManager -match 'GrantEscapeKeyFromAdminSkip[\s\S]*?OnEscapeKeyReady\?\.Invoke\s*\(\s*\)') 'Admin skip key grant must notify key-ready listeners.'

Write-Host 'Clue admin skip checks passed.'
