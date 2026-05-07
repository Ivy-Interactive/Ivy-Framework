<#
.SYNOPSIS
    Copies reference files from Ivy.Internals.Workflows.References into the skill folders,
    rewriting namespaces so they serve as clean templates.

.DESCRIPTION
    The canonical source of truth for all reference code lives in:
      - Ivy\Ivy.Internals.Workflows.References\  (shared .cs examples)
      - Ivy\Ivy.Internals\Workflows\Apps\CreateApp\AdHoc\References\  (AdHoc-specific .md)
      - Ivy-Framework\AGENTS.md  (framework documentation)
      - Ivy\Ivy.Internals\Workflows\Connections\GenerateDbConnection\  (DB generation references)
      - Ivy\Ivy.Internals\Workflows\Conversion\*\References\  (conversion mapping references)

    This script copies those files into src/skills/*/references/ and rewrites the
    original namespaces to generic "MyProject" namespaces so they work as templates.

.NOTES
    Run from the Ivy-Framework repo root, or pass -RefsRoot / -SkillsRoot explicitly.
#>
param(
    [string]$RefsRoot      = (Join-Path $PSScriptRoot '..\..\..\Ivy\Ivy.Internals.Workflows.References'),
    [string]$AdHocRoot     = (Join-Path $PSScriptRoot '..\..\..\Ivy\Ivy.Internals\Workflows\Apps\CreateApp\AdHoc\References'),
    [string]$GenDbRoot     = (Join-Path $PSScriptRoot '..\..\..\Ivy\Ivy.Internals\Workflows\Connections\GenerateDbConnection'),
    [string]$ConvRoot      = (Join-Path $PSScriptRoot '..\..\..\Ivy\Ivy.Internals\Workflows\Conversion'),
    [string]$FrameworkRoot = (Join-Path $PSScriptRoot '..\..'),
    [string]$SkillsRoot    = (Join-Path $PSScriptRoot '..\claude-plugin\skills')
)

$RefsRoot      = (Resolve-Path $RefsRoot).Path
$AdHocRoot     = (Resolve-Path $AdHocRoot).Path
$GenDbRoot     = (Resolve-Path $GenDbRoot).Path
$ConvRoot      = (Resolve-Path $ConvRoot).Path
$FrameworkRoot = (Resolve-Path $FrameworkRoot).Path
$SkillsRoot    = (Resolve-Path $SkillsRoot).Path

# ── Namespace replacements applied to every copied .cs file ──────────────────
$NamespaceReplacements = @(
    @{ From = 'Ivy.Internals.Workflows.References.Apps.Dashboard';              To = 'MyProject.Apps.Dashboard' }
    @{ From = 'Ivy.Internals.Workflows.References.Apps.Products';               To = 'MyProject.Apps.Products' }
    @{ From = 'Ivy.Internals.Workflows.References.Apps.Orders';                 To = 'MyProject.Apps.Orders' }
    @{ From = 'Ivy.Internals.Workflows.References.Apps';                        To = 'MyProject.Apps' }
    @{ From = 'Ivy.Internals.Workflows.References.Connections.IvyAgentExamples'; To = 'MyProject.Connections.MyDb' }
    @{ From = 'Ivy.Internals.Workflows.References.Connections.OpenAI';          To = 'MyProject.Connections.OpenAI' }
)

# ── Using-alias lines to strip (they reference internal type aliases) ────────
$AliasLinesToStrip = @(
    'using Exa = Ivy.Internals.Workflows.References.Connections.IvyAgentExamples;'
)

# ── Copy manifest ────────────────────────────────────────────────────────────
# Each entry: Source (relative to $RefsRoot or absolute), Destination (relative to $SkillsRoot)
$Copies = @(
    # ── ivy-create-dashboard ─────────────────────────────────────────────────
    @{ Src = 'Apps\DashboardApp.cs';                                  Dst = 'ivy-create-dashboard\references\DashboardApp.cs' }
    @{ Src = 'Apps\Dashboard\TotalSalesMetricView.cs';                Dst = 'ivy-create-dashboard\references\TotalSalesMetricView.cs' }
    @{ Src = 'Apps\Dashboard\OrdersMetricView.cs';                    Dst = 'ivy-create-dashboard\references\OrdersMetricView.cs' }
    @{ Src = 'Apps\Dashboard\CustomerRetentionRateMetricView.cs';     Dst = 'ivy-create-dashboard\references\CustomerRetentionRateMetricView.cs' }
    @{ Src = 'Apps\Dashboard\SalesByDayLineChartView.cs';             Dst = 'ivy-create-dashboard\references\SalesByDayLineChartView.cs' }
    @{ Src = 'Apps\Dashboard\OrdersByChannelPieChartView.cs';         Dst = 'ivy-create-dashboard\references\OrdersByChannelPieChartView.cs' }
    @{ Src = 'Apps\Dashboard\SalesByCategoryBarChartView.cs';         Dst = 'ivy-create-dashboard\references\SalesByCategoryBarChartView.cs' }
    @{ Src = 'Apps\Dashboard\SalesByStoreTypeAreaChartView.cs';       Dst = 'ivy-create-dashboard\references\SalesByStoreTypeAreaChartView.cs' }

    # ── ivy-create-crud ──────────────────────────────────────────────────────
    @{ Src = 'Apps\ProductsApp.cs';                                   Dst = 'ivy-create-crud\references\ProductsApp.cs' }
    @{ Src = 'Apps\OrdersApp.cs';                                     Dst = 'ivy-create-crud\references\OrdersApp.cs' }
    @{ Src = 'Apps\Products\ProductListBlade.cs';                     Dst = 'ivy-create-crud\references\ProductListBlade.cs' }
    @{ Src = 'Apps\Products\ProductDetailsBlade.cs';                  Dst = 'ivy-create-crud\references\ProductDetailsBlade.cs' }
    @{ Src = 'Apps\Products\ProductCreateDialog.cs';                  Dst = 'ivy-create-crud\references\ProductCreateDialog.cs' }
    @{ Src = 'Apps\Products\ProductEditSheet.cs';                     Dst = 'ivy-create-crud\references\ProductEditSheet.cs' }
    @{ Src = 'Apps\Orders\OrderListBlade.cs';                         Dst = 'ivy-create-crud\references\OrderListBlade.cs' }
    @{ Src = 'Apps\Orders\OrderDetailsBlade.cs';                      Dst = 'ivy-create-crud\references\OrderDetailsBlade.cs' }
    @{ Src = 'Apps\Orders\OrderCreateDialog.cs';                      Dst = 'ivy-create-crud\references\OrderCreateDialog.cs' }
    @{ Src = 'Apps\Orders\OrderEditSheet.cs';                         Dst = 'ivy-create-crud\references\OrderEditSheet.cs' }
    @{ Src = 'Apps\Orders\OrderLinesBlade.cs';                        Dst = 'ivy-create-crud\references\OrderLinesBlade.cs' }
    @{ Src = 'Apps\Orders\OrderLineCreateDialog.cs';                  Dst = 'ivy-create-crud\references\OrderLineCreateDialog.cs' }
    @{ Src = 'Apps\Orders\OrderLineEditSheet.cs';                     Dst = 'ivy-create-crud\references\OrderLineEditSheet.cs' }

    # ── ivy-create-app ───────────────────────────────────────────────────────
    @{ Src = 'Apps\ChatApp.cs';                                       Dst = 'ivy-create-app\references\ChatApp.cs' }
    @{ Src = 'Connections\OpenAI\OpenAIConnection.cs';                Dst = 'ivy-create-app\references\OpenAIConnection.cs' }
)

# AdHoc-specific references (source is $AdHocRoot, not $RefsRoot)
$AdHocCopies = @(
    @{ Src = 'DesignGuidelines.md'; Dst = 'ivy-create-app\references\DesignGuidelines.md' }
)

# GenerateDbConnection references (source is $GenDbRoot)
$GenDbCopies = @(
    @{ Src = 'GenerateAndReviewDbml\References\DbmlGeneratePrompt.md';              Dst = 'ivy-generate-db-connection\references\DbmlGeneratePrompt.md' }
    @{ Src = 'GenerateCodeAndMigrate\References\DbmlToDataContextPrompt.md';        Dst = 'ivy-generate-db-connection\references\DbmlToDataContextPrompt.md' }
    @{ Src = 'GenerateCodeAndMigrate\References\DbmlToDataContext-Sqlite.md';       Dst = 'ivy-generate-db-connection\references\DbmlToDataContext-Sqlite.md' }
    @{ Src = 'GenerateCodeAndMigrate\References\DbmlToDataContext-Postgres.md';     Dst = 'ivy-generate-db-connection\references\DbmlToDataContext-Postgres.md' }
    @{ Src = 'GenerateCodeAndMigrate\References\DbmlToDataContext-SqlServer.md';    Dst = 'ivy-generate-db-connection\references\DbmlToDataContext-SqlServer.md' }
    @{ Src = 'GenerateCodeAndMigrate\References\DbmlToDataContext-MySql.md';        Dst = 'ivy-generate-db-connection\references\DbmlToDataContext-MySql.md' }
    @{ Src = 'GenerateCodeAndMigrate\CreateSeeder\References\DataSeederPrompt.md';  Dst = 'ivy-generate-db-connection\references\DataSeederPrompt.md' }
    @{ Src = 'GenerateCodeAndMigrate\CreateSeeder\References\DataSeeder-Sqlite.md'; Dst = 'ivy-generate-db-connection\references\DataSeeder-Sqlite.md' }
    @{ Src = 'GenerateCodeAndMigrate\CreateSeeder\References\DataSeeder-Postgres.md'; Dst = 'ivy-generate-db-connection\references\DataSeeder-Postgres.md' }
    @{ Src = 'GenerateCodeAndMigrate\CreateSeeder\References\DataSeeder-SqlServer.md'; Dst = 'ivy-generate-db-connection\references\DataSeeder-SqlServer.md' }
    @{ Src = 'GenerateCodeAndMigrate\CreateSeeder\References\DataSeeder-MySql.md';  Dst = 'ivy-generate-db-connection\references\DataSeeder-MySql.md' }
)

# Conversion workflow references (source is $ConvRoot/<Name>/References/)
# Each conversion skill gets all .md files from its corresponding workflow References folder
$ConversionSkills = @(
    @{ Workflow = 'Streamlit'; Skill = 'ivy-convert-streamlit' }
    @{ Workflow = 'Retool';    Skill = 'ivy-convert-retool' }
    @{ Workflow = 'Lovable';   Skill = 'ivy-convert-lovable' }
    @{ Workflow = 'Odoo';      Skill = 'ivy-convert-odoo' }
    @{ Workflow = 'Reflex';    Skill = 'ivy-convert-reflex' }
)

# Framework-level files (source is $FrameworkRoot) — copied into each skill's references
$FrameworkCopies = @(
    @{ Src = 'AGENTS.md'; Dst = 'ivy\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-convert-airtable\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-convert-excel\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-convert-lovable\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-convert-odoo\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-convert-reflex\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-convert-retool\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-convert-streamlit\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-create-any-connection\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-create-app\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-create-auth-connection\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-create-crud\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-create-dashboard\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-create-db-connection\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-create-external-widget\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-create-graphql-connection\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-create-openapi-connection\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-create-soap-connection\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-create-theme\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-create-using-reference-connection\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-deploy-to-desktop\references\AGENTS.md' }
    @{ Src = 'AGENTS.md'; Dst = 'ivy-generate-db-connection\references\AGENTS.md' }
)

function Copy-WithRewrite {
    param(
        [string]$SourcePath,
        [string]$DestPath
    )

    if (-not (Test-Path $SourcePath)) {
        Write-Warning "Source not found: $SourcePath"
        return $false
    }

    $destDir = Split-Path $DestPath -Parent
    if (-not (Test-Path $destDir)) {
        New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    }

    $content = Get-Content $SourcePath -Raw -Encoding UTF8

    if ($SourcePath -match '\.cs$') {
        foreach ($alias in $AliasLinesToStrip) {
            $content = $content -replace [regex]::Escape($alias), ''
        }

        # Replace Exa.Line with Line (from the stripped alias)
        $content = $content -replace '\bExa\.', ''

        foreach ($r in $NamespaceReplacements) {
            $content = $content -replace [regex]::Escape($r.From), $r.To
        }

        # Replace IvyAgentExamplesContextFactory with MyDbContextFactory
        $content = $content -replace 'IvyAgentExamplesContextFactory', 'MyDbContextFactory'
    }

    # Normalize line endings and trim trailing blank lines
    $content = $content -replace "`r`n", "`n"
    $content = $content.TrimEnd("`n", " ") + "`n"

    Set-Content -Path $DestPath -Value $content -NoNewline -Encoding UTF8
    return $true
}

# ── Main ─────────────────────────────────────────────────────────────────────
Write-Host "Copying reference files..." -ForegroundColor Cyan
Write-Host "  Source (shared):     $RefsRoot"
Write-Host "  Source (adhoc):      $AdHocRoot"
Write-Host "  Source (gendb):      $GenDbRoot"
Write-Host "  Source (conversion): $ConvRoot"
Write-Host "  Source (framework):  $FrameworkRoot"
Write-Host "  Destination:         $SkillsRoot"
Write-Host ""

$copied = 0
$failed = 0

foreach ($entry in $Copies) {
    $src = Join-Path $RefsRoot  $entry.Src
    $dst = Join-Path $SkillsRoot $entry.Dst

    if (Copy-WithRewrite -SourcePath $src -DestPath $dst) {
        Write-Host "  OK  $($entry.Dst)" -ForegroundColor Green
        $copied++
    } else {
        $failed++
    }
}

foreach ($entry in $AdHocCopies) {
    $src = Join-Path $AdHocRoot  $entry.Src
    $dst = Join-Path $SkillsRoot $entry.Dst

    if (Copy-WithRewrite -SourcePath $src -DestPath $dst) {
        Write-Host "  OK  $($entry.Dst)" -ForegroundColor Green
        $copied++
    } else {
        $failed++
    }
}

foreach ($entry in $GenDbCopies) {
    $src = Join-Path $GenDbRoot   $entry.Src
    $dst = Join-Path $SkillsRoot  $entry.Dst

    if (Copy-WithRewrite -SourcePath $src -DestPath $dst) {
        Write-Host "  OK  $($entry.Dst)" -ForegroundColor Green
        $copied++
    } else {
        $failed++
    }
}

foreach ($conv in $ConversionSkills) {
    $refsDir = Join-Path $ConvRoot "$($conv.Workflow)\References"
    if (-not (Test-Path $refsDir)) {
        Write-Warning "Conversion references not found: $refsDir"
        $failed++
        continue
    }
    $files = Get-ChildItem -Path $refsDir -File -Filter '*.md'
    foreach ($file in $files) {
        $dst = Join-Path $SkillsRoot "$($conv.Skill)\references\$($file.Name)"
        if (Copy-WithRewrite -SourcePath $file.FullName -DestPath $dst) {
            Write-Host "  OK  $($conv.Skill)\references\$($file.Name)" -ForegroundColor Green
            $copied++
        } else {
            $failed++
        }
    }
}

foreach ($entry in $FrameworkCopies) {
    $src = Join-Path $FrameworkRoot $entry.Src
    $dst = Join-Path $SkillsRoot   $entry.Dst

    if (Copy-WithRewrite -SourcePath $src -DestPath $dst) {
        Write-Host "  OK  $($entry.Dst)" -ForegroundColor Green
        $copied++
    } else {
        $failed++
    }
}

Write-Host ""
Write-Host "Done. Copied $copied file(s), $failed failure(s)." -ForegroundColor Cyan
