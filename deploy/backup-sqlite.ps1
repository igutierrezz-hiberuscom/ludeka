# =====================================================================
# Script de Copia de Seguridad para SQLite en Ludeka (PowerShell / Windows)
# =====================================================================

$ErrorActionPreference = "Stop"

$DatabasePath = "ludeka.db"
$BackupDir = "backups"
$RetentionDays = 7
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$BackupFile = Join-Path $BackupDir "ludeka_backup_$Timestamp.db"

Write-Host "==> Iniciando copia de seguridad de Ludeka SQLite..." -ForegroundColor Cyan

if (-not (Test-Path $DatabasePath)) {
    Write-Warning "No se encontró el archivo de base de datos '$DatabasePath'."
    exit 0
}

if (-not (Test-Path $BackupDir)) {
    New-Item -ItemType Directory -Path $BackupDir | Out-Null
}

# Realizar copia de seguridad segura
Copy-Item -Path $DatabasePath -Destination $BackupFile -Force
Write-Host "--> Copia de seguridad guardada en: $BackupFile" -ForegroundColor Green

# Rotación de copias antiguas (más de 7 días)
$LimitDate = (Get-Date).AddDays(-$RetentionDays)
Get-ChildItem -Path $BackupDir -Filter "ludeka_backup_*.db" | Where-Object { $_.CreationTime -lt $LimitDate } | ForEach-Object {
    Write-Host "--> Eliminando copia antigua: $($_.Name)" -ForegroundColor DarkGray
    Remove-Item $_.FullName -Force
}

Write-Host "✅ Proceso de copia de seguridad finalizado correctamente." -ForegroundColor Green
