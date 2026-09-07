#!/usr/bin/env bash
# =====================================================================
# Script de Copia de Seguridad en Caliente para SQLite en Ludeka
# Compatible con el modo WAL (Write-Ahead Logging) sin bloqueo de lectura
# =====================================================================

set -euo pipefail

CONTAINER_NAME="ludeka-web"
BACKUP_DIR="/var/lib/docker/volumes/ludeka_data/_data/backups"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILENAME="ludeka_backup_${TIMESTAMP}.db"
RETENTION_DAYS=7

echo "==> [$(date)] Iniciando copia de seguridad de la base de datos Ludeka..."

# Comprobar si el contenedor está en ejecución
if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
    echo "--> Contenedor '${CONTAINER_NAME}' detectado. Ejecutando .backup en caliente desde el contenedor..."
    docker exec "${CONTAINER_NAME}" mkdir -p /app/data/backups
    docker exec "${CONTAINER_NAME}" sqlite3 /app/data/ludeka.db ".backup '/app/data/backups/${BACKUP_FILENAME}'"
    
    # Comprobar integridad de la copia generada
    echo "--> Verificando integridad del respaldo..."
    INTEGRITY=$(docker exec "${CONTAINER_NAME}" sqlite3 "/app/data/backups/${BACKUP_FILENAME}" "PRAGMA integrity_check;")
    if [ "${INTEGRITY}" != "ok" ]; then
        echo "❌ ERROR: La verificación de integridad falló: ${INTEGRITY}"
        exit 1
    fi

    # Rotación: eliminar copias con más de 7 días
    echo "--> Aplicando política de retención (${RETENTION_DAYS} días)..."
    docker exec "${CONTAINER_NAME}" find /app/data/backups -type f -name "ludeka_backup_*.db" -mtime +${RETENTION_DAYS} -delete

    echo "✅ Copia de seguridad completada con éxito: /app/data/backups/${BACKUP_FILENAME}"
else
    echo "⚠️ Contenedor '${CONTAINER_NAME}' no encontrado en ejecución. Buscando ruta local en volumen..."
    if [ -d "${BACKUP_DIR}/.." ]; then
        mkdir -p "${BACKUP_DIR}"
        sqlite3 "${BACKUP_DIR}/../ludeka.db" ".backup '${BACKUP_DIR}/${BACKUP_FILENAME}'"
        find "${BACKUP_DIR}" -type f -name "ludeka_backup_*.db" -mtime +${RETENTION_DAYS} -delete
        echo "✅ Respaldo local completado: ${BACKUP_DIR}/${BACKUP_FILENAME}"
    else
        echo "❌ ERROR: No se encontró la base de datos ni el contenedor activo."
        exit 1
    fi
fi
