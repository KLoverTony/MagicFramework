param(
    [string]$ModRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

$spellDefsDir = Join-Path $ModRoot 'Defs\SpellDefs'
$researchDefsDir = Join-Path $ModRoot 'Defs\ResearchProjectDefs'
$outputPath = Join-Path $ModRoot 'Defs\ThingDefs\Items\MFV_GeneratedSpellScrolls.xml'
$recipeOutputPath = Join-Path $ModRoot 'Defs\RecipeDefs\MFV_GeneratedSpellScrollRecipes.xml'

function Get-ChildText {
    param(
        [System.Xml.XmlElement]$Element,
        [string]$Name
    )

    if ($null -eq $Element) {
        return $null
    }

    $node = $Element.SelectSingleNode($Name)
    if ($null -eq $node) {
        return $null
    }

    return $node.InnerText.Trim()
}

function Get-LearningNode {
    param([System.Xml.XmlElement]$Spell)

    return $Spell.SelectSingleNode('learning')
}

function Get-ResearchPrerequisites {
    param([System.Xml.XmlElement]$Learning)

    if ($null -eq $Learning) {
        return @()
    }

    $researchNodes = $Learning.SelectNodes('researchPrerequisites/li')
    if ($null -eq $researchNodes) {
        return @()
    }

    return @($researchNodes | ForEach-Object { $_.InnerText.Trim() } | Where-Object { $_ })
}

function Get-SpellTier {
    param([System.Xml.XmlElement]$Spell)

    $tierText = Get-ChildText -Element $Spell -Name 'meta/tier'
    $tier = 1
    if (![int]::TryParse($tierText, [ref]$tier)) {
        $tier = 1
    }

    return $tier
}

function Get-ResearchPrerequisitesFromProject {
    param([System.Xml.XmlElement]$ResearchProject)

    if ($null -eq $ResearchProject) {
        return @()
    }

    $nodes = $ResearchProject.SelectNodes('prerequisites/li | hiddenPrerequisites/li')
    if ($null -eq $nodes) {
        return @()
    }

    return @($nodes | ForEach-Object { $_.InnerText.Trim() } | Where-Object { $_ })
}

$researchPrerequisiteMap = @{}
$researchFiles = Get-ChildItem -Path $researchDefsDir -Filter '*.xml' -ErrorAction SilentlyContinue | Sort-Object Name
foreach ($file in $researchFiles) {
    [xml]$document = Get-Content -LiteralPath $file.FullName
    foreach ($researchProject in $document.SelectNodes('/Defs/ResearchProjectDef')) {
        $defName = Get-ChildText -Element $researchProject -Name 'defName'
        if ([string]::IsNullOrWhiteSpace($defName)) {
            continue
        }

        $researchPrerequisiteMap[$defName] = @(Get-ResearchPrerequisitesFromProject -ResearchProject $researchProject)
    }
}

function Add-ResearchAndAncestors {
    param(
        [string]$ResearchDefName,
        [hashtable]$Seen
    )

    if ([string]::IsNullOrWhiteSpace($ResearchDefName) -or $Seen.ContainsKey($ResearchDefName)) {
        return
    }

    $Seen[$ResearchDefName] = $true
    if (!$researchPrerequisiteMap.ContainsKey($ResearchDefName)) {
        return
    }

    foreach ($prerequisite in @($researchPrerequisiteMap[$ResearchDefName])) {
        Add-ResearchAndAncestors -ResearchDefName $prerequisite -Seen $Seen
    }
}

function Get-ResearchUnlockCount {
    param([string[]]$ResearchDefNames)

    $seen = @{}
    foreach ($researchDefName in @($ResearchDefNames)) {
        Add-ResearchAndAncestors -ResearchDefName $researchDefName -Seen $seen
    }

    return [Math]::Max(1, $seen.Count)
}

function Get-MarketValue {
    param([string[]]$ResearchDefNames)

    $unlockCount = Get-ResearchUnlockCount -ResearchDefNames $ResearchDefNames
    return 200 * $unlockCount
}

function New-RetryingXmlWriter {
    param(
        [string]$Path,
        [System.Xml.XmlWriterSettings]$Settings
    )

    for ($attempt = 1; $attempt -le 10; $attempt++) {
        try {
            $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
            return [System.Xml.XmlWriter]::Create([System.IO.Stream]$stream, $Settings)
        }
        catch [System.UnauthorizedAccessException] {
            if ($attempt -eq 10) {
                throw
            }
            Start-Sleep -Milliseconds (100 * $attempt)
        }
    }
}

$spellFiles = Get-ChildItem -Path $spellDefsDir -Filter '*.xml' | Sort-Object Name
$spellRecords = New-Object System.Collections.Generic.List[object]

foreach ($file in $spellFiles) {
    [xml]$document = Get-Content -LiteralPath $file.FullName
    foreach ($spell in $document.SelectNodes('/Defs/MagicFramework.Definitions.SpellDef')) {
        $defName = Get-ChildText -Element $spell -Name 'defName'
        if ([string]::IsNullOrWhiteSpace($defName)) {
            continue
        }

        $learning = Get-LearningNode -Spell $spell
        $canBeLearned = Get-ChildText -Element $learning -Name 'canBeLearned'
        if ($canBeLearned -and $canBeLearned.Equals('false', [System.StringComparison]::OrdinalIgnoreCase)) {
            continue
        }

        $label = Get-ChildText -Element $spell -Name 'label'
        if ([string]::IsNullOrWhiteSpace($label)) {
            $label = $defName
        }

        $researchPrerequisites = @(Get-ResearchPrerequisites -Learning $learning)
        $mayRequire = $spell.GetAttribute('MayRequire')

        $spellRecords.Add([pscustomobject]@{
            DefName = $defName
            Label = $label
            Tier = Get-SpellTier -Spell $spell
            MarketValue = Get-MarketValue -ResearchDefNames $researchPrerequisites
            Research = $researchPrerequisites
            MayRequire = $mayRequire
        })
    }
}

$settings = New-Object System.Xml.XmlWriterSettings
$settings.Indent = $true
$settings.IndentChars = '  '
$settings.Encoding = New-Object System.Text.UTF8Encoding($false)

$writer = New-RetryingXmlWriter -Path $outputPath -Settings $settings
try {
    $writer.WriteStartDocument()
    $writer.WriteStartElement('Defs')

    foreach ($record in $spellRecords) {
        $researchPrerequisites = @($record.Research)

        $writer.WriteStartElement('ThingDef')
        $writer.WriteAttributeString('ParentName', 'MFV_SpellScrollBase')
        if (![string]::IsNullOrWhiteSpace($record.MayRequire)) {
            $writer.WriteAttributeString('MayRequire', $record.MayRequire)
        }

        $writer.WriteElementString('defName', "MFV_SpellScroll_$($record.DefName)")
        $writer.WriteElementString('label', "spell scroll ($($record.Label))")
        $writer.WriteElementString('description', "A prepared arcane scroll that teaches a pawn how to cast $($record.Label).")
        $writer.WriteElementString('forceDebugSpawnable', 'true')

        if ($record.Tier -ge 3) {
            $writer.WriteStartElement('graphicData')
            $writer.WriteElementString('texPath', 'Things/Item/Spell scroll - major')
            $writer.WriteElementString('graphicClass', 'Graphic_Single')
            $writer.WriteElementString('drawSize', '0.8')
            $writer.WriteEndElement()
        }

        $writer.WriteStartElement('statBases')
        $writer.WriteElementString('MarketValue', $record.MarketValue.ToString([System.Globalization.CultureInfo]::InvariantCulture))
        $writer.WriteEndElement()

        $writer.WriteStartElement('comps')
        $writer.WriteAttributeString('Inherit', 'False')
        $writer.WriteStartElement('li')
        $writer.WriteAttributeString('Class', 'CompProperties_Forbiddable')
        $writer.WriteEndElement()
        $writer.WriteStartElement('li')
        $writer.WriteAttributeString('Class', 'CompProperties_Usable')
        $writer.WriteElementString('useJob', 'UseItem')
        $writer.WriteElementString('useLabel', 'Read scroll')
        $writer.WriteEndElement()
        $writer.WriteStartElement('li')
        $writer.WriteAttributeString('Class', 'MFVanilla.Core.CompProperties_UseEffectLearnSpell')
        $writer.WriteElementString('spell', $record.DefName)
        if ($researchPrerequisites.Count -gt 0) {
            $writer.WriteStartElement('requiredResearch')
            foreach ($researchDefName in $researchPrerequisites) {
                $writer.WriteElementString('li', $researchDefName)
            }
            $writer.WriteEndElement()
        }
        $writer.WriteEndElement()
        $writer.WriteStartElement('li')
        $writer.WriteAttributeString('Class', 'CompProperties_UseEffectDestroySelf')
        $writer.WriteEndElement()
        $writer.WriteEndElement()

        $writer.WriteEndElement()
    }

    $writer.WriteEndElement()
    $writer.WriteEndDocument()
}
finally {
    if ($null -ne $writer) {
        $writer.Dispose()
    }
}

Write-Host "Generated $($spellRecords.Count) spell scroll defs at $outputPath"

$recipeWriter = New-RetryingXmlWriter -Path $recipeOutputPath -Settings $settings
try {
    $recipeWriter.WriteStartDocument()
    $recipeWriter.WriteStartElement('Defs')

    foreach ($record in $spellRecords) {
        $researchPrerequisites = @($record.Research)

        $recipeWriter.WriteStartElement('RecipeDef')
        if (![string]::IsNullOrWhiteSpace($record.MayRequire)) {
            $recipeWriter.WriteAttributeString('MayRequire', $record.MayRequire)
        }
        $recipeWriter.WriteElementString('defName', "MFV_ScribeScroll_$($record.DefName)")
        $recipeWriter.WriteElementString('label', "scribe scroll ($($record.Label))")
        $recipeWriter.WriteElementString('description', "Prepare a spell scroll that teaches a pawn how to cast $($record.Label). The scribe must have the Arcane Gift and already know $($record.Label).")
        $recipeWriter.WriteElementString('jobString', "Scribing scroll ($($record.Label)).")
        $recipeWriter.WriteElementString('workAmount', (700 + ($record.Tier * 200)).ToString([System.Globalization.CultureInfo]::InvariantCulture))
        $recipeWriter.WriteElementString('workSpeedStat', 'GeneralLaborSpeed')
        $recipeWriter.WriteElementString('effectWorking', 'Tailor')
        $recipeWriter.WriteElementString('soundWorking', 'Recipe_Tailor')
        if ($researchPrerequisites.Count -gt 0) {
            $recipeWriter.WriteElementString('researchPrerequisite', $researchPrerequisites[0])
        }

        $recipeWriter.WriteStartElement('ingredients')
        $recipeWriter.WriteStartElement('li')
        $recipeWriter.WriteStartElement('filter')
        $recipeWriter.WriteStartElement('thingDefs')
        if ($record.Tier -lt 3) {
            $recipeWriter.WriteElementString('li', 'MFV_Papyrus')
        }
        $recipeWriter.WriteElementString('li', 'MFV_Parchment')
        $recipeWriter.WriteEndElement()
        $recipeWriter.WriteEndElement()
        $recipeWriter.WriteElementString('count', '1')
        $recipeWriter.WriteEndElement()
        $recipeWriter.WriteStartElement('li')
        $recipeWriter.WriteStartElement('filter')
        $recipeWriter.WriteStartElement('thingDefs')
        $recipeWriter.WriteElementString('li', 'MFV_ArcaneInk')
        $recipeWriter.WriteEndElement()
        $recipeWriter.WriteEndElement()
        $recipeWriter.WriteElementString('count', '1')
        $recipeWriter.WriteEndElement()
        $recipeWriter.WriteEndElement()

        $recipeWriter.WriteStartElement('fixedIngredientFilter')
        $recipeWriter.WriteStartElement('thingDefs')
        if ($record.Tier -lt 3) {
            $recipeWriter.WriteElementString('li', 'MFV_Papyrus')
        }
        $recipeWriter.WriteElementString('li', 'MFV_Parchment')
        $recipeWriter.WriteElementString('li', 'MFV_ArcaneInk')
        $recipeWriter.WriteEndElement()
        $recipeWriter.WriteEndElement()

        $recipeWriter.WriteStartElement('products')
        $recipeWriter.WriteElementString("MFV_SpellScroll_$($record.DefName)", '1')
        $recipeWriter.WriteEndElement()

        $recipeWriter.WriteElementString('workSkill', 'Crafting')
        $recipeWriter.WriteElementString('requiredGiverWorkType', 'Crafting')
        $recipeWriter.WriteStartElement('modExtensions')
        $recipeWriter.WriteStartElement('li')
        $recipeWriter.WriteAttributeString('Class', 'MFVanilla.Core.ScribeSpellScrollRecipeExtension')
        $recipeWriter.WriteElementString('spell', $record.DefName)
        $recipeWriter.WriteEndElement()
        $recipeWriter.WriteEndElement()
        $recipeWriter.WriteStartElement('recipeUsers')
        $recipeWriter.WriteElementString('li', 'MFV_ScribingTable')
        $recipeWriter.WriteEndElement()
        $recipeWriter.WriteElementString('displayPriority', (1000 - ($record.Tier * 100)).ToString([System.Globalization.CultureInfo]::InvariantCulture))
        $recipeWriter.WriteEndElement()
    }

    $recipeWriter.WriteEndElement()
    $recipeWriter.WriteEndDocument()
}
finally {
    if ($null -ne $recipeWriter) {
        $recipeWriter.Dispose()
    }
}

Write-Host "Generated $($spellRecords.Count) spell scroll recipe defs at $recipeOutputPath"
