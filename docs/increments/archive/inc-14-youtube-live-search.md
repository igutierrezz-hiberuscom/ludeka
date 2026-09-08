# Incremento 14: Búsqueda Quirúrgica y Enlace de YouTube en Tiempo Real

- **Identificador SDD:** `change-14-youtube-live-search`
- **Estado:** ✅ **Completado y Archivado** (333 tests en verde al 100%)
- **Puntos del MVP cubiertos:** 5.2 (Pipeline de Ingesta Quirúrgica de YouTube).
- **Objetivo Principal:** Permitir al sistema y a los moderadores buscar de forma automatizada vídeos en YouTube en tiempo real con foco prioritario en canales oficiales de Editoriales, Creadores y Tiendas, incorporando el nuevo formato de "Cómo funciona (en 2 minutos)" junto con tutoriales de reglas completas y partidas en mesa.

---

## 1. Alcance Funcional Implementado

1. **Nuevo Formato Audiovisual `QuickOverview` ("⚡ Cómo funciona en 2 min"):**
   - Incorporado al enum de dominio `MediaType.QuickOverview` en `Ludeka.Core.Enums`.
   - Permite clasificar vídeos breves (≤ 180-210 s) orientados a explicar el flujo de turno y mecánicas esenciales para saber si te gusta el juego.
   - Pestaña dedicada `⚡ Cómo Funciona` en `MultimediaHub.razor`.
2. **Servicio de Búsqueda Quirúrgica (`IYouTubeSearchService`):**
   - Abstracción desacoplada en `Ludeka.Application.Contracts`.
   - Implementación `YouTubeSearchService` con cliente `HttpClient` para llamadas en tiempo real a la API oficial de Google (`https://www.googleapis.com/youtube/v3/search` y `videos`).
   - Consultas especializadas:
     - *Cómo funciona:* `"{Título} cómo funciona mecánicas en 2 minutos"` (≤ 3 min).
     - *Tutoriales:* `"{Título} cómo jugar tutorial español"` (8–25 min).
     - *Partidas:* `"{Título} partida completa español"` (con extracción de comensales).
   - Conversión de duración ISO 8601 (`PT#M#S` a segundos y formato `mm:ss`).
3. **Foco Central en Canales de Editoriales, Creadores y Tiendas (`IChannelFocusProvider`):**
   - Padrón oficial de canales hispanohablantes verificados:
     - *Editoriales:* Devir TV, Tranjis Games, Maldito Games, Asmodee Ibérica, 2Tomatoes, TCG Factory, Arrakis Games, etc.
     - *Creadores / Divulgadores:* Análisis Parálisis, Meepletopía, El Agujero de Hobbit, Mesa de Guerra, Pareja de Ases, Sentido Antihorario, Jugador Inicial, La Mazmorra de Pacheco, etc.
     - *Tiendas Especializadas:* Zacatrus!, Jugamos Otra, etc.
   - Ponderación de relevancia (+50 a +70 puntos) para situar los vídeos de estos canales en las primeras posiciones de sugerencias.
4. **Extractor de Jugadores para Partidas (`PlayerCountExtractor`):**
   - Analizador con expresiones regulares multivariante para detectar número de comensales (`"a 2"`, `"a 3"`, `"en solitario"`).
   - Fallback garantizado a la mejor escalabilidad del juego (`Game.Scalability`), satisfaciendo la invariante obligatoria de `MediaItem.PlayerCountBadge`.
5. **Modo Conectado y Salvaguardas sin Conexión:**
   - Soporte para clave oficial en `appsettings.json` (`YouTube:ApiKey`).
   - Dataset curado `YouTubeSimulationDataset` para los 40 títulos del hobby como salvaguardas en entornos CI sin internet.
6. **Flujo de Moderación en 1 Clic (`YouTubeSearchModal.razor` & `MediaModeration.razor`):**
   - Accesible desde `/moderacion-media`, `/moderacion/multimedia` y `/admin/moderacion-medios`.
   - Diálogo modal con autocompletado de catálogo, pestañas de resultados (`⚡ Cómo funciona`, `🎬 Tutoriales`, `🎲 Partidas`), previsualización de vídeo y botones `"📥 A Pendientes"` o `"✅ Aprobar Ya"`.
   - Prevención de duplicados por URL mediante `IMediaRepository.ExistsByUrlAsync`.

---

## 2. Criterios de Aceptación Verificados

```gherkin
Escenario: Búsqueda quirúrgica de vídeos "Cómo funciona" (2 min)
  Dado un juego en el catálogo como "Catan"
  Cuando el moderador solicita sugerencias de YouTube
  Entonces el sistema ofrece vídeos clasificados como QuickOverview con duración menor a 3 minutos
  Y permite incorporarlos directamente a la pestaña "Cómo funciona" del Hub Multimedia

Escenario: Foco prioritario en canales de editoriales, creadores y tiendas
  Dado un juego de Devir como "Carcassonne"
  Cuando se buscan vídeos en YouTube
  Entonces los vídeos subidos por canales oficiales registrados (como Devir TV, Zacatrus o Análisis Parálisis) aparecen antes que canales genéricos o no verificados

Escenario: Búsqueda de partidas completas con extracción de comensales
  Dado un vídeo con título "Partida a 2 jugadores a Wingspan"
  Cuando el moderador lo previsualiza e ingesta
  Entonces el sistema asigna automáticamente el badge "Partida a 2" sin errores de validación de dominio

Escenario: Consulta en vivo con YouTube Data API v3
  Dado que se ha configurado la clave en YouTube:ApiKey
  Cuando se busca un juego en el panel de moderación
  Entonces el sistema realiza una llamada HTTP en vivo a la API oficial de YouTube y devuelve los resultados en tiempo real
```
