# PowerShell Script Output Capture from Bash

Running `pwsh -NoProfile -File script.ps1` from bash does not reliably capture stdout — output is often empty, especially when the script uses `return` or `Write-Output` and is run in background.

## Workaround

Instead of running the .ps1 script, replicate its logic directly in bash:
1. Call `storage upload ivy-tendril <file>` directly from a bash loop
2. The SAS token is shared across all uploads in a session — capture it from one upload and reuse it for URL construction
3. Construct artifact markdown in bash rather than relying on PowerShell output

## When this applies

Whenever Upload-Artifacts.ps1 (or similar pwsh scripts) need to return output to the caller in bash.
