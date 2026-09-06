# Script de ejecución de tests para Ludeka (.NET 10)
$ErrorActionPreference = "Stop"

$userDotnet = "$env:USERPROFILE\.dotnet"
if (Test-Path "$userDotnet\dotnet.exe") {
    $env:DOTNET_ROOT = $userDotnet
    $env:PATH = "$userDotnet;$env:PATH"
}

Write-Host "Ejecutando suite de pruebas unitarias..." -ForegroundColor Cyan
dotnet test src/Ludeka.slnx
