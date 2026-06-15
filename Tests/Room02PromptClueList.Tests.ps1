$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$generatorPath = Join-Path $root 'Assets/Room02_Operating/Clues/Editor/ClueAssetGenerator.cs'
$normalCluePath = Join-Path $root 'Assets/Room02_Operating/Clues/Normal'
$keyCluePath = Join-Path $root 'Assets/Room02_Operating/Clues/KeyClue'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function B {
    param([string] $Value)
    return [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($Value))
}

function Get-EntryBlock {
    param([string] $Source, [string] $ClueID)
    $match = [regex]::Match($Source, "new\s+ClueEntry\s*\(\s*""$([regex]::Escape($ClueID))""[\s\S]*?\),", 'Singleline')
    Assert-True $match.Success "Missing clue entry block: $ClueID"
    return $match.Value
}

function Get-AssetTextByClueID {
    param([string] $ClueID)
    $assetFiles = @(Get-ChildItem -LiteralPath $normalCluePath -Filter '*.asset' -File) + @(Get-ChildItem -LiteralPath $keyCluePath -Filter '*.asset' -File)
    $assetFile = $assetFiles | Where-Object {
        (Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8) -match "clueID:\s+$([regex]::Escape($ClueID))(\r?\n|$)"
    } | Select-Object -First 1
    Assert-True ($null -ne $assetFile) "Missing active clue asset for clueID: $ClueID"
    return [regex]::Unescape((Get-Content -LiteralPath $assetFile.FullName -Raw -Encoding UTF8))
}

function Normalize-Text {
    param([string] $Value)
    return ($Value -replace '\s+', ' ').Trim()
}

Assert-True (Test-Path -LiteralPath $generatorPath) 'Missing ClueAssetGenerator.cs'
Assert-True (Test-Path -LiteralPath $normalCluePath) 'Missing normal clue asset folder.'
Assert-True (Test-Path -LiteralPath $keyCluePath) 'Missing key clue asset folder.'

$generator = Get-Content -LiteralPath $generatorPath -Raw -Encoding UTF8
$storyEntriesMatch = [regex]::Match($generator, 'CurrentStoryEntries\s*=\s*new\[\]\s*\{(?<body>[\s\S]*?)\};', 'Singleline')
Assert-True ($storyEntriesMatch.Success) 'ClueAssetGenerator must define CurrentStoryEntries.'
$storyEntries = $storyEntriesMatch.Groups['body'].Value

$expectedClues = @(
    @{ Id = 'normal_cast_notice'; Name = B '67Cw7JetIOyViOuCtOusuA=='; Description = B '7IiY7Iig7IukIOuLtOuLuTog7KeE7IS47JuFLCDrtIntg5ztmIQuIO2ZmOyekCDsl606IOycoOyViOuCmC4='; Notebook = B '7IiY7Iig7Iuk7JeQIOygkeq3vO2VoCDsiJgg7J6I7JeI642YIOyduOusvOydtCDtirnsoJXrkJzri6Qu' },
    @{ Id = 'normal_memorial_frame'; Name = B '7ZWY7Iuc7Zi4IOy2lOuqqCDslaHsnpA='; Description = B 'MeqwnOyblCDsoIQg7IKs66edLiDtlqXrhYQgMjLshLguIOuPmeyVhOumrCDrtoDsm5Ag7J2864+ZLg=='; Notebook = B '7LWc6re8IOuPmeyVhOumrOyXkOyEnCDsgqzrp53snpDqsIAg7J6I7JeI64ukLiDsgqzsnbjsnYAg67aI66qF7ZmV7ZWY64ukLg==' },
    @{ Id = 'normal_conversation_memo'; Name = B '7JOw66CI6riw7Ya1IOuplOuqqA=='; Description = B '7JWI64KYLCDrhKTqsIAg7ZWcIOynk+ydhCDsnorsp4Ag7JWK6rKg64ukLg=='; Notebook = B '7Jyg7JWI64KY7JeQ6rKMIOybkO2VnOydhCDtkojsnYAg7J2466y87J20IOyeiOuLpC4g7ZWE7LK066W8IOq4sOyWte2VtOuRrOyVvCDtlZzri6Qu' },
    @{ Id = 'normal_medical_certificate'; Name = B '7ZWY7Iuc7Zi4IOynhOuLqOyEnA=='; Description = B '66eQ6riwIOynhOuLqC4g64u064u57J2YOiDrtIntg5ztmIQu'; Notebook = B '7IKs66ed7ZWcIOu2gOybkCDtlZjsi5ztmLjripQg67aI7LmY67ORIO2ZmOyekOyYgOuLpC4=' },
    @{ Id = 'normal_ward_calendar'; Name = B '67OR7IukIOuCmeyEnA=='; Description = B '7JmcIOyCtOumtCDsiJgg7J6I7JeI64qU642wIOyCtOumrOyngCDslYrslZjslrQu'; Notebook = B '7ZWY7Iuc7Zi47J2YIOyjveydjOyXkCDrtoTrhbjtlZjripQg7J2466y87J20IOyeiOuLpC4=' },
    @{ Id = 'clue_hasho_will'; Name = B '7ZWY7Iuc7Zi4IOycoOyEnCDsgqzrs7g='; Description = B '7IK0IOydtOycoOulvCDsnoPsl4jri6QuIOyViOuCmOqwgCDqt7jqsbgg67m87JWX7JWEIOqwlOuLpC4='; Notebook = B '7ZWY7Iuc7Zi47J2YIOyjveydjOqzvCDsnKDslYjrgpgg7IKs7J207JeQIOyXsOqysOqzoOumrOqwgCDsnojri6Qu' },
    @{ Id = 'key_clue_coldest_place'; Name = B '7Lmo64yAIOuwkSDsqr3sp4A='; Description = B '7J20IOyViOyXkCDri7XsnbQg7J6I64ukLiDrgZ3quYzsp4Ag7LC+7JWE6528Lg=='; Notebook = B '64iE6rWw6rCAIOydtCDrs5Hsm5Ag7JWI7JeQIOustOyWuOqwgOulvCDsiKjqsqjrkoDri6Qu' },
    @{ Id = 'key_clue_temperature_warning'; Name = B '64OJ7J6lIOyVve2SiO2VqA=='; Description = B '7Jio64+EIOqyveqzoCDsiqTti7Dsu6QuIOusuCDrqqjshJzrpqzsl5Ag6riB7Z6MIOyekOq1reydtCDsnojri6Qu'; Notebook = B '64iE6rWw6rCAIOy1nOq3vOyXkCDsnbQg7JW97ZKI7ZWo7J2EIOyXtOyXiOuLpC4=' },
    @{ Id = 'normal_bong_rebuttal'; Name = B '67SJ7YOc7ZiEIOuplOuqqA=='; Description = B '7IS47JuF7JWEIOuCtOqwgCDrqLzsoIAg7IiY7Iig7IukIOuTpOyWtOqwiOqyjC4gLyDri7U6IOq0nOywruyVhCwg64K06rCAIO2VoOqyjC4='; Notebook = B '64u57J28IOyImOyIoOyLpCDsnoXsnqUg7Iic7ISc7JeQIOuzgOuPmeydtCDsnojsl4jri6Qu' },
    @{ Id = 'key_clue_fridge_scratches'; Name = B '7JW97ZKIIOuqqeuhne2RnA=='; Description = B '66qp66Gd7JeQIOyXhuuKlCDslb3tkojsnbQg7ZWY64KYIOuwmOy2nOuQkOuLpC4g64Kg7Kec64qUIOyYpOuKmC4='; Notebook = B '7Jik64qYIOuIhOq1sOqwgCDsnbQg67O06rSA7Iuk7JeQ7IScIOyVve2SiOydhCDqsIDsoLjqsJTri6Qu' },
    @{ Id = 'normal_makeup_toolbox'; Name = B '7KeE7IS47JuFIOu2hOyepeuMgA=='; Description = B '67aJ7J2AIO2OmOyduO2KuCDthrXsnbQg7Je066Ck7J6I64ukLiDrsJzqsIDrnb0g66qo7JaRIOyekOq1reydtCDssI3tmIDsnojri6Qu'; Notebook = B '64iE6rWw6rCAIOydtCDsnpDrpqzsl5DshJwg67Cc6rCA65297JeQIO2OmOyduO2KuOulvCDsuaDtlojri6Qu' },
    @{ Id = 'normal_sumi_memo'; Name = B '66y47IiY66+4IOydvOq4sOyepQ=='; Description = B '7Jik64qYIOustOyKqCDsnbzsnbQg7IOd6ri4IOqygyDqsJnri6QuIOuCmOyBnCDsmIjqsJDsnbQg65Og64ukLg=='; Notebook = B '7Jik64qYIOyCrOqxtOydhCDrr7jrpqwg64iI7LmY7LGIIOyduOusvOydtCDsnojsl4jrjZgg6rKDIOqwmeuLpC4=' },
    @{ Id = 'clue_makeup_diary'; Name = B '7KeE7IS47JuFIOydvOq4sOyepQ=='; Description = B '642UIOydtOyDgSDrr7jro7Ag7IiYIOyXhuuLpC4g7Jik64qY7J2064ukLg=='; Notebook = B '64iE6rWw6rCAIOyYpOuKmOydhCDsnITtlbQg7Jik656YIOq4sOuLpOugpOyZlOuLpC4=' },
    @{ Id = 'normal_under_table_space'; Name = B '7IiY7Iig64yAIOyVhOuemA=='; Description = B '7IKs656MIO2VnCDrqoXsnbQg7Iio7J2EIOyImCDsnojripQg6rO16rCELiDrsJTri6Xsl5Ag6riB7Z6MIOyekOq1reqzvCDrtonsnYAg7Y6Y7J247Yq4IOyekOq1reydtCDrgqjslYTsnojri6Qu'; Notebook = B '67KU7ZaJIOuLueyLnCDsiJjsiKDrjIAg7JWE656YIOuIhOq1sOqwgCDsiKjslrTsnojsl4jri6Qu' },
    @{ Id = 'normal_mirror_message'; Name = B '67K9IOuplOuqqA=='; Description = B '7ZWY7Iuc7Zi466W8IOychO2VtOyEnC4g66+47JWI7ZW0LCDslYjrgpguIOKAlCDsk7DroIjquLDthrUg66mU66qo7JmAIOqwmeydgCDtlYTssrQu'; Notebook = B '7JOw66CI6riw7Ya1IOuplOuqqOyZgCDqsJnsnYAg7ZWE7LK064ukLiDrkZAg66mU66qo66W8IOyTtCDsnbjrrLzsnbQg6rCZ64ukLg==' }
)

$entryCount = ([regex]::Matches($storyEntries, 'new\s+ClueEntry\s*\(')).Count
Assert-True ($entryCount -eq 15) "Prompt clue list must contain exactly 15 entries, found $entryCount."

foreach ($clue in $expectedClues) {
    $block = Get-EntryBlock $storyEntries $clue.Id
    foreach ($field in @('Name', 'Description', 'Notebook')) {
        Assert-True ($block.Contains($clue[$field])) "Generator clue $($clue.Id) missing prompt $field text: $($clue[$field])"
    }

    $assetText = Get-AssetTextByClueID $clue.Id
    $normalizedAssetText = Normalize-Text $assetText
    foreach ($field in @('Name', 'Description', 'Notebook')) {
        $expectedText = Normalize-Text $clue[$field]
        Assert-True ($normalizedAssetText.Contains($expectedText)) "Clue asset $($clue.Id) missing prompt $field text: $($clue[$field])"
    }
}

Write-Host 'Room02 prompt clue list checks passed.'
