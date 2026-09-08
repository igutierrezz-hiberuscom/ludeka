# Especificación: store-live-stock-check (Incremento 27)

## Propósito
Dotar a las ofertas comerciales de compra en Ludeka (`StoreOffersCard`) de monitorización y verificación de disponibilidad y stock en tiempo real, garantizando latencia cero en el primer renderizado de la ficha, resiliencia extrema mediante límite de tiempo estricto (timeout de 1.5s), degradación transparente, caché L1 con TTL dinámico, diferenciación clara de estados (`InStock`, `LowStock`, `OutOfStock`, `Unknown`) y prevención de compras frustradas mediante atenuación visual de productos agotados.

---

## Requerimientos

### Requerimiento 1: Latencia Cero y Carga Diferida Asíncrona
El sistema DEBE entregar la ficha del juego al navegador de forma instantánea (<100ms) sin esperar la respuesta de tiendas externas, delegando la consulta de disponibilidad en una tarea asíncrona no bloqueante tras el primer renderizado.

#### Escenario: Acceso a la ficha de juego con múltiples ofertas
- DADO que un usuario accede a la ficha pública de un juego con ofertas vinculadas (ej. Zacatrus, Cuarto de Juegos)
- CUANDO el servidor procesa y renderiza inicialmente el componente `StoreOffersCard`
- ENTONCES los datos base de la oferta (tienda, precio orientativo, enlace) se muestran de inmediato
- Y se inicia una consulta de stock en segundo plano sin congelar la interacción ni demorar el Time to First Byte (TTFB).

---

### Requerimiento 2: Taxonomía y Modelo de Disponibilidad (`StockStatus`)
El sistema DEBE clasificar la disponibilidad en cuatro estados semánticos claros:
1. `InStock`: El producto está disponible con existencias confirmadas.
2. `LowStock`: Últimas unidades disponibles en el inventario de la tienda.
3. `OutOfStock`: Producto sin existencias o temporalmente agotado.
4. `Unknown`: La tienda no informa disponibilidad clara o no ha respondido dentro del límite de tiempo.

#### Escenario: Detección de producto en stock con unidades conocidas
- DADO que la tienda externa devuelve disponibilidad confirmada y un conteo de 3 unidades
- CUANDO finaliza la consulta de stock
- ENTONCES el estado asignado es `StockStatus.InStock`
- Y la información registra `AvailableQuantity = 3` y la marca de tiempo de la comprobación.

#### Escenario: Detección de últimas unidades
- DADO que la tienda externa reporta estado de disponibilidad limitada o baja
- CUANDO finaliza la consulta de stock
- ENTONCES el estado asignado es `StockStatus.LowStock`.

#### Escenario: Detección de producto agotado
- DADO que la tienda externa reporta que el artículo no tiene existencias o está descatalogado
- CUANDO finaliza la consulta de stock
- ENTONCES el estado asignado es `StockStatus.OutOfStock`.

---

### Requerimiento 3: Límite Estricto de Tiempo (Timeout 1.5s) y Degradación Resiliente
Cada consulta de verificación de stock a una tienda externa DEBE estar acotada a un tiempo límite máximo de 1.500 ms. Si la tienda no responde en dicho intervalo o devuelve errores de red/HTTP (404, 500), el sistema DEBE degradar elegantemente a `StockStatus.Unknown` sin propagar excepciones ni romper la interfaz.

#### Escenario: Servidor de tienda externa lento o no responde
- DADO que una tienda colaboradora sufre una sobrecarga y no responde en 1.500 ms
- CUANDO se ejecuta la verificación de stock
- ENTONCES la petición se cancela limpiamente
- Y el estado devuelto es `StockStatus.Unknown` con nota descriptiva "Tiempo de espera agotado"
- Y la interfaz muestra la indicación "⚪ Verificar en web" manteniendo el botón de compra habilitado.

#### Escenario: Error HTTP de red o 404
- DADO que la URL de compra de una tienda devuelve error 404 Not Found o 500 Internal Server Error
- CUANDO el cliente de stock procesa la respuesta
- ENTONCES no se produce ninguna excepción no controlada
- Y el estado se resuelve como `StockStatus.Unknown`.

---

### Requerimiento 4: Capa de Caché L1 en Memoria con TTL Dinámico
El servicio de stock DEBE utilizar una caché en memoria (`IMemoryCache`) para almacenar el resultado de cada verificación, evitando peticiones duplicadas y protegiendo a las tiendas colaboradoras.

#### Escenario: Visitas recurrentes a la misma ficha
- DADO que el stock de una oferta fue verificado con éxito hace 10 minutos
- Y el TTL configurado para respuestas válidas es de 30 minutos
- CUANDO un nuevo usuario entra en la ficha del juego
- ENTONCES la información de stock se sirve inmediatamente desde la memoria caché
- Y no se realiza ninguna llamada HTTP hacia los servidores de la tienda.

#### Escenario: Expiración diferenciada para errores
- DADO que una consulta de stock falló por timeout o error de red y se cacheó con TTL de error (5 minutos)
- CUANDO transcurren 6 minutos y se solicita de nuevo el stock
- ENTONCES el sistema reintenta la consulta contra la tienda por si el servicio se ha recuperado.

---

### Requerimiento 5: Adaptadores de Tienda y Parser de Microdatos Schema.org / OpenGraph
El sistema DEBE disponer de una arquitectura modular de clientes de stock (`IStoreStockClient`):
1. `HtmlSchemaStoreStockClient`: Capaz de inspeccionar páginas HTML buscando metadatos estructurados estándar (`itemprop="availability"`, JSON-LD `http://schema.org/InStock` o `OutOfStock`, `og:availability`, etc.).
2. `SimulationStoreStockClient`: Provee datos deterministas y reproducibles para entornos de test y desarrollo offline.

#### Escenario: Parseo de etiqueta Schema.org ItemAvailability
- DADO un documento HTML de tienda con `<link itemprop="availability" href="https://schema.org/InStock" />`
- CUANDO el cliente `HtmlSchemaStoreStockClient` analiza el contenido
- ENTONCES determina con precisión que el estado es `StockStatus.InStock`.

#### Escenario: Parseo de JSON-LD con ItemAvailability OutOfStock
- DADO un documento HTML con bloque script `application/ld+json` que contiene `"availability": "http://schema.org/OutOfStock"`
- CUANDO el cliente analiza el contenido
- ENTONCES determina que el estado es `StockStatus.OutOfStock`.

---

### Requerimiento 6: Interfaz Visual y Anti-Frustración en `StoreOffersCard`
El componente Blazor DEBE presentar claramente el estado de stock sin ambigüedades:
1. Distintivo verde nítido para `InStock` ("🟢 En stock" o "🟢 En stock - X uds").
2. Distintivo ámbar para `LowStock` ("🟡 Últimas unidades").
3. Distintivo rojo para `OutOfStock` ("🔴 Agotado").
4. Si la oferta está agotada (`OutOfStock`), el botón de compra DEBE cambiar su estilo visual (atenuado o con borde sutil) y mostrar el texto "Agotado en tienda / Ver disponibilidad" para evitar falsas expectativas de compra.
5. Se mostrará el tiempo relativo transcurrido desde la comprobación ("Comprobado hace X min" o "En vivo").
6. Botón de refresco manual ("🔄 Actualizar stock") que invalide la caché de esa oferta y vuelva a consultar.

#### Escenario: Interacción con producto agotado
- DADO un juego cuya oferta en una tienda está confirmada como `OutOfStock`
- CUANDO el usuario revisa la tarjeta de tiendas
- ENTONCES visualiza el badge "🔴 Agotado"
- Y el botón principal advierte con claridad "Agotado / Ver en tienda" con apariencia atenuada.
