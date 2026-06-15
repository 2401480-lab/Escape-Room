$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$room02ScenePath = Join-Path $root 'Assets/Room02_Operating/Scenes/Scene_OperatingRoom.unity'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

Assert-True (Test-Path -LiteralPath $room02ScenePath) 'Missing Room02-owned gameplay scene.'

$scene = Get-Content -LiteralPath $room02ScenePath -Raw -Encoding UTF8
$clueCount = ([regex]::Matches($scene, 'm_Name:\s+Clue_')).Count

Assert-True ($scene -match 'guid:\s+beef3bcde1610f04aa96275436147bb9') 'Room02 gameplay scene must include the Show/Abandoned Asylum map prefab.'
Assert-True ($clueCount -eq 31) "Room02 gameplay scene must contain 31 Room02 clue boxes, found $clueCount."
Assert-True ($scene -match 'm_Name:\s+Admin_ClueGuideOverlay') 'Room02 gameplay scene must contain the admin clue guide overlay.'
Assert-True ($scene -match 'm_Name:\s+Room02_BGM') 'Room02 gameplay scene must contain Room02 BGM.'
Assert-True ($scene -notmatch 'm_Name:\s+TestClue_cast_notice') 'Room02 gameplay scene must not keep the old Show test clue.'
Assert-True ($scene -notmatch 'm_Name:\s+TestClueBox') 'Room02 gameplay scene must not keep temporary test clue boxes.'

Write-Host 'Room02 Show map checks passed.'
