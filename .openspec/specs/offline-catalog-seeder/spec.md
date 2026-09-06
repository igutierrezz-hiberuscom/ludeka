# Especificación: offline-catalog-seeder

## Propósito
Proveer un seeder inicial con 25 a 50 juegos de mesa populares en español (ficheros JSON embebidos) y contexto SQLite EF Core 10, garantizando una experiencia de catálogo rica en modo offline sin depender de llamadas de red obligatorias.

## Requerimientos

### Requerimiento: Contexto de Base de Datos SQLite (`LudecaDbContext`)
La capa de infraestructura DEBE configurar EF Core 10 con SQLite y mapeo de Value Objects JSON (`ToJson()`) para almacenar el catálogo de juegos.

#### Escenario: Inicialización de base de datos y migraciones
- DADO que la aplicación arranca por primera vez
- CUANDO `LudecaDbContext` se inicializa
- ENTONCES DEBE crear la base de datos local SQLite `ludeca.db` si no existe
- Y aplicar automáticamente el esquema relacional.

---

### Requerimiento: Carga de Semillas JSON del Catálogo
El servicio `CatalogSeeder` DEBE poblar la base de datos con los 25 a 50 títulos top en español si la tabla `Games` está vacía.

#### Escenario: Carga inicial de catálogo curado
- DADO un fichero JSON local `seed-games.json` con juegos curados (*Catán*, *Wingspan*, *Los Castillos de Borgoña*, *Azul*, *7 Wonders Duel*, etc.)
- CUANDO la base de datos no contiene registros
- ENTONCES el seeder DEBE insertar los juegos con sus slugs, badges de ADN lúdico, semáforos de escalabilidad y datos de fundas
- Y dejar el catálogo listo para navegación inmediata.