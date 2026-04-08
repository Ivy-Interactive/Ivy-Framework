# NotifySlack.Tests.ps1 — Pester v5 tests for NotifySlack.ps1 YAML parsing
# Requirements: Pester v5.0.0+
# Run: Invoke-Pester -Path ./NotifySlack.Tests.ps1 -Output Detailed

BeforeAll {
    if (-not (Get-Module -ListAvailable -Name powershell-yaml)) {
        Install-Module -Name powershell-yaml -Force -Scope CurrentUser
    }
    Import-Module powershell-yaml
}

Describe "plan.yaml parsing with ConvertFrom-Yaml" {
    It "parses simple title and project" {
        $yaml = @"
state: Executing
project: Tendril
title: Fix the login bug
prs:
- https://github.com/owner/repo/pull/1
commits: []
"@
        $plan = ConvertFrom-Yaml $yaml
        $plan.title | Should -Be "Fix the login bug"
        $plan.project | Should -Be "Tendril"
    }

    It "parses title with multi-line quoted string" {
        $yaml = @"
state: Executing
project: Framework
title: "Fix the bug that spans multiple lines"
prs: []
"@
        $plan = ConvertFrom-Yaml $yaml
        $plan.title | Should -Be "Fix the bug that spans multiple lines"
    }

    It "parses title with special characters in quotes" {
        $yaml = @"
state: Executing
project: Agent
title: 'Fix: Add support for special values'
prs: []
"@
        $plan = ConvertFrom-Yaml $yaml
        $plan.title | Should -Be "Fix: Add support for special values"
    }

    It "parses YAML block scalar title" {
        $yaml = @"
state: Draft
project: Tendril
title: |
  Multi-line title
  with literal newlines
prs: []
"@
        $plan = ConvertFrom-Yaml $yaml
        $plan.title | Should -Match "Multi-line title"
    }

    It "parses empty prs list" {
        $yaml = @"
state: Draft
project: Tendril
title: Test
prs: []
"@
        $plan = ConvertFrom-Yaml $yaml
        $prs = if ($plan.prs) { @($plan.prs) } else { @() }
        $prs.Count | Should -Be 0
    }

    It "parses multiple PRs" {
        $yaml = @"
state: Draft
project: Tendril
title: Test
prs:
- https://github.com/owner/repo/pull/1
- https://github.com/owner/repo/pull/2
- https://github.com/owner/other/pull/99
"@
        $plan = ConvertFrom-Yaml $yaml
        $prs = @($plan.prs)
        $prs.Count | Should -Be 3
        $prs[0] | Should -Be "https://github.com/owner/repo/pull/1"
        $prs[2] | Should -Be "https://github.com/owner/other/pull/99"
    }
}

Describe "config.yaml slackEmoji parsing with ConvertFrom-Yaml" {
    It "extracts slackEmoji for a specific project" {
        $yaml = @"
projects:
  - name: Framework
    color: Blue
    meta:
      slackEmoji: ":hammer_and_wrench:"
  - name: Tendril
    color: Emerald
    meta:
      slackEmoji: ":seedling:"
"@
        $config = ConvertFrom-Yaml $yaml
        $projectConfig = $config.projects | Where-Object { $_.name -eq "Tendril" } | Select-Object -First 1
        $projectConfig.meta.slackEmoji | Should -Be ":seedling:"
    }

    It "returns empty when project has no slackEmoji" {
        $yaml = @"
projects:
  - name: Internal
    color: Gray
"@
        $config = ConvertFrom-Yaml $yaml
        $projectConfig = $config.projects | Where-Object { $_.name -eq "Internal" } | Select-Object -First 1
        $slackEmoji = ""
        if ($projectConfig -and $projectConfig.meta -and $projectConfig.meta.slackEmoji) {
            $slackEmoji = $projectConfig.meta.slackEmoji
        }
        $slackEmoji | Should -Be ""
    }

    It "handles project not found in config" {
        $yaml = @"
projects:
  - name: Framework
    meta:
      slackEmoji: ":hammer_and_wrench:"
"@
        $config = ConvertFrom-Yaml $yaml
        $projectConfig = $config.projects | Where-Object { $_.name -eq "NonExistent" } | Select-Object -First 1
        $projectConfig | Should -BeNullOrEmpty
    }
}
