param(
    [Parameter(Mandatory=$true)]
    [string]$ClassName,

    [Parameter(Mandatory=$true)]
    [string]$MethodName
)

$repoPath = "D:\Repos\_Ivy\Ivy-Framework\src\Ivy"

# Search for class definition
$classFiles = Get-ChildItem -Path $repoPath -Filter "$ClassName.cs" -Recurse

if ($classFiles.Count -eq 0) {
    Write-Host "Class $ClassName not found"
    exit 1
}

$classFile = $classFiles[0].FullName
$content = Get-Content $classFile -Raw

# Search for method
if ($content -match "public\s+\w+\s+$MethodName\s*\(") {
    Write-Host "Found: $ClassName.$MethodName"
    Write-Host "File: $classFile"

    # Extract method signature
    $lines = $content -split "`n"
    for ($i = 0; $i -lt $lines.Length; $i++) {
        if ($lines[$i] -match "public\s+\w+\s+$MethodName\s*\(") {
            Write-Host "Signature: $($lines[$i].Trim())"
            break
        }
    }
    exit 0
} else {
    Write-Host "Method $MethodName not found on $ClassName"
    exit 1
}
