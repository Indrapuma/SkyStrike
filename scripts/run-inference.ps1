$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoDir = Split-Path -Parent $scriptDir
$modelPath = Join-Path $repoDir 'artifacts\rl\qtable.json'
$dotnetExe = 'C:\Program Files\dotnet\dotnet.exe'

if (-not (Test-Path $modelPath)) {
    Write-Host "Saved Q-table not found: $modelPath"
    Write-Host 'Run training first so the game can produce artifacts\rl\qtable.json'
    exit 1
}

Push-Location $repoDir
try {
    & $dotnetExe run --project .\GameCSharp\GameCSharp.csproj -- --inference
}
finally {
    Pop-Location
}