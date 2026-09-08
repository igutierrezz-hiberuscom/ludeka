# Incremento 12: Simulación y Mock de API BGG (30 Juegos Base + 10 Expansiones Reales)

- **Identificador SDD:** `change-12-mock-bgg-simulation`
- **Estado:** ✅ **Completado y Archivado** (266 tests en verde al 100%)
- **Objetivo Principal:** Proveer una suite completa de simulación offline para la API de BoardGameGeek (XMLAPI2) con 30 juegos base legendarios y 10 expansiones oficiales con datos reales y enriquecidos, permitiendo probar la búsqueda asistida, la importación de colecciones y el procesado de la cola sin depender de conectividad externa ni de tokens de BGG.

---

## 1. Alcance Funcional Implementado

1. **Dataset Realista de 40 Títulos del Hobby (`BggSimulationDataset`):**
   - **30+ Juegos Base:** Metadatos completos (BGG ID, título español y original, autor, editorial en España, año, descripciones atractivas, ADN lúdico, carátula de alta resolución, semáforo de escalabilidad detallado por comensal, fundas y enlaces de compra).
     *(Catan, Carcassonne, Wingspan, Terraforming Mars, 7 Wonders Duel, Ark Nova, Azul, Cascadia, Dune: Imperium, Everdell, Gloomhaven, Heat: Pedal to the Metal, Scythe, Spirit Island, Splendor, The Crew, Brass: Birmingham, Clank!, Pandemic, Root, Patchwork, Ticket to Ride, Concordia, Love Letter, Viticulture, Agrícola, Las Ruinas Perdidas de Arnak, Dixit, Código Secreto, Great Western Trail, Los Castillos de Borgoña)*.
   - **10 Expansiones Oficiales:** Conectadas a sus respectivos juegos base con badges de impacto, necesidad, aportes y recetas de mesa (*Preludio*, *Hellas & Elysium*, *Europa*, *Oceanía*, *Asia*, *Posadas & Catedrales*, *Constructores & Comerciantes*, *Pantheon*, *El Auge de Ix*, *Bellfaire*).
2. **Implementación de `SimulatedBggClient`:**
   - Implementa [`IBggClient`](file:///c:/repos/Ludeka/src/Ludeka.Application/Contracts/IBggClient.cs).
   - `SearchGamesAsync(query)`: Búsqueda flexible en memoria sobre los títulos (por título en español, título original y diseñador sin distinguir mayúsculas, minúsculas ni tildes).
   - `FetchGameByBggIdAsync(bggId)`: Reconstrucción instantánea de la entidad `Game` con todos sus Value Objects.
   - `FetchUserCollectionAsync(username)`: Retorno de colecciones lúdicas simuladas para nombres de usuario de prueba (`ludeka_demo`, `pareja_jugona`, `maraton_euro`, `default`) para verificar el flujo de importación en 1 clic.
3. **Conmutador de Configuración Transparente:**
   - Parámetro `Bgg:SimulateApi` en `appsettings.json`.
   - Fallback automático: Si `Bgg:ApiToken` no está configurado o `SimulateApi` es `true`, el sistema conmuta elegantemente a la simulación sin lanzar excepciones ni fallar.
4. **Actualización de `CatalogSeeder` y `seed-games.json`:**
   - Incorporar los 31 juegos base en `seed-games.json` y las 10 expansiones en el semillado de `CatalogSeeder` para que cualquier entorno nazca poblado con un catálogo vivo y vibrante.

---

## 2. Criterios de Aceptación (Gherkin)

```gherkin
Escenario: Búsqueda asistida en modo simulado
  Dado que el sistema tiene configurado el modo simulado de BGG
  Cuando el usuario busca el término "Dune" en el modal de búsqueda asistida
  Entonces el sistema devuelve los resultados correspondientes a "Dune: Imperium" con su BggId oficial
  Y no se realiza ninguna petición HTTP externa hacia boardgamegeek.com

Escenario: Importación de colección de usuario simulado
  Dado que el usuario introduce el usuario BGG de prueba "ludeka_demo"
  Cuando pulsa "Importar Colección"
  Entonces se reciben los juegos del catálogo de prueba
  Y los títulos existentes se asignan a su ludoteca local
  Y los títulos no catalogados se encolan en la tabla de importaciones pendientes

Escenario: Procesamiento de cola con Mock BGG
  Dado un juego pendiente en la cola de auto-catalogación con BggId correspondiente al dataset mock
  Cuando el administrador procesa el lote de la cola
  Entonces el juego se cataloga automáticamente con todos sus datos reales y semáforo de comensales
  Y las colecciones de usuario en espera son promovidas con éxito
```
