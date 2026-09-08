# Especificación: card-sleeves-spec-stores

## Propósito
Dotar a la ficha de juego de la sección especializada "Protege tu juego", informando con precisión milimétrica sobre las medidas de cartas, recuento, cálculo automático de paquetes requeridos (en presentaciones de 50 y 100 unidades), guía de grosor de micras, enlaces contextuales de compra por tamaño a tiendas colaboradoras con filtrado por país, herramientas de edición para moderadores y extracción desde BGG XMLAPI2.

---

## Requerimientos

### Requerimiento 1: Visualización Editorial y Silueta Gráfica en Ficha
El sistema DEBE mostrar en la ficha de cada juego que contenga cartas una tarjeta especializada con la silueta gráfica proporcional de la carta, sus dimensiones exactas en mm, el recuento de cartas y la cantidad de paquetes recomendados.

#### Escenario: Juego con múltiples formatos de cartas (ej. 7 Wonders Duel)
- DADO que un usuario consulta la ficha pública de "7 Wonders Duel"
- CUANDO visualiza el componente "Protege tu juego"
- ENTONCES observa dos bloques de fundas diferenciados:
  - Formato "Mini European (44 x 68 mm)" con 73 cartas.
  - Formato "Large / Tarot (65 x 100 mm)" con 12 cartas.
- Y el sistema calcula que para las cartas Mini Euro se requieren 2 paquetes de 50 (o 1 de 100).
- Y para las cartas Tarot se requiere 1 paquete de 50.

#### Escenario: Juego sin cartas enfundables (ej. Azul o Hive)
- DADO un juego cuya colección de fundas está vacía
- CUANDO el usuario consulta la sección de fundas
- ENTONCES observa un mensaje limpio y tranquilizador: "¡Buenas noticias! Este juego no contiene cartas que requieran fundas o no tiene componentes enfundables."

---

### Requerimiento 2: Enlace de Compra Quirúrgico Contextual por Tienda y País
El sistema DEBE generar enlaces directos a la categoría o búsqueda del tamaño de funda exacto para tiendas colaboradoras (ej. Zacatrus), incorporando el tag de afiliado oficial de Ludeka y respetando el país activo del usuario.

#### Escenario: Navegación al tamaño de funda en Zacatrus para cartas Estándar
- DADO un juego con cartas de formato "Standard Card Game" (63.5 x 88 mm)
- Y el usuario tiene activo el país "España"
- CUANDO el usuario visualiza las opciones de compra de fundas
- ENTONCES el botón de compra para Zacatrus genera una URL parametrizada hacia el catálogo de fundas de 63.5x88 mm (ej. `https://zacatrus.es/accesorios/fundas.html?tamano=63.5x88&ref=ludeka` o búsqueda canónica)
- Y la URL contiene el parámetro de afiliación de Ludeka.

#### Escenario: Filtrado territorial por país sin envíos
- DADO un usuario con país seleccionado "México" o "Argentina"
- Y las ofertas de fundas configuradas en la tienda solo realizan envíos a "España"
- CUANDO el usuario consulta la guía de fundas
- ENTONCES el sistema no muestra botones de tiendas que no realizan envíos a su territorio
- Y muestra un aviso informativo de disponibilidad territorial.

---

### Requerimiento 3: Guía Didáctica de Grosor de Fundas (Micras)
El sistema DEBE ofrecer al usuario una explicación compacta sobre la diferencia entre fundas Standard (50-60 micras) y Premium (100 micras).

#### Escenario: Consulta de recomendación de grosor
- DADO un usuario en la tarjeta "Protege tu juego"
- CUANDO consulta la guía de grosor (micras)
- ENTONCES visualiza la comparativa didáctica:
  - "Standard (50-60 µm): Más económicas, barajeo ágil, ocupan menos espacio y caben en el inserto original de la caja."
  - "Premium (100 µm): Máxima protección y rigidez, gran durabilidad contra desgaste, pero doblan el grosor del mazo."

---

### Requerimiento 4: Gestión y Corrección Manual para Moderadores
Los moderadores del catálogo DEBEN poder añadir, modificar o eliminar formatos de fundas de cualquier juego desde el modal de edición editorial (`GameEditorModal`), con autocompletado por presets estándar y registro en auditoría.

#### Escenario: Añadir formato de fundas mediante preset estándar
- DADO un moderador con permiso `CanEditGames` editando la ficha de un juego
- CUANDO selecciona la pestaña "🛡️ Fundas"
- Y elige el preset "Euro Standard (59 x 92 mm)" e introduce "110" cartas
- Y pulsa "Guardar Cambios"
- ENTONCES la ficha pública se actualiza al instante con las nuevas fundas
- Y se genera una entrada en la bitácora de auditoría (`GameEditLog` y `AuditLog`) reflejando la adición.

---

### Requerimiento 5: Ingesta de Enlaces de Fundas desde BGG XMLAPI2
El parser de BGG DEBE reconocer elementos `<link type="boardgamecardsleeve" ...>` y extraer dimensiones y recuentos para enriquecer automáticamente títulos recién importados.

#### Escenario: Parseo de XML de BGG con enlace de fundas
- DADO un fragmento XML de BGG con `<link type="boardgamecardsleeve" id="104" value="56 x 87 mm" />`
- CUANDO se invoca el parser de fundas de BGG
- ENTONCES el sistema extrae `WidthMm = 56`, `HeightMm = 87` y mapea el formato a "Estándar USA".
