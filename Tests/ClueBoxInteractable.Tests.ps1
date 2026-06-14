$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$boxPath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueBoxInteractable.cs'
$themePath = Join-Path $root 'Assets/Room02_Operating/Clues/HorrorUITheme.cs'
$journalPath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueJournalUI.cs'
$settingsPath = Join-Path $root 'Assets/Room02_Operating/Clues/SettingsUI.cs'
$timerPath = Join-Path $root 'Assets/Room02_Operating/Clues/TimerUI.cs'
$setupPath = Join-Path $root 'Assets/Room02_Operating/Clues/Editor/ClueSceneSetupTool.cs'
$adapterPath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueBoxRuntimeAdapter.cs'
$runtimeBoxPath = Join-Path $root 'Assets/Room02_Operating/Resources/Room02_ClueBox.prefab'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function U {
    param([int[]] $CodePoints)
    return -join ($CodePoints | ForEach-Object { [char] $_ })
}

Assert-True (Test-Path -LiteralPath $boxPath) 'Missing Room02 ClueBoxInteractable.cs'
Assert-True (Test-Path -LiteralPath $themePath) 'Missing Room02 HorrorUITheme.cs'
Assert-True (Test-Path -LiteralPath $adapterPath) 'Missing Room02 ClueBoxRuntimeAdapter.cs'
Assert-True (Test-Path -LiteralPath $runtimeBoxPath) 'Missing Room02 runtime clue box prefab.'

$box = Get-Content -LiteralPath $boxPath -Raw -Encoding UTF8
$theme = Get-Content -LiteralPath $themePath -Raw -Encoding UTF8
$journal = Get-Content -LiteralPath $journalPath -Raw -Encoding UTF8
$settings = Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8
$timer = Get-Content -LiteralPath $timerPath -Raw -Encoding UTF8
$setup = Get-Content -LiteralPath $setupPath -Raw -Encoding UTF8
$adapter = Get-Content -LiteralPath $adapterPath -Raw -Encoding UTF8
$allCode = "$box`n$theme`n$journal`n$settings`n$timer"

$boxPrompt = '[F] ' + (U 0xBC15,0xC2A4,0x0020,0xC870,0xC0AC,0xD558,0xAE30)
$searchedPrompt = U 0xC774,0xBBF8,0x0020,0xC870,0xC0AC,0xD55C,0x0020,0xBC15,0xC2A4

Assert-True ($box -match 'namespace\s+EscapeRoom') 'ClueBoxInteractable must use EscapeRoom namespace.'
Assert-True ($box -match 'class\s+ClueBoxInteractable\s*:\s*MonoBehaviour') 'ClueBoxInteractable must be a MonoBehaviour.'
Assert-True ($box -match 'ClueData\s+clueData') 'ClueBoxInteractable must expose a ClueData field.'
Assert-True ($box -match 'interactDistance\s*=\s*4f') 'ClueBoxInteractable must default to the existing 4m raycast range.'
Assert-True ($box -match 'Physics\.Raycast' -and $box -match 'Camera\.main') 'ClueBoxInteractable must use camera raycast aiming.'
Assert-True ($box -match 'KeyCode\.F') 'ClueBoxInteractable must search boxes with F.'
Assert-True ($box -notmatch 'KeyCode\.E') 'ClueBoxInteractable must not consume E because doors own E.'
Assert-True ($box.Contains($boxPrompt)) 'ClueBoxInteractable must show [F] 박스 조사하기.'
Assert-True ($box.Contains($searchedPrompt)) 'ClueBoxInteractable must have an already-searched prompt.'
Assert-True ($box -match 'ClueJournalManager\.Instance\.AddClue') 'ClueBoxInteractable must add clues through ClueJournalManager.'
Assert-True ($box -match 'isSearched\s*=\s*true') 'ClueBoxInteractable must keep a searched state.'
Assert-True ($box -notmatch 'gameObject\.SetActive\s*\(\s*false\s*\)') 'ClueBoxInteractable must leave the box in the scene after searching.'
Assert-True ($setup -match 'ClueBoxInteractable') 'Room02 clue setup must place box clue interactables.'
Assert-True ($setup -match 'Box_V1\.prefab|Box_V2\.prefab|SmallMetalicCase\.prefab|CaseMetallic\.prefab') 'Room02 clue setup must use installed box/case assets.'
Assert-True ($setup -match 'SetupOperatingRoomSceneForBatch' -and $setup -match 'EditorSceneManager\.OpenScene' -and $setup -match 'EditorSceneManager\.SaveScene') 'Room02 clue setup must provide a batch scene apply method that opens and saves Scene_OperatingRoom.'
Assert-True ($setup -match 'DestroyObjectImmediate\s*\(\s*existing\.gameObject\s*\)' -or $setup -match 'DestroyImmediate\s*\(\s*existing\.gameObject\s*\)') 'Room02 clue setup must replace old cube clue objects with box objects.'
Assert-True ($adapter -match 'class\s+ClueBoxRuntimeAdapter\s*:\s*MonoBehaviour') 'ClueBoxRuntimeAdapter must be a runtime MonoBehaviour.'
Assert-True ($adapter -match 'Resources\.Load<GameObject>\s*\(\s*"Room02_ClueBox"\s*\)') 'ClueBoxRuntimeAdapter must load the Room02 runtime box prefab.'
Assert-True ($adapter -match 'ClueBoxInteractable' -and $adapter -match 'clueData') 'ClueBoxRuntimeAdapter must add ClueBoxInteractable and copy clue data.'
Assert-True ($adapter -match 'SetActive\s*\(\s*false\s*\)') 'ClueBoxRuntimeAdapter must hide old clue marker objects after creating boxes.'

Assert-True ($theme -match 'class\s+HorrorUITheme') 'HorrorUITheme must centralize Room02 horror UI styling.'
Assert-True ($theme -match 'BloodRed' -and $theme -match 'PanelBlack' -and $theme -match 'TextDim') 'HorrorUITheme must define horror colors.'
Assert-True ($theme -match 'ApplyText' -and $theme -match 'ApplyPanel' -and $theme -match 'ApplyButton') 'HorrorUITheme must expose text, panel, and button helpers.'

Assert-True ($journal -match 'HorrorUITheme' -and $settings -match 'HorrorUITheme' -and $timer -match 'HorrorUITheme') 'HUD UI must use HorrorUITheme.'
Assert-True ($allCode -notmatch 'CursorController') 'Room02 clue box and horror UI must not touch CursorController.'
Assert-True ($allCode -notmatch 'Time\.timeScale') 'Room02 clue box and horror UI must not change Time.timeScale.'

Write-Host 'Clue box and horror UI checks passed.'
