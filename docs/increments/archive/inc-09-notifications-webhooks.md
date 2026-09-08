# Incremento 9: Notificaciones y Webhooks de Comunidad (Discord y Telegram)

- **Identificador SDD:** `change-09-notifications-webhooks`
- **Objetivo Principal:** Difusión multicanal automatizada para dinamizar la comunidad avisando de eventos clave en Discord y Telegram sin intervención manual ni latencia en peticiones HTTP.
- **Estado:** ✅ **Completado y Archivado** (189 tests en verde al 100%).

---

## 1. Alcance Funcional y Técnico Entregado

1. **Motor de Webhooks Multicanal (`ICommunityNotificationService`):**
   - Integración con Discord Webhooks y Telegram Bot API con plantillas enriquecidas (Embeds con color de marca, enlaces directos y detalles del juego).
2. **Cola de Despacho en Segundo Plano (Outbox Pattern / `Channel<T>`):**
   - Desacoplamiento asíncrono mediante `BackgroundService` (`CommunityNotificationDispatcherHostedService`) para cero impacto en la latencia de usuario.
3. **Eventos Automatizados:**
   - ⚠️ Alerta de sorteo a punto de expirar (24 horas antes del cierre).
   - 🛍️ Boletín de lanzamientos de tiendas de los viernes.
   - 🛡️ Nuevo veredicto fundador publicado.
   - 💡 Duda de reglas con respuesta aceptada.
4. **Configuración Segura y Modo Dry-Run:**
   - Opciones en `appsettings.json` (`CommunityNotifications`) con soporte para modo simulado/dry-run en desarrollo y pruebas.

---

## 2. Artefactos Clave

- **Dominio:** [`CommunityNotificationLog.cs`](file:///c:/repos/Ludeka/src/Ludeka.Core/Entities/CommunityNotificationLog.cs).
- **Aplicación:** [`ICommunityNotificationService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/ICommunityNotificationService.cs), [`CommunityNotificationService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Application/Features/Community/CommunityNotificationService.cs).
- **Infraestructura:** [`DiscordWebhookClient.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Notifications/DiscordWebhookClient.cs), [`TelegramBotClient.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Notifications/TelegramBotClient.cs), [`InMemoryCommunityNotificationQueue.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Notifications/InMemoryCommunityNotificationQueue.cs), [`CommunityNotificationDispatcherHostedService.cs`](file:///c:/repos/Ludeka/src/Ludeka.Infrastructure/Notifications/CommunityNotificationDispatcherHostedService.cs).

---

## 3. Verificación

- Pruebas unitarias de encolado, generación de payloads para Discord/Telegram y registro de logs en [`tests/Ludeka.UnitTests`](file:///c:/repos/Ludeka/tests/Ludeka.UnitTests).
