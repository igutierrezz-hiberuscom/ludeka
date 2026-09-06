# Script de inicio rápido para Ludeka (.NET 10)
$ErrorActionPreference = "Stop"

# 1. Configurar SDK de .NET 10
$userDotnet = "$env:USERPROFILE\.dotnet"
if (Test-Path "$userDotnet\dotnet.exe") {
    $env:DOTNET_ROOT = $userDotnet
    $env:PATH = "$userDotnet;$env:PATH"
}

# 2. Compilar Tailwind CSS si npx está disponible
if (Get-Command npx.cmd -ErrorAction SilentlyContinue) {
    Write-Host "Compilando Tailwind CSS..." -ForegroundColor DarkGray
    Push-Location "src\Ludeka.Web"
    try {
        npx.cmd -y tailwindcss@3.4.17 -i ./Styles/input.css -o ./wwwroot/app.css --minify | Out-Null
    } catch {
        Write-Warning "No se pudo recompilar Tailwind CSS; usando app.css existente."
    } finally {
        Pop-Location
    }
}

Write-Host "Iniciando Ludeka en http://localhost:5081 ..." -ForegroundColor Cyan
dotnet run --project src/Ludeka.Web
