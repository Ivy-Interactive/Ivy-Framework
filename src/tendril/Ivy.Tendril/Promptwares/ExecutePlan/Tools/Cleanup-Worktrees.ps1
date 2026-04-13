param(
    [Parameter(Mandatory = $true)]
    [string]$PlanPath
)

$worktreesDir = Join-Path $PlanPath "worktrees"
if (-not (Test-Path $worktreesDir)) {
    return
}

Get-ChildItem $worktreesDir -Directory | ForEach-Object {
    $wtDir = $_.FullName
    $repoName = $_.Name

    Write-Host "Removing worktree: $repoName"

    $gitFile = Join-Path $wtDir ".git"
    if (Test-Path $gitFile) {
        $gitContent = Get-Content $gitFile -Raw
        if ($gitContent -match 'gitdir:\s*(.+)') {
            $gitDir = $Matches[1].Trim()
            $repoGitDir = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($gitDir, "..", ".."))
            $repoRoot = [System.IO.Path]::GetDirectoryName($repoGitDir)

            if ($repoRoot -and (Test-Path $repoRoot)) {
                Push-Location $repoRoot
                git worktree remove $wtDir --force 2>$null
                Pop-Location
            }
        }
    }

    if (Test-Path $wtDir) {
        Remove-Item $wtDir -Recurse -Force -ErrorAction SilentlyContinue

        if (Test-Path $wtDir) {
            cmd /c "rmdir /s /q `"$wtDir`"" 2>$null
        }
    }
}

if (Test-Path $worktreesDir) {
    Remove-Item $worktreesDir -Recurse -Force -ErrorAction SilentlyContinue
}
