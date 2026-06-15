$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$introPath = Join-Path $root 'Assets/Room02_Operating/Clues/IntroScenarioUI.cs'
$generatorPath = Join-Path $root 'Assets/Room02_Operating/Clues/Editor/ClueAssetGenerator.cs'
$scenePath = Join-Path $root 'Assets/Room02_Operating/Scenes/Show.unity'
$journalUiPath = Join-Path $root 'Assets/Room02_Operating/Clues/ClueJournalUI.cs'
$storyManagerPath = Join-Path $root 'Assets/Room02_Operating/Clues/StoryProgressManager.cs'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function U {
    param([int[]] $CodePoints)
    return -join ($CodePoints | ForEach-Object { [char] $_ })
}

function From-B64 {
    param([string] $Value)
    return [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($Value))
}

function Get-EntryBlock {
    param([string] $Source, [string] $ClueID)
    $match = [regex]::Match($Source, "new\s+ClueEntry\s*\(\s*""$([regex]::Escape($ClueID))""[\s\S]*?\),", 'Singleline')
    Assert-True $match.Success "Missing clue entry block: $ClueID"
    return $match.Value
}

foreach ($path in @($introPath, $generatorPath, $scenePath, $journalUiPath, $storyManagerPath)) {
    Assert-True (Test-Path -LiteralPath $path) "Missing story file: $path"
}

$intro = Get-Content -LiteralPath $introPath -Raw -Encoding UTF8
$generator = Get-Content -LiteralPath $generatorPath -Raw -Encoding UTF8
$scene = Get-Content -LiteralPath $scenePath -Raw -Encoding UTF8
$journalUi = Get-Content -LiteralPath $journalUiPath -Raw -Encoding UTF8
$storyManager = Get-Content -LiteralPath $storyManagerPath -Raw -Encoding UTF8

$introRequiredLines = @(
    (From-B64 '64iI7J2EIOuWoOuztOuLiCDssKjqsIDsmrQg67OR7JuQIOuzteuPhOyYgOuLpC4='),
    (From-B64 '66y47J2AIOyeoOqyqCDsnojqs6AsIOu2iOu5m+ydgCDrtojslYjtlZjqsowg6rmc67mh7J2464ukLg=='),
    (From-B64 '7Jik64qYIOuwpCwg7J20IO2PkOyalOyWkSDrs5Hsm5Dsl5DshJwg7ZWcIOuqheydtCDso73sl4jri6Qu'),
    (From-B64 '7Ius66C5IOuPmeyVhOumrCDrtoDsm5Ag7Jyg7JWI64KYLg=='),
    (From-B64 '64+F7IK07J207JeI64ukLg=='),
    (From-B64 '7Jqp7J2Y7J6Q64qUIOyEuCDrqoXsnbTri6Qu'),
    (From-B64 '7KeE7IS47JuF'),
    (From-B64 '7ZaJ7IKsIOq4sO2ajeyekC4g7IiY7Iig7IukIOq1rOyXrSDri7Tri7ku'),
    (From-B64 '67SJ7YOc7ZiE'),
    (From-B64 '7KeE7IS47JuF7J2YIOygiOy5nC4g7IiY7Iig7IukIOyViOuCtOybkC4='),
    (From-B64 '66y47IiY66+4'),
    (From-B64 '64+Z7JWE66asIDPtlZnrhYQuIOyigOu5hCDsl60g64u064u5Lg=='),
    (From-B64 '6re466as6rOgIOq3uCDspJEg7ZWcIOuqheydgCDslYTsp4Eg7J20IOyViOyXkCDsnojri6Qu'),
    (From-B64 '6re466as6rOgIOKAlCAyMOu2hCDslYjsl5Ag7Jes6riw7IScIOuCmOqwgOudvC4='),
    (From-B64 'U3BhY2UgLyBGIC8g7YG066atOiDri6TsnYw=')
)
foreach ($line in $introRequiredLines) {
    Assert-True ($intro.Contains($line)) "Intro narration missing: $line"
}

$storyEntriesMatch = [regex]::Match($generator, 'CurrentStoryEntries\s*=\s*new\[\]\s*\{(?<body>[\s\S]*?)\};', 'Singleline')
Assert-True ($storyEntriesMatch.Success) 'ClueAssetGenerator must define CurrentStoryEntries for the 15-clue story.'
$storyEntries = $storyEntriesMatch.Groups['body'].Value
$entryCount = ([regex]::Matches($storyEntries, 'new\s+ClueEntry\s*\(')).Count
Assert-True ($entryCount -eq 15) "Story must use exactly 15 clues, found $entryCount."

foreach ($id in @(
    'normal_cast_notice',
    'normal_memorial_frame',
    'normal_conversation_memo',
    'normal_medical_certificate',
    'normal_ward_calendar',
    'clue_hasho_will',
    'key_clue_coldest_place',
    'key_clue_temperature_warning',
    'normal_bong_rebuttal',
    'key_clue_fridge_scratches',
    'normal_makeup_toolbox',
    'normal_sumi_memo',
    'clue_makeup_diary',
    'normal_under_table_space',
    'normal_mirror_message'
)) {
    Assert-True ($storyEntries.Contains($id)) "15-clue story missing clue ID: $id"
}

foreach ($removedID in @(
    'normal_security_log',
    'normal_production_plan',
    'normal_cctv_notice',
    'normal_deleted_entry_trace',
    'normal_poison_ampoule',
    'normal_hidden_camera',
    'normal_paint_footprints',
    'normal_nurse_inventory_log'
)) {
    Assert-True (-not $storyEntries.Contains($removedID)) "15-clue story must remove extra clue ID: $removedID"
}

$notebookPrefix = U 0xC218,0xCCA9,0x003A
$bong = U 0xBD09,0xD0DC,0xD604
$jin = U 0xC9C4,0xC138,0xC6C5
$common = U 0xACF5,0xD1B5
$hintHeader = U 0xC218,0xC9D1,0xB41C,0x0020,0xB2E8,0xC11C,0x0020,0xD78C,0xD2B8
$emptyHint = U 0xC544,0xC9C1,0x0020,0xD655,0xC778,0xB41C,0x0020,0xD78C,0xD2B8,0x0020,0xC5C6,0xC74C
$moon = U 0xBB38,0xC218,0xBBF8
$caseCommon = U 0xC0AC,0xAC74,0x0020,0xACF5,0xD1B5

$castNotice = Get-EntryBlock $storyEntries 'normal_cast_notice'
$memorialFrame = Get-EntryBlock $storyEntries 'normal_memorial_frame'
$trashMemo = Get-EntryBlock $storyEntries 'normal_conversation_memo'
$medicalCertificate = Get-EntryBlock $storyEntries 'normal_medical_certificate'
$wardGraffiti = Get-EntryBlock $storyEntries 'normal_ward_calendar'
$hashoWill = Get-EntryBlock $storyEntries 'clue_hasho_will'
$bedNote = Get-EntryBlock $storyEntries 'key_clue_coldest_place'
$fridgeBox = Get-EntryBlock $storyEntries 'key_clue_temperature_warning'
$bongMemo = Get-EntryBlock $storyEntries 'normal_bong_rebuttal'
$medicineInventory = Get-EntryBlock $storyEntries 'key_clue_fridge_scratches'
$jinMakeupDesk = Get-EntryBlock $storyEntries 'normal_makeup_toolbox'
$sumiDiary = Get-EntryBlock $storyEntries 'normal_sumi_memo'
$jinDiary = Get-EntryBlock $storyEntries 'clue_makeup_diary'
$underTable = Get-EntryBlock $storyEntries 'normal_under_table_space'
$wallMemo = Get-EntryBlock $storyEntries 'normal_mirror_message'

$requiredStoryChecks = @(
    @($castNotice, (From-B64 '67Cw7JetIOyViOuCtOusuA=='), (From-B64 '7IiY7Iig7IukIOuLtOuLuTog7KeE7IS47JuFLCDrtIntg5ztmIQuIO2ZmOyekCDsl606IOycoOyViOuCmC4='), (From-B64 '7IiY7Iig7Iuk7JeQIOygkeq3vO2VoCDsiJgg7J6I7JeI642YIOyduOusvOydtCDtirnsoJXrkJzri6Qu')),
    @($memorialFrame, (From-B64 '7ZWY7Iuc7Zi4IOy2lOuqqCDslaHsnpA='), (From-B64 'MeqwnOyblCDsoIQg7IKs66edLiDtlqXrhYQgMjLshLguIOuPmeyVhOumrCDrtoDsm5Ag7J2864+ZLg=='), (From-B64 '7LWc6re8IOuPmeyVhOumrOyXkOyEnCDsgqzrp53snpDqsIAg7J6I7JeI64ukLiDsgqzsnbjsnYAg67aI66qF7ZmV7ZWY64ukLg==')),
    @($trashMemo, (From-B64 '7JOw66CI6riw7Ya1IOuplOuqqA=='), (From-B64 '7JWI64KYLCDrhKTqsIAg7ZWcIOynk+ydhCDsnorsp4Ag7JWK6rKg64ukLg=='), (From-B64 '7Jyg7JWI64KY7JeQ6rKMIOybkO2VnOydhCDtkojsnYAg7J2466y87J20IOyeiOuLpC4g7ZWE7LK066W8IOq4sOyWte2VtOuRrOyVvCDtlZzri6Qu')),
    @($medicalCertificate, (From-B64 '7ZWY7Iuc7Zi4IOynhOuLqOyEnA=='), (From-B64 '66eQ6riwIOynhOuLqC4g64u064u57J2YOiDrtIntg5ztmIQu'), (From-B64 '7IKs66ed7ZWcIOu2gOybkCDtlZjsi5ztmLjripQg67aI7LmY67ORIO2ZmOyekOyYgOuLpC4=')),
    @($wardGraffiti, (From-B64 '67OR7IukIOuCmeyEnA=='), (From-B64 '7JmcIOyCtOumtCDsiJgg7J6I7JeI64qU642wIOyCtOumrOyngCDslYrslZjslrQu'), (From-B64 '7ZWY7Iuc7Zi47J2YIOyjveydjOyXkCDrtoTrhbjtlZjripQg7J2466y87J20IOyeiOuLpC4=')),
    @($hashoWill, (From-B64 '7ZWY7Iuc7Zi4IOycoOyEnCDsgqzrs7g='), (From-B64 '7IK0IOydtOycoOulvCDsnoPsl4jri6QuIOyViOuCmOqwgCDqt7jqsbgg67m87JWX7JWEIOqwlOuLpC4='), (From-B64 '7ZWY7Iuc7Zi47J2YIOyjveydjOqzvCDsnKDslYjrgpgg7IKs7J207JeQIOyXsOqysOqzoOumrOqwgCDsnojri6Qu')),
    @($bedNote, (From-B64 '7Lmo64yAIOuwkSDsqr3sp4A='), (From-B64 '7J20IOyViOyXkCDri7XsnbQg7J6I64ukLiDrgZ3quYzsp4Ag7LC+7JWE6528Lg=='), (From-B64 '64iE6rWw6rCAIOydtCDrs5Hsm5Ag7JWI7JeQIOustOyWuOqwgOulvCDsiKjqsqjrkoDri6Qu')),
    @($fridgeBox, (From-B64 '64OJ7J6lIOyVve2SiO2VqA=='), (From-B64 '7Jio64+EIOqyveqzoCDsiqTti7Dsu6QuIOusuCDrqqjshJzrpqzsl5Ag6riB7Z6MIOyekOq1reydtCDsnojri6Qu'), (From-B64 '64iE6rWw6rCAIOy1nOq3vOyXkCDsnbQg7JW97ZKI7ZWo7J2EIOyXtOyXiOuLpC4=')),
    @($bongMemo, (From-B64 '67SJ7YOc7ZiEIOuplOuqqA=='), (From-B64 '7IS47JuF7JWEIOuCtOqwgCDrqLzsoIAg7IiY7Iig7IukIOuTpOyWtOqwiOqyjC4gLyDri7U6IOq0nOywruyVhCwg64K06rCAIO2VoOqyjC4='), (From-B64 '64u57J28IOyImOyIoOyLpCDsnoXsnqUg7Iic7ISc7JeQIOuzgOuPmeydtCDsnojsl4jri6Qu')),
    @($medicineInventory, (From-B64 '7JW97ZKIIOuqqeuhne2RnA=='), (From-B64 '66qp66Gd7JeQIOyXhuuKlCDslb3tkojsnbQg7ZWY64KYIOuwmOy2nOuQkOuLpC4g64Kg7Kec64qUIOyYpOuKmC4='), (From-B64 '7Jik64qYIOuIhOq1sOqwgCDsnbQg67O06rSA7Iuk7JeQ7IScIOyVve2SiOydhCDqsIDsoLjqsJTri6Qu')),
    @($jinMakeupDesk, (From-B64 '7KeE7IS47JuFIOu2hOyepeuMgA=='), (From-B64 '67aJ7J2AIO2OmOyduO2KuCDthrXsnbQg7Je066Ck7J6I64ukLiDrsJzqsIDrnb0g66qo7JaRIOyekOq1reydtCDssI3tmIDsnojri6Qu'), (From-B64 '64iE6rWw6rCAIOydtCDsnpDrpqzsl5DshJwg67Cc6rCA65297JeQIO2OmOyduO2KuOulvCDsuaDtlojri6Qu')),
    @($sumiDiary, (From-B64 '66y47IiY66+4IOydvOq4sOyepQ=='), (From-B64 '7Jik64qYIOustOyKqCDsnbzsnbQg7IOd6ri4IOqygyDqsJnri6QuIOuCmOyBnCDsmIjqsJDsnbQg65Og64ukLg=='), (From-B64 '7Jik64qYIOyCrOqxtOydhCDrr7jrpqwg64iI7LmY7LGIIOyduOusvOydtCDsnojsl4jrjZgg6rKDIOqwmeuLpC4=')),
    @($jinDiary, (From-B64 '7KeE7IS47JuFIOydvOq4sOyepQ=='), (From-B64 '642UIOydtOyDgSDrr7jro7Ag7IiYIOyXhuuLpC4g7Jik64qY7J2064ukLg=='), (From-B64 '64iE6rWw6rCAIOyYpOuKmOydhCDsnITtlbQg7Jik656YIOq4sOuLpOugpOyZlOuLpC4=')),
    @($underTable, (From-B64 '7IiY7Iig64yAIOyVhOuemA=='), (From-B64 '7IKs656MIO2VnCDrqoXsnbQg7Iio7J2EIOyImCDsnojripQg6rO16rCELiDrsJTri6Xsl5Ag6riB7Z6MIOyekOq1reqzvCDrtonsnYAg7Y6Y7J247Yq4IOyekOq1reydtCDrgqjslYTsnojri6Qu'), (From-B64 '67KU7ZaJIOuLueyLnCDsiJjsiKDrjIAg7JWE656YIOuIhOq1sOqwgCDsiKjslrTsnojsl4jri6Qu')),
    @($wallMemo, (From-B64 '67K9IOuplOuqqA=='), (From-B64 '7ZWY7Iuc7Zi466W8IOychO2VtOyEnC4g66+47JWI7ZW0LCDslYjrgpguIOyTsOugiOq4sO2GtSDrqZTrqqjsmYAg6rCZ7J2AIO2VhOyytOuLpC4='), (From-B64 '7JOw66CI6riw7Ya1IOuplOuqqOyZgCDqsJnsnYAg7ZWE7LK064ukLiDrkZAg66mU66qo66W8IOyTtCDsnbjrrLzsnbQg6rCZ64ukLg=='))
)

foreach ($check in $requiredStoryChecks) {
    $block = $check[0]
    for ($i = 1; $i -lt $check.Count; $i++) {
        Assert-True ($block.Contains($check[$i])) "Story clue entry missing required text: $($check[$i])"
    }
    Assert-True ($block.Contains($notebookPrefix)) "Story clue entry must include notebook prefix: $($check[1])"
}

Assert-True ($castNotice.Contains($bong) -and $castNotice.Contains($jin)) 'Cast notice must update Bong/Jin suspect notebook.'
Assert-True ($underTable.Contains($jin)) 'Under-table clue must update Jin suspect notebook.'
Assert-True ($medicineInventory.Contains($common)) 'Medicine inventory clue must update common notebook.'

$sceneClueCount = ([regex]::Matches($scene, 'm_Name:\s+Clue_')).Count
$wiredClueCount = ([regex]::Matches($scene, 'clueData:\s*\{fileID:\s*11400000')).Count
Assert-True ($sceneClueCount -eq 15) "Show scene must contain exactly 15 clue boxes, found $sceneClueCount."
Assert-True ($wiredClueCount -eq 15) "Show scene must wire exactly 15 clue data refs, found $wiredClueCount."

Assert-True ($journalUi -match 'BuildSuspectCards\s*\(\s*\)' -and $journalUi -match 'GetCollectedHintsForPerson') 'Suspect notebook must rebuild from collected clue hints.'
Assert-True ($journalUi.Contains($hintHeader) -and $journalUi.Contains($emptyHint)) 'Suspect notebook must show collected hints and empty state.'
Assert-True ($journalUi.Contains($jin) -and $journalUi.Contains($bong) -and $journalUi.Contains($moon) -and $journalUi.Contains($caseCommon)) 'Suspect notebook must include suspects and common case notes.'

Assert-True ($storyManager -match 'requiredClueCount\s*=\s*10') 'StoryProgressManager must unlock suspect selection after 10 collected clues.'
Assert-True ($storyManager -match 'HasAllStoryClues') 'StoryProgressManager must expose all-story-clues completion.'
Assert-True ($storyManager -match 'collectedClueIDs\.Count\s*>=\s*requiredClueCount') 'StoryProgressManager must check collected clue count for suspect selection.'
Assert-True ($storyManager -notmatch 'hasEscapeKey\s*&&\s*HasAllStoryClues') 'Suspect selection must not require the escape key once 10 clues are collected.'
Assert-True ($storyManager -match 'CanSelectSuspect') 'StoryProgressManager must name the 10-clue suspect selection readiness explicitly.'

Write-Host 'Room02 15-clue story checks passed.'
