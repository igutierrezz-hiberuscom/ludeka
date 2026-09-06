# Especificación: offline-catalog-seeder

## Propósito
Proveer un seeder inicial con 25 a 50 juegos de mesa populares en español (ficheros JSON embebidos) y contexto SQLite EF Core 10, garantizando una experiencia de catálogo rica en modo offline sin depender de llamadas de red obligatorias.

## Requerimientos

### Requerimiento: Contexto de Base de Datos SQLite (`LudekaDbContext`)
El sistema DEBE proveer un contexto de persistencia SQLite que configure el agregado `Game` y mapee los Value Objects de escalabilidad y ADN lúdico mediante columnas JSON (`ToJson()`).

#### Escenario: Creación automática de la base de datos
- DADO que la aplicación arranca por primera vez
- CUANDO `LudekaDbContext` se inicializa
- ENTONCES DEBE crear la base de datos local SQLite `ludeka.db` si no existe
- Y aplicar automáticamente el esquema relacional.

---

### Requerimiento: Carga de Semillas JSON del Catálogo
El servicio `CatalogSeeder` DEBE poblar la base de datos con los 25 a 50 títulos top en español si la tabla `Games` está vacía.

#### Escenario: Carga inicial de catálogo curado
- DADO un fichero JSON local `seed-games.json` con juegos curados (*Catán*, *Wingspan*, *Los Castillos de Borgoña*, *Azul*, *7 Wonders Duel*, etc.)
- CUANDO la base de datos no contiene registros
- ENTONCES el seeder DEBE insertar los juegos con sus slugs, badges de ADN lúdico, semáforos de escalabilidad y datos de fundas
- Y dejar el catálogo listo para navegación inmediata.