$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$clueDataPath = Join-Path $root 'Assets/Clues/ClueData.cs'
$managerPath = Join-Path $root 'Assets/Clues/ClueJournalManager.cs'
$interactablePath = Join-Path $root 'Assets/Clues/ClueInteractable.cs'
$uiPath = Join-Path $root 'Assets/Clues/ClueJournalUI.cs'

function Assert-True {
    param(
        [bool] $Condition,
        [string] $Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

Assert-True (Test-Path -LiteralPath $clueDataPath) 'Missing Assets/Clues/ClueData.cs'
Assert-True (Test-Path -LiteralPath $managerPath) 'Missing Assets/Clues/ClueJournalManager.cs'
Assert-True (Test-Path -LiteralPath $interactablePath) 'Missing Assets/Clues/ClueInteractable.cs'
Assert-True (Test-Path -LiteralPath $uiPath) 'Missing Assets/Clues/ClueJournalUI.cs'

$clueData = Get-Content -LiteralPath $clueDataPath -Raw -Encoding UTF8
$manager = Get-Content -LiteralPath $managerPath -Raw -Encoding UTF8
$interactable = Get-Content -LiteralPath $interactablePath -Raw -Encoding UTF8
$ui = Get-Content -LiteralPath $uiPath -Raw -Encoding UTF8
$allCode = "$clueData`n$manager`n$interactable`n$ui"

function U {
    param([int[]] $CodePoints)
    return -join ($CodePoints | ForEach-Object { [char] $_ })
}

$collectPrompt = '[F] ' + (U 0xC99D,0xAC70) + ' ' + (U 0xC218,0xC9D1)
$evidenceTab = U 0xC218,0xC9D1,0x0020,0xC99D,0xAC70
$suspectTab = (U 0xC6A9,0xC758,0xC790) + ' ' + (U 0xC218,0xCCA9)
$explorePlaceholder = (U 0xC774,0x0020,0xAD6C,0xC5ED,0xC744,0x0020,0xD0D0,0xC0C9,0xD558,0xBA74,0x0020,0xC99D,0xAC70,0xB97C,0x0020,0xC218,0xC9D1,0xD560,0x0020,0xC218,0x0020,0xC788,0xC2B5,0xB2C8,0xB2E4)
$people = @(
    (U 0xC720,0xC548,0xB098),
    (U 0xC9C4,0xC138,0xC6C5),
    (U 0xBD09,0xD0DC,0xD604),
    (U 0xBB38,0xC218,0xBBF8),
    (U 0xD558,0xC2DC,0xD638)
)
$roles = @(
    (U 0xD53C,0xD574,0xC790),
    (U 0xC6A9,0xC758,0xC790),
    (U 0xACE0,0xC778)
)

Assert-True ($clueData -match 'namespace\s+EscapeRoom') 'ClueData must use namespace EscapeRoom.'
Assert-True ($clueData -match 'class\s+ClueData\s*:\s*ScriptableObject') 'ClueData must be a ScriptableObject.'
Assert-True ($clueData -match 'CreateAssetMenu') 'ClueData must expose a CreateAssetMenu entry.'
foreach ($field in @('clueName', 'description', 'meaning', 'areaName')) {
    Assert-True ($clueData -match "string\s+$field\s*;") "ClueData missing string field: $field"
}

Assert-True ($manager -match 'namespace\s+EscapeRoom') 'ClueJournalManager must use namespace EscapeRoom.'
Assert-True ($manager -match 'static\s+ClueJournalManager\s+Instance') 'ClueJournalManager must expose a singleton Instance.'
Assert-True ($manager -match 'AddClue\s*\(\s*ClueData\s+\w+\s*\)') 'ClueJournalManager must implement AddClue(ClueData).'
Assert-True ($manager -match 'HasClue\s*\(\s*ClueData\s+\w+\s*\)') 'ClueJournalManager must implement HasClue(ClueData).'
Assert-True ($manager -match 'OnCluesChanged') 'ClueJournalManager must expose a UI refresh event.'
Assert-True ($manager -match 'Contains\s*\(') 'ClueJournalManager must prevent duplicate clues.'

Assert-True ($interactable -match 'namespace\s+EscapeRoom') 'ClueInteractable must use namespace EscapeRoom.'
Assert-True ($interactable -match 'class\s+ClueInteractable\s*:\s*MonoBehaviour') 'ClueInteractable must be a MonoBehaviour.'
Assert-True ($interactable -match 'ClueData\s+clueData') 'ClueInteractable must expose a ClueData field.'
Assert-True ($interactable -match 'collectDistance\s*=\s*2f') 'ClueInteractable must default to 2m collection range.'
Assert-True ($interactable -match 'KeyCode\.F') 'ClueInteractable must collect with F.'
Assert-True ($interactable -notmatch 'KeyCode\.E') 'ClueInteractable must not consume E because E is reserved for doors.'
Assert-True ($interactable.Contains($collectPrompt)) 'ClueInteractable must show the requested prompt text.'
Assert-True ($interactable -match 'SetActive\s*\(\s*false\s*\)') 'ClueInteractable must disable the object after collection.'
Assert-True ($interactable -match 'ClueJournalManager\.Instance\.AddClue') 'ClueInteractable must call ClueJournalManager.AddClue().'

Assert-True ($ui -match 'namespace\s+EscapeRoom') 'ClueJournalUI must use namespace EscapeRoom.'
Assert-True ($ui -match 'ScreenSpaceOverlay') 'ClueJournalUI must create/use a Screen Space Overlay canvas.'
Assert-True ($ui -match 'KeyCode\.J' -and $ui -match 'KeyCode\.Tab') 'ClueJournalUI must toggle with J or Tab.'
Assert-True ($ui.Contains($evidenceTab)) 'ClueJournalUI must include the collected evidence tab.'
Assert-True ($ui.Contains($suspectTab)) 'ClueJournalUI must include the suspect notebook tab.'
foreach ($person in $people) {
    Assert-True ($ui.Contains($person)) "ClueJournalUI missing person entry: $person"
}
foreach ($role in $roles) {
    Assert-True ($ui.Contains($role)) "ClueJournalUI missing role label: $role"
}
Assert-True ($ui -match '\?\?\?' -and $ui.Contains($explorePlaceholder)) 'ClueJournalUI must render undiscovered clue placeholders.'
Assert-True ($ui -match 'ScrollToClue') 'ClueJournalUI evidence chips must scroll to the clue card.'
Assert-True ($ui -match 'collectedCount.*totalCount|progressText') 'ClueJournalUI must display collection progress.'

Assert-True ($allCode -notmatch 'CursorController') 'Clue journal system must not reference CursorController.'
Assert-True ($allCode -notmatch 'Time\.timeScale') 'Clue journal system must not change Time.timeScale.'

Write-Host 'Clue journal system checks passed.'
