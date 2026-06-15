$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$storyManagerPath = Join-Path $root 'Assets/Room02_Operating/Clues/StoryProgressManager.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $storyManagerPath) 'Missing StoryProgressManager.'

$storyManager = Get-Content -LiteralPath $storyManagerPath -Raw -Encoding UTF8

Assert-True ($storyManager -match 'requiredEscapeKeyClueCount\s*=\s*15') 'StoryProgressManager must require all 15 story clues before auto-granting the escape key.'
Assert-True ($storyManager -match 'HasAllStoryClues\s*=>\s*collectedClueIDs\.Count\s*>=\s*requiredEscapeKeyClueCount') 'HasAllStoryClues must mean all 15 story clues, not the 10-clue suspect-selection threshold.'
Assert-True ($storyManager -match 'TryAutoCollectEscapeKey') 'StoryProgressManager must have a dedicated automatic escape-key collection path.'
Assert-True ($storyManager -match 'collectedClueIDs\.Count\s*[<>]=?\s*requiredEscapeKeyClueCount') 'Automatic escape-key collection must be based on collected clue count.'
Assert-True ($storyManager -match 'TryAutoCollectEscapeKey\s*\(\s*\);[\s\S]*?OnEscapeKeyReady') 'StoryProgressManager must evaluate automatic key collection before key-ready progress events finish.'
Assert-True ($storyManager -match 'hasEscapeKey\s*=\s*true') 'Automatic escape-key collection must set hasEscapeKey.'
Assert-True ($storyManager -match 'OnEscapeKeyCollected\?\.Invoke\s*\(\s*\)') 'Automatic escape-key collection must notify existing key listeners.'
Assert-True ($storyManager -match 'StoryPhase\.SuspectSelection') 'Automatic key collection must preserve suspect-selection flow.'

Write-Host 'Story key acquisition checks passed.'
