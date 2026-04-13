param(
    [string]$SessionFile
)

# Read session.ldjson and extract token usage at generation boundaries
$generations = @()

Get-Content $SessionFile | ForEach-Object {
    $event = $_ | ConvertFrom-Json

    if ($event.eventName -eq "UITokenUsageMessage") {
        $generations += [PSCustomObject]@{
            GenerationNumber = $event.generationNumber
            InputTokens = $event.inputTokens
            OutputTokens = $event.outputTokens
            TotalTokens = $event.inputTokens + $event.outputTokens
            Timestamp = $event.timestamp
        }
    }
}

return $generations | Format-Table -AutoSize
