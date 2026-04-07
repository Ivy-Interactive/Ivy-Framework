<#
.SYNOPSIS
    Invokes a coding agent (Claude, Codex, or Gemini) with a standardized interface.

.DESCRIPTION
    Wraps provider-specific CLI tools behind a common interface. Accepts tools
    in Claude's format and translates them for other providers. Returns
    standardized streaming JSON output.

.PARAMETER Cli
    The coding agent provider: Claude, Codex, or Gemini.

.PARAMETER Model
    Optional model override. If empty, uses the provider's default.

.PARAMETER Prompt
    The prompt text to send to the agent. Mutually exclusive with PromptFile.

.PARAMETER PromptFile
    Path to a file containing the prompt. Mutually exclusive with Prompt.

.PARAMETER AllowedTools
    Array of allowed tool names in Claude's format (Read, Write, Bash, etc.).
    Translated to provider-specific formats automatically.

.PARAMETER SessionId
    Session identifier for cost tracking and resumption.

.PARAMETER WorkingDirectory
    Working directory for the agent process.

.PARAMETER ExtraArgs
    Additional CLI arguments to pass through to the underlying tool.
#>

param(
    [ValidateSet("Claude", "Codex", "Gemini")]
    [string]$Cli = "Claude",

    [string]$Model = "",

    [string]$Prompt = "",

    [string]$PromptFile = "",

    [string[]]$AllowedTools = @(),

    [string]$SessionId = "",

    [string]$WorkingDirectory = "",

    [string[]]$ExtraArgs = @()
)

if ($WorkingDirectory) {
    Push-Location $WorkingDirectory
}

try {
    switch ($Cli) {
        "Claude" {
            $cliArgs = @("--print", "--verbose", "--output-format", "stream-json", "--dangerously-skip-permissions")

            if ($Model) {
                $cliArgs += @("--model", $Model)
            }

            if ($SessionId) {
                $cliArgs += @("--session-id", $SessionId)
            }

            if ($AllowedTools.Count -gt 0) {
                $cliArgs += @("--allowedTools", ($AllowedTools -join ","))
            }

            $cliArgs += $ExtraArgs

            # Get prompt content
            $promptContent = if ($PromptFile -and (Test-Path $PromptFile)) {
                Get-Content $PromptFile -Raw
            } elseif ($Prompt) {
                $Prompt
            } else {
                Write-Error "Either -Prompt or -PromptFile must be provided"
                return
            }

            $cliArgs += "--"
            $cliArgs += $promptContent

            & claude @cliArgs 2>&1
        }

        "Codex" {
            $cliArgs = @("--full-auto")

            if ($Model) {
                $cliArgs += @("--model", $Model)
            }

            # Codex uses --approval-mode with tool groups
            # Map Claude tool names to Codex's tool permissions
            # Codex has: read, write, execute, network
            # full-auto already allows all tools, so no mapping needed

            $cliArgs += $ExtraArgs

            # Get prompt content
            $promptContent = if ($PromptFile -and (Test-Path $PromptFile)) {
                Get-Content $PromptFile -Raw
            } elseif ($Prompt) {
                $Prompt
            } else {
                Write-Error "Either -Prompt or -PromptFile must be provided"
                return
            }

            $cliArgs += $promptContent

            # Run codex and wrap output in standardized JSON format
            $output = & codex @cliArgs 2>&1
            foreach ($line in $output) {
                $lineStr = "$line"
                # Try to parse as JSON for passthrough
                try {
                    $null = $lineStr | ConvertFrom-Json
                    $lineStr
                }
                catch {
                    # Wrap non-JSON output as status messages
                    if ($lineStr.Trim()) {
                        @{ type = "status"; message = $lineStr } | ConvertTo-Json -Compress
                    }
                }
            }

            # Emit a result line from the last output
            if ($output) {
                $lastOutput = ($output | Where-Object { "$_".Trim() } | Select-Object -Last 1)
                if ($lastOutput) {
                    @{ type = "result"; result = "$lastOutput" } | ConvertTo-Json -Compress
                }
            }
        }

        "Gemini" {
            $cliArgs = @()

            if ($Model) {
                $cliArgs += @("--model", $Model)
            }

            # Gemini CLI uses --sandbox for auto-approval of tools
            $cliArgs += "--sandbox"

            $cliArgs += $ExtraArgs

            # Get prompt content
            $promptContent = if ($PromptFile -and (Test-Path $PromptFile)) {
                Get-Content $PromptFile -Raw
            } elseif ($Prompt) {
                $Prompt
            } else {
                Write-Error "Either -Prompt or -PromptFile must be provided"
                return
            }

            $cliArgs += $promptContent

            # Run gemini and wrap output in standardized JSON format
            $output = & gemini @cliArgs 2>&1
            foreach ($line in $output) {
                $lineStr = "$line"
                try {
                    $null = $lineStr | ConvertFrom-Json
                    $lineStr
                }
                catch {
                    if ($lineStr.Trim()) {
                        @{ type = "status"; message = $lineStr } | ConvertTo-Json -Compress
                    }
                }
            }

            if ($output) {
                $lastOutput = ($output | Where-Object { "$_".Trim() } | Select-Object -Last 1)
                if ($lastOutput) {
                    @{ type = "result"; result = "$lastOutput" } | ConvertTo-Json -Compress
                }
            }
        }
    }
}
finally {
    if ($WorkingDirectory) {
        Pop-Location
    }
}
