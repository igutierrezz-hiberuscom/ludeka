# 🚀 Guía Operativa de Despliegue en Producción — Ludeka

Esta guía describe el procedimiento técnico para desplegar, operar y mantener la plataforma **Ludeka** (.NET 10 y Blazor Web App) en cualquier servidor VPS Linux (Ubuntu 22.04/24.04 LTS o Debian 12) utilizando Docker, Docker Compose y Nginx.

---

## 1. Requisitos de Infraestructura Recomendados

- **Servidor VPS:** Hetzner Cloud, DigitalOcean, OVH, AWS EC2, Azure VM.
- **CPU:** 2 vCPU (arquitectura x86_64 o ARM64).
- **Memoria RAM:** Mínimo 2 GB (4 GB recomendado para holgura de compilación y caché).
- **Almacenamiento:** 20 GB SSD / NVMe.
- **Sistema Operativo:** Ubuntu 24.04 LTS o Debian 12 con acceso `sudo`.
- **Puertos de Red Abiertos:** `80` (HTTP), `443` (HTTPS) y `22` (SSH).

---

## 2. Instalación de Docker y Docker Compose en el VPS

En una sesión SSH en el servidor:

```bash
# 1. Actualizar repositorios e instalar paquetes base
sudo apt update && sudo apt install -y curl git ufw ca-certificates gnupg

# 2. Instalar Docker Engine oficial
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt update && sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# 3. Permitir ejecución sin sudo para el usuario actual
sudo usermod -aG docker $USER
newgrp docker
```

Configurar el cortafuegos UFW:
```bash
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
```

---

## 3. Clonación del Proyecto y Configuración de Secretos

```bash
# Clonar el repositorio oficial
git clone https://github.com/igutierrezz-hiberuscom/ludeka.git /opt/ludeka
cd /opt/ludeka

# Crear el archivo de entorno de producción
cp .env.example .env

# Editar secretos de producción
nano .env
```

Asegúrate de configurar:
- `PORT=5081` (puerto interno en el que escucha el contenedor frente al host).
- `ConnectionStrings__DefaultConnection=Data Source=/app/data/ludeka.db`.
- Tokens de BGG, Discord y Telegram según corresponda.
- Establecer `CommunityNotifications__DryRun=false` para habilitar notificaciones reales a la comunidad.

---

## 4. Despliegue con Docker Compose

Para construir la imagen multi-stage (compilación de Tailwind CSS + .NET 10 SDK) y levantar el servicio en segundo plano:

```bash
# Construir y arrancar el contenedor
docker compose up -d --build

# Verificar estado y logs
docker compose ps
docker compose logs -f ludeka-web
```

---

## 5. Endpoints de Diagnóstico y Salud (Health Checks)

El contenedor incluye dos sondas HTTP estándar de ASP.NET Core:

1. **Liveness Probe (`/healthz`):**
   ```bash
   curl -i http://localhost:5081/healthz
   ```
   *Respuesta esperada:* HTTP `200 OK` con `{"status":"Healthy","mode":"liveness"}`.

2. **Readiness Probe (`/ready`):**
   ```bash
   curl -i http://localhost:5081/ready
   ```
   *Respuesta esperada:* HTTP `200 OK` evaluando conectividad con SQLite, permisos de escritura en el volumen y el estado de la cola en segundo plano.

---

## 6. Configuración de Nginx Frontal y Certificado SSL (HTTPS)

### 6.1 Instalar Nginx y Certbot en el Host
```bash
sudo apt install -y nginx certbot python3-certbot-nginx
```

### 6.2 Copiar Configuración del Servidor
```bash
# Copiar archivo de configuración virtual
sudo cp /opt/ludeka/deploy/nginx/default.conf /etc/nginx/sites-available/ludeka.conf
sudo ln -s /etc/nginx/sites-available/ludeka.conf /etc/nginx/sites-enabled/

# Ajustar el upstream a localhost:5081 si Nginx se ejecuta en el host
sudo sed -i 's/ludeka-web:8080/127.0.0.1:5081/g' /etc/nginx/sites-available/ludeka.conf

# Verificar sintaxis y recargar
sudo nginx -t
sudo systemctl reload nginx
```

### 6.3 Obtener Certificado SSL Gratuito con Let's Encrypt
```bash
sudo certbot --nginx -d ludeka.es -d www.ludeka.es
```

> [!IMPORTANT]
> Blazor Server requiere comunicación continua mediante WebSockets (`_blazor`). La configuración provista en `deploy/nginx/default.conf` incluye las directivas `Upgrade $http_upgrade`, `Connection $connection_upgrade` y `proxy_buffering off` necesarias para evitar desconexiones de interfaz.

---

## 7. Automatización de Copias de Seguridad de SQLite

La base de datos SQLite opera en modo WAL (Write-Ahead Logging). El script `deploy/backup-sqlite.sh` genera respaldos en caliente consistentes sin bloquear las lecturas o escrituras de los usuarios.

### 7.1 Dar permisos de ejecución
```bash
chmod +x /opt/ludeka/deploy/backup-sqlite.sh
chmod +x /opt/ludeka/deploy/restore-sqlite.sh
```

### 7.2 Programar Backup Diario con Cron
Editar el crontab del sistema:
```bash
sudo crontab -e
```
Añadir la siguiente línea para ejecutar el respaldo todas las noches a las 04:00 AM:
```cron
0 4 * * * /opt/ludeka/deploy/backup-sqlite.sh >> /var/log/ludeka_backup.log 2>&1
```

El script conserva automáticamente los últimos 7 días de respaldos y purga copias más antiguas.

---

## 8. Actualización de Versiones (Zero-Downtime Rollout)

Para desplegar una nueva versión publicada en git:

```bash
cd /opt/ludeka

# 1. Obtener últimos cambios
git pull origin main

# 2. Reconstruir imagen y recrear contenedor sin pérdida de volumen
docker compose up -d --build

# 3. Comprobar salud tras el despliegue
curl -f http://localhost:5081/ready || echo "ALERTA: Despliegue fallido"
```

Los datos persistentes residen en el volumen Docker nombrado `ludeka_data`, por lo que nunca se borran al actualizar la imagen del contenedor.

---

## 9. Resolución de Incidencias Comunes

### A. La interfaz parpadea o se desconecta ("Attempting to reconnect...")
- **Causa:** El proxy inverso no está reenviando los WebSockets o tiene buffering activo.
- **Solución:** Revisa que `proxy_buffering off;` y las cabeceras `Upgrade` y `Connection` estén presentes en la configuración de Nginx.

### B. Error `503 Service Unavailable` en `/ready`
- **Causa:** La base de datos SQLite no puede abrirse o el directorio carece de permisos de escritura para el usuario `app` (UID 1654).
- **Solución:** Ejecuta `docker compose logs ludeka-web` para identificar la excepción y verifica los permisos del volumen con `docker exec -it ludeka-web ls -la /app/data`.
