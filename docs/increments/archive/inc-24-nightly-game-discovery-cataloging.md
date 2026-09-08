# Incremento 24: Detección Automática de Juegos en Novedades y Cola Nocturna Inteligente BGG/Gemini

- **Identificador SDD:** `change-24-nightly-game-discovery-cataloging`
- **Estado:** ✅ **Completado y Archivado (600 tests pasando al 100%)**
- **Puntos de la Especificación:** Extracción de Entidades con IA (INC-13), Ingesta Automática BGG (INC-05), Cola de Auto-Catalogación, Balanceo de Cuotas y Límites de API.
- **Objetivo Principal:** Conectar el pipeline de detección de novedades editoriales con el catálogo general de Ludeka: cuando el worker nocturno captura una noticia o post de novedad, un extractor inteligente identifica el título del juego mencionado. Si el título no existe en Ludeka, se consulta la API de BGG para comprobar su existencia y se añade a la cola de catalogación (`CatalogingQueue`). En la ejecución nocturna, el procesador ingesta los juegos de la cola, y si hay menos de 20 títulos pendientes, completa el cupo diario hasta 20 incorporando automáticamente los títulos con mejor ranking de BGG aún no catalogados, respetando escrupulosamente los límites de cuota de BGG y Gemini.

---

## 1. Alcance Funcional y Técnico

1. **Extractor de Juegos en Novedades (`INewsGameExtractor`):**
   - Análisis sintáctico y semántico del titular y cuerpo de cada novedad capturada (vía expresiones regulares y/o síntesis de bajo coste con Google Gemini Flash / heurística).
   - Identificación del juego o expansión objeto del anuncio editorial.
   - **Comprobación de Existencia en Ludeka:** Búsqueda difusa/exacta en el catálogo local (`IGameRepository`).
     - Si ya existe en Ludeka: vinculación directa (`CommunityNews.RelatedGameId = game.Id`).
     - Si no existe: consulta rápida a BGG Search API (`/xmlapi2/search?type=boardgame&query={title}`).
     - Si BGG devuelve coincidencia unívoca o de alta confianza: encolar automáticamente en `CatalogingQueue` con `BggId`, `ExtractedTitle` y origen `NewsDiscovery`.

2. **Orquestador Nocturno Inteligente de Ingesta (`NightlyCatalogingService`):**
   - Tarea programada en segundo plano (*BackgroundService* / Cron) configurada para ejecutarse en horas de baja demanda.
   - **Parámetro de Cupo Diario Configurable (`DailyCatalogingLimit`, default: 20):** Permite ajustar el límite fácilmente en `appsettings.json` o variables de entorno para ampliar la ingesta en el futuro sin tocar código.
   - **Paso 1 — Procesamiento de la Cola de Prioridad:**
     - Toma los juegos pendientes de `CatalogingQueue` (procedentes de importaciones de usuarios o de la detección de novedades).
     - Procesa hasta un máximo de `N` juegos pendientes (ej. si hay 8 juegos en cola, procesa los 8).
   - **Paso 2 — Relleno Automático de Catálogo con el Top de BGG:**
     - Si el número de juegos procesados de la cola es inferior a 20 (ej. se procesaron 8, restan 12):
     - El servicio consulta el Top de BGG para identificar los mejores juegos por ranking que todavía no forman parte del catálogo de Ludeka.
     - Descarga, cataloga y sintetiza con IA los 12 juegos restantes hasta completar exactamente el cupo de 20 diarios.

3. **Protección de APIs y Resiliencia de Tasa de Refresco:**
   - Control estricto de cadencia: pausas mínimas entre peticiones a BGG (2-3 segundos) para evitar bloqueos HTTP 429 de BGG XMLAPI2.
   - Gestión de cuotas de tokens para Google Gemini: procesamiento por lotes seguro con reintentos exponenciales.
   - Si se detecta un error de red o saturación temporal en BGG, el proceso se suspende limpiamente guardando el punto de corte para la noche siguiente.

4. **Panel de Supervisión en Administración (`/admin/cola-catalogacion`):**
   - Vista para moderadores con el histórico de ejecuciones nocturnas, juegos procesados, juegos procedentes de novedades y cupo restante.

---

## 2. Criterios de Aceptación (Gherkin)

```gherkin
Escenario: Detección de un juego nuevo a partir de una publicación editorial
  Dado que el batch nocturno ingesta una novedad titulada "Devir anuncia la edición en castellano de Apiary"
  Cuando el extractor analiza el texto y detecta el juego "Apiary"
  Y "Apiary" no existe en la base de datos de Ludeka pero sí en BGG (BggId 370132)
  Entonces se inserta un registro en "CatalogingQueue" con BggId 370132 y motivo "NewsDiscovery"

Escenario: Ejecución nocturna con cola incompleta y relleno con Top de BGG
  Dado que la cola de catalogación tiene 6 juegos pendientes
  Y el cupo diario configurado es de 20 juegos
  Cuando se inicia la ejecución nocturna del orquestador
  Entonces procesa e ingesta los 6 juegos de la cola
  Y a continuación identifica e ingesta los siguientes 14 mejores juegos del ranking de BGG no presentes en Ludeka
  Y al finalizar se han catalogado exactamente 20 juegos nuevos sin exceder el límite

Escenario: Respeto a las cuotas y pausas entre llamadas
  Dado el orquestador procesando 20 juegos consecutivamente
  Cuando invoca a BGG XMLAPI2 y a Gemini
  Entonces aplica una pausa mínima entre llamadas de 2.5 segundos
  Y no se registra ningún error de tasa de límite (HTTP 429) en el log de la aplicación
```

---

## 3. Consideraciones Arquitectónicas y Dependencias

- **Servicio:** `IHostedService` o job en segundo plano orquestado con tokens de cancelación.
- **Configuración:** `NightlyCatalogingOptions` inyectado mediante `IOptionsSnapshot<NightlyCatalogingOptions>` para permitir cambiar el cupo en caliente sin reiniciar el servidor.
