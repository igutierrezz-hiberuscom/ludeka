# Especificación: media-ingestion-curated

## Propósito
Definir las reglas y contratos de la estrategia de ingesta acotada de contenidos de YouTube e Instagram (cold start) y la provisión de datos curados offline-first para los juegos del catálogo base, respetando los límites de carga cognitiva y cuotas externas.

## Requerimientos

### Requerimiento: Regla de Corte en Cold Start (Máximo 2 Contenidos por Categoría)
En la ingesta inicial y semillado del catálogo base, el sistema DEBE limitar la cantidad de vídeos principales a un máximo de 2 tutoriales y 2 partidas destacadas por juego.

#### Escenario: Consulta de ficha multimedia de un juego con catálogo semillado
- DADO un juego base con múltiples recursos registrados
- CUANDO se invoca el servicio `GetGameMediaAsync(gameId)`
- ENTONCES el resultado devuelto DEBE entregar colecciones segregadas (`Tutorials`, `Playthroughs`, `InstagramPosts`, `ShortReels`) que contengan exclusivamente elementos en estado `Approved` y que no estén marcados como rotos (`IsBroken == false`).

---

### Requerimiento: Catálogo Curado de Canales Hispanohablantes
El semillado inicial DEBE incluir contenidos reales y representativos de la divulgación lúdica en español (ej. *Análisis-Parálisis*, *Zacatrus TV*, *El Troquel*, *Meepletopia*, *Rincón Legacy*, *Juegos de Mesa 221B*, etc.) para los 15 títulos del catálogo base.

#### Escenario: Semillado inicial con enriquecimiento multimedia
- DADO el arranque inicial del sistema con una base de datos recién creada
- CUANDO se ejecuta el proceso de inicialización `CatalogSeeder`
- ENTONCES la base de datos DEBE contener elementos multimedia aprobados vinculados a títulos icónicos (como *Catan*, *Terraforming Mars*, *Wingspan*, *Ark Nova*, etc.), además de un conjunto de elementos en estado `PendingApproval` y elementos huérfanos (`GameId == null`) para posibilitar la prueba inmediata del panel de moderación.
