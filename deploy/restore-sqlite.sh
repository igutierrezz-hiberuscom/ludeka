#!/usr/bin/env bash
# =====================================================================
# Script de Restauración Segura para SQLite en Ludeka
# =====================================================================

set -euo pipefail

if [ "$#" -ne 1 ]; then
    echo "Uso: $0 <ruta_al_archivo_de_backup.db>"
    exit 1
fi

BACKUP_SOURCE="$1"
CONTAINER_NAME="ludeka-web"

if [ ! -f "${BACKUP_SOURCE}" ]; then
    echo "❌ ERROR: El archivo de respaldo '${BACKUP_SOURCE}' no existe."
    exit 1
fi

echo "==> Verificando integridad del archivo de respaldo..."
INTEGRITY=$(sqlite3 "${BACKUP_SOURCE}" "PRAGMA integrity_check;")
if [ "${INTEGRITY}" != "ok" ]; then
    echo "❌ ERROR: El archivo de respaldo está corrupto o es inválido: ${INTEGRITY}"
    exit 1
fi
echo "--> Integridad verificada: OK."

read -p "¿Confirmas la restauración de la base de datos? (s/N): " CONFIRM
if [[ ! "${CONFIRM}" =~ ^[sS]$ ]]; then
    echo "Restauración cancelada por el usuario."
    exit 0
fi

echo "--> Deteniendo contenedor '${CONTAINER_NAME}'..."
docker stop "${CONTAINER_NAME}" || true

DATA_VOLUME_PATH="/var/lib/docker/volumes/ludeka_data/_data"
CURRENT_DB="${DATA_VOLUME_PATH}/ludeka.db"

if [ -f "${CURRENT_DB}" ]; then
    SAFEGUARD_BACKUP="${DATA_VOLUME_PATH}/ludeka_pre_restore_$(date +%Y%m%d_%H%M%S).db"
    echo "--> Creando copia de salvaguarda de la base de datos actual en: ${SAFEGUARD_BACKUP}"
    cp "${CURRENT_DB}" "${SAFEGUARD_BACKUP}"
    # Eliminar posibles ficheros WAL/SHM remanentes
    rm -f "${CURRENT_DB}-wal" "${CURRENT_DB}-shm"
fi

echo "--> Restaurando archivo..."
cp "${BACKUP_SOURCE}" "${CURRENT_DB}"
chmod 664 "${CURRENT_DB}"

echo "--> Reiniciando contenedor '${CONTAINER_NAME}'..."
docker start "${CONTAINER_NAME}"

echo "✅ Restauración completada con éxito."
