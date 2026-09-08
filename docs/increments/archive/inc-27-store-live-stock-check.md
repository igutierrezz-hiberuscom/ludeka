# Incremento 27: Monitorización y Verificación de Stock en Tiempo Real en Enlaces de Compra

- **Identificador SDD:** `change-27-store-live-stock-check`
- **Estado:** ✅ **Completado y Archivado** (656 tests en verde al 100%)
- **Módulo del Sistema:** [`20-verificacion-stock-tiempo-real-tiendas.md`](file:///c:/repos/Ludeka/docs/specs/sistema/20-verificacion-stock-tiempo-real-tiendas.md)
- **Puntos de la Especificación:** Enlaces a Tiendas y Afiliados (INC-11), Experiencia de Compra y Transparencia, Rendimiento y Caching Web, Streaming Rendering.
- **Objetivo Principal:** Resolver la frustración del usuario provocada por enlaces a tiendas con productos agotados o descatalogados. Se implementa un verificador de disponibilidad y stock en tiempo real para cada oferta comercial vinculada al juego (Zacatrus, Cuarto de Juegos, etc.) mediante una arquitectura no bloqueante y de latencia cero: carga asíncrona desacoplada del primer renderizado de la ficha, caché distribuida con TTL corto y degradación elegante si la tienda externa tarda en responder.

---

## 1. Alcance Funcional y Técnico

1. **Estado de Disponibilidad en el Dominio (`StockStatus`):**
   - Enum fuertemente tipado:
     - `InStock` (🟢 Disponible / En stock inmediato).
     - `LowStock` (🟡 Últimas unidades disponibles).
     - `OutOfStock` (🔴 Agotado / Temporalmente sin existencias).
     - `Unknown` (⚪ Comprobar en tienda / Sin información reciente).

2. **Arquitectura de Cero Impacto en el Rendimiento de la Ficha:**
   - **Renderizado Progresivo (Streaming Rendering):** La ficha del juego se entrega al navegador de forma instantánea (<100ms) sin esperar a las tiendas externas.
   - **Carga Diferida de Stock (`StoreOffersCard.razor`):** Un componente interactivo solicita de forma asíncrona el estado de stock una vez pintada la vista principal, mostrando un esqueleto de carga sutil (*shimmer*) que muta a la insignia de stock correspondiente.
   - **Límite Estricto de Tiempo (Timeout de 1.5s):** Si una tienda externa no responde en 1.500 ms, la petición se cancela limpiamente marcando el estado como `Unknown` con el texto "Verificar en web", impidiendo cualquier bloqueo del hilo de navegación.

3. **Capa de Caché Inteligente con TTL Dinámico (`IStoreStockService`):**
   - **Caché en Memoria:** Cada comprobación de stock se persiste en caché con un tiempo de expiración (TTL) configurable (30 minutos para éxitos, 5 minutos para errores/timeouts).
   - Si 50 usuarios acceden a la ficha de "Ark Nova" en el transcurso de media hora, solo se realiza una única consulta a los servidores de las tiendas, protegiendo tanto la infraestructura propia como la de las tiendas colaboradoras.

4. **Indicadores Visuales Claros para el Jugador:**
   - En la tabla de precios comparativos:
     - Badge verde nítido con texto "🟢 En Stock" cuando hay existencias.
     - Badge rojo "🔴 Agotado" con botón de compra atenuado ("Agotado / Ver tienda") para evitar compras frustradas.
     - Fecha relativa de la última verificación (ej. "Comprobado hace 12 min").
     - Botón de recarga manual interactivo ("🔄").

---

## 2. Criterios de Aceptación (Gherkin)

```gherkin
Escenario: Acceso instantáneo a la ficha sin bloqueo por comprobación de stock
  Dado que un usuario entra en la ficha de un juego con 3 ofertas de tiendas
  Cuando el servidor renderiza la página
  Entonces el tiempo de respuesta inicial no se ve demorado por las tiendas externas
  Y el componente de tiendas muestra inicialmente el precio base y consulta el stock en segundo plano

Escenario: Actualización en vivo del badge de stock disponible
  Dado un juego con stock confirmado en Zacatrus
  Cuando la consulta en segundo plano finaliza con éxito
  Entonces la tarjeta de Zacatrus se actualiza mostrando "🟢 En stock"

Escenario: Oferta agotada reflejada con transparencia
  Dado que una tienda colaboradora tiene el producto marcado como "Agotado"
  Cuando el sistema consulta su disponibilidad
  Entonces el usuario visualiza claramente el distintivo "🔴 Agotado"
  Y se evita transmitir una falsa expectativa de compra inmediata

Escenario: Uso de caché para visitas consecutivas
  Dado que el stock del juego "Catan" en la tienda A fue comprobado hace 5 minutos
  Cuando un segundo usuario entra en la ficha de "Catan"
  Entonces el estado se sirve inmediatamente desde la memoria caché
  Y no se emite ninguna petición HTTP hacia la web de la tienda A
```

---

## 3. Consideraciones Arquitectónicas y Dependencias

- **Servicio:** `IStoreStockService` y `IStoreStockClient` con clientes especializados (`HtmlSchemaStoreStockClient` y `SimulationStoreStockClient`).
- **Resiliencia:** Cancelación estricta con `CancellationTokenSource` y degradación elegante ante excepciones de red.
