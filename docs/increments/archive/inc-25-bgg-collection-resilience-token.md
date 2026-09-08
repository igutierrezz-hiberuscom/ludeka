# Incremento 25: Auditoría de Resiliencia, Rate Limiting y Estrategia de Token en el Importador de Ludotecas BGG

- **Identificador SDD:** `change-25-bgg-collection-resilience-token`
- **Estado:** ✅ **Completado y Archivado (609 tests pasando al 100%)**
- **Puntos de la Especificación:** Importador BGG en 1 Clic (INC-05), Integración BGG XMLAPI2, Políticas de Reintento y Resiliencia (Polly / Backoff), Soporte de Credenciales y Tokens API.
- **Objetivo Principal:** Auditar minuciosamente el cliente de importación de colecciones de usuarios de BoardGameGeek (`/xmlapi2/collection`), resolver los problemas habituales derivados del comportamiento asíncrono de BGG (respuestas `HTTP 202 Accepted` cuando BGG encola la petición en sus servidores) y el bloqueo por rate limiting (errores `429` / `503`), e incorporar soporte nativo para claves de API / tokens Bearer si BGG activa o exige autenticación oficial, ofreciendo al usuario una barra de progreso informativa y desacoplada de la interfaz.

---

## 1. Alcance Funcional y Técnico

1. **Auditoría del Protocolo BGG XMLAPI2 para Colecciones:**
   - Análisis del comportamiento de `/xmlapi2/collection?username={user}&own=1`:
     - BGG suele devolver `202 Accepted` ("Your request for this collection has been accepted and will be processed...") si la colección no está en su memoria caché de servidor.
     - Si la aplicación interpreta un 202 como error o vacío, la importación fracasa inmediatamente.
   - **Mecanismo de Polling Asíncrono con Backoff Exponencial:**
     - Implementación de un ciclo de espera controlado (esperar 3s, 5s, 8s...) con límite de tiempo máximo (timeout amigable de 45 segundos) hasta que BGG devuelva `200 OK` con el XML completo.

2. **Gestión de Identificación y Cabeceras (Buenas Prácticas BGG):**
   - Configuración de cabeceras obligatorias requeridas por las directrices de BGG: `User-Agent` descriptivo personalizado con nombre de la app y contacto para evitar bloqueos por IP a nivel de Cloudflare.

3. **Arquitectura Preparada para Tokens / Autenticación BGG:**
   - Soporte para credenciales en `appsettings.json` y variables de entorno:
     - `BggApiSettings:ApiKey` / `BggApiSettings:BearerToken` (opcional o requerido según la evolución de BGG).
   - Inyección de `DelegatingHandler` en el `HttpClient` de BGG para adjuntar automáticamente la cabecera `Authorization: Bearer <token>` o parámetro `&token=` si está configurado.

4. **Feedback Visual en Tiempo Real para el Usuario (`ImportModal.razor`):**
   - En lugar de una pantalla congelada, la interfaz muestra estados comprensibles:
     - *"Solicitando colección a BoardGameGeek..."*
     - *"BGG está preparando tu colección en sus servidores (intento 2 de 5)..."*
     - *"Procesando 145 juegos encontrados..."*
   - Si BGG no responde tras el tiempo máximo, se ofrece continuar en segundo plano y notificar al usuario cuando esté lista.

---

## 2. Criterios de Aceptación (Gherkin)

```gherkin
Escenario: Manejo transparente de respuesta HTTP 202 de BGG
  Dado un usuario que solicita importar su colección "jugador_madrid"
  Cuando BGG XMLAPI2 responde inicialmente con código 202 Accepted
  Entonces el servicio no falla y aplica una estrategia de reintento inteligente
  Y el usuario visualiza el mensaje "BGG está preparando tu colección..."
  Y en el siguiente reintento tras 4 segundos recibe el XML con código 200 OK completando la importación

Escenario: Importación con clave de API configurada en el servidor
  Dado que el administrador ha configurado una clave "BggApiKey" en la configuración
  Cuando el cliente HttpClient efectúa una petición a "/xmlapi2/collection"
  Entonces la petición viaja con la cabecera de autenticación requerida y el User-Agent oficial

Escenario: Control de límite de tasa (Rate Limit 429)
  Dado que BGG responde con código 429 Too Many Requests
  Entonces el sistema captura el código sin romper la aplicación
  Y espera el tiempo especificado en la cabecera Retry-After antes de reintentar
  Y muestra un aviso de calma al usuario informando de la saturación momentánea de BGG
```

---

## 3. Consideraciones Arquitectónicas y Dependencias

- **Resiliencia:** Uso de políticas Polly en el registro del cliente `HttpClient` (`services.AddHttpClient<IBggApiClient, BggApiClient>()`).
- **Desacoplo:** Las importaciones masivas de colecciones grandes (>500 juegos) pueden ejecutarse en segundo plano guardando el progreso en la sesión del usuario.
