# Script de verificación y prueba en vivo para Google Gemini API en Ludeka
$ErrorActionPreference = "Stop"

Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host "       Ludeka - Verificador de Google Gemini API       " -ForegroundColor Cyan
Write-Host "=======================================================" -ForegroundColor Cyan

# 1. Recuperar API Key
$apiKey = $env:Gemini__ApiKey

if ([string]::IsNullOrWhiteSpace($apiKey)) {
    # Intentar leer desde User Secrets de .NET
    try {
        $secretsJson = dotnet user-secrets list --project src/Ludeka.Web/Ludeka.Web.csproj 2>$null
        foreach ($line in $secretsJson) {
            if ($line -match "Gemini:ApiKey\s*=\s*(.+)") {
                $apiKey = $matches[1].Trim()
                break
            }
        }
    } catch {}
}

if ([string]::IsNullOrWhiteSpace($apiKey)) {
    # Intentar leer desde variable de entorno de Usuario en Windows
    $apiKey = [Environment]::GetEnvironmentVariable("Gemini__ApiKey", "User")
}

if ([string]::IsNullOrWhiteSpace($apiKey)) {
    Write-Host "[!] No se encontro la clave Gemini__ApiKey en el entorno actual." -ForegroundColor Yellow
    Write-Host "    Introduce tu API Key de Gemini a continuacion para probarla:" -ForegroundColor Yellow
    $inputKey = Read-Host "    Gemini API Key"
    if ([string]::IsNullOrWhiteSpace($inputKey)) {
        Write-Host "[-] Operacion cancelada: No se proporciono ninguna clave." -ForegroundColor Red
        exit 1
    }
    $apiKey = $inputKey.Trim()
}

$maskedKey = $apiKey.Substring(0, [Math]::Min(6, $apiKey.Length)) + "..." + $apiKey.Substring([Math]::Max(0, $apiKey.Length - 4))
Write-Host "[+] Clave detectada: $maskedKey" -ForegroundColor Green

# 2. Configurar Simulate=false
$env:Gemini__Simulate = "false"
$model = if ($env:Gemini__Model) { $env:Gemini__Model } else { "gemini-3.6-flash" }
Write-Host "[+] Modelo seleccionado: $model" -ForegroundColor Green
Write-Host "[+] Conectando con Google AI Studio..." -ForegroundColor Cyan

# 3. Payload de prueba para Ludeka
$testPayload = @{
    contents = @(
        @{
            parts = @(
                @{
                    text = "Eres el analista lúdico de Ludeka. Analiza brevemente el juego 'Ark Nova' (eurogame de peso medio-alto de gestión zoológica, 1-4 jugadores, 14+ años). Devuelve estrictamente un JSON con las claves: generalVerdict, scalabilitySummary, ageSummary, footprintSummary en español."
                }
            )
        }
    )
    generationConfig = @{
        responseMimeType = "application/json"
        temperature = 0.2
    }
} | ConvertTo-Json -Depth 5

$url = "https://generativelanguage.googleapis.com/v1beta/models/${model}:generateContent?key=${apiKey}"

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $response = Invoke-RestMethod -Uri $url -Method Post -Body $testPayload -ContentType "application/json; charset=utf-8"
    $stopwatch.Stop()

    Write-Host "[OK] Conexion exitosa con Google Gemini! Tiempo: $($stopwatch.ElapsedMilliseconds) ms" -ForegroundColor Green
    Write-Host ""
    Write-Host "--- Respuesta Estructurada Obtenida ---" -ForegroundColor Yellow

    $candidateText = $response.candidates[0].content.parts[0].text
    $parsed = $candidateText | ConvertFrom-Json

    Write-Host "Veredicto General:" -ForegroundColor Cyan
    Write-Host $parsed.generalVerdict
    Write-Host ""
    Write-Host "Escalabilidad:" -ForegroundColor Cyan
    Write-Host $parsed.scalabilitySummary
    Write-Host ""
    Write-Host "Accesibilidad y Edad:" -ForegroundColor Cyan
    Write-Host $parsed.ageSummary
    Write-Host ""
    Write-Host "Huella en Mesa:" -ForegroundColor Cyan
    Write-Host $parsed.footprintSummary
    Write-Host ""
    Write-Host "=======================================================" -ForegroundColor Cyan
    Write-Host "[EXITO] Tu API Key es 100% valida y funciona con Ludeka." -ForegroundColor Green
    Write-Host "Para usarla en la web, ejecuta: ./dev.ps1" -ForegroundColor Green
    Write-Host "=======================================================" -ForegroundColor Cyan
}
catch {
    $stopwatch.Stop()
    Write-Host "[-] Error al conectar con Google Gemini API:" -ForegroundColor Red
    $err = $_.Exception.Message
    Write-Host "    Detalle: $err" -ForegroundColor Red
    if ($_.Exception.Response) {
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $errBody = $reader.ReadToEnd()
            Write-Host "    Cuerpo de error: $errBody" -ForegroundColor DarkRed
        } catch {}
    }
    Write-Host ""
    Write-Host "Recomendaciones:" -ForegroundColor Yellow
    Write-Host "1. Verifica que la clave en https://aistudio.google.com/ sea correcta." -ForegroundColor White
    Write-Host "2. Asegurate de que tu clave no tenga espacios adicionales al principio o final." -ForegroundColor White
    exit 1
}
