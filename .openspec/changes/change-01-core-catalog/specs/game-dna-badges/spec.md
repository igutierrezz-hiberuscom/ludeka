# Especificación: game-dna-badges

## Propósito
Definir los Value Objects de dominio y los badges de UI para Tipo de Confrontación, Estilo Lúdico y Modo Solitario, permitiendo una lectura visual compacta del ADN lúdico en 3 segundos tanto en tarjetas como en la ficha del juego.

## Requerimientos

### Requerimiento: Value Object de Tipo de Confrontación (`ConfrontationType`)
El modelo de dominio DEBE clasificar los juegos en paradigmas de confrontación claros para informar sobre la dinámica grupal y la tensión competitiva.

#### Escenario: Evaluar confrontación cooperativa
- DADO un juego que requiere colaboración total del equipo contra el juego (ej. *The Crew*, *Gloomhaven*)
- CUANDO se evalúa su tipo de confrontación
- ENTONCES DEBE representarse como `Cooperative`
- Y renderizarse con la etiqueta `"Cooperativo"` y el icono `"🤝"`.

#### Escenario: Evaluar confrontación competitiva
- DADO un juego donde los jugadores compiten individualmente por ganar (ej. *Catán*, *Wingspan*)
- CUANDO se evalúa su tipo de confrontación
- ENTONCES DEBE representarse como `Competitive`
- Y renderizarse con la etiqueta `"Competitivo"` y el icono `"⚔️"`.

#### Escenario: Evaluar roles ocultos o equipos
- DADO un juego donde los roles están ocultos o se compite por bandos (ej. *Secret Hitler*, *Código Secreto*)
- CUANDO se evalúa su tipo de confrontación
- ENTONCES DEBE representarse como `HiddenRolesOrTeams`
- Y renderizarse con la etiqueta `"Roles Ocultos / Equipos"` y el icono `"👥"`.

#### Escenario: Evaluar semi-cooperativo
- DADO un juego con metas compartidas de supervivencia pero con traidores o ganadores individuales (ej. *Nemesis*)
- CUANDO se evalúa su tipo de confrontación
- ENTONCES DEBE representarse como `SemiCooperative`
- Y renderizarse con la etiqueta `"Semi-cooperativo"` y el icono `"🗡️"`.

---

### Requerimiento: Value Object de Estilo Lúdico (`GameStyle`)
El modelo de dominio DEBE clasificar los juegos en arquetipos mecánicos y temáticos reconocibles por la comunidad.

#### Escenario: Clasificar estilo Eurogame
- DADO un juego enfocado en gestión de recursos, colocación de trabajadores y azar mínimo (ej. *Terraforming Mars*, *Agrícola*)
- CUANDO se evalúa su estilo
- ENTONCES DEBE asignarse `Eurogame`
- Y renderizarse con la etiqueta `"Eurogame"` y el icono `"⚙️"`.

#### Escenario: Clasificar estilo Ameritrash / Temático
- DADO un juego centrado en ambientación, combate y tensión narrativa (ej. *Zombicide*, *Las Mansiones de la Locura*)
- CUANDO se evalúa su estilo
- ENTONCES DEBE asignarse `Ameritrash`
- Y renderizarse con la etiqueta `"Ameritrash / Temático"` y el icono `"🎲"`.

#### Escenario: Clasificar estilo Party Game
- DADO un juego ligero pensado para grupos casuales, humor e interacción social (ej. *Dixit*, *Exploding Kittens*)
- CUANDO se evalúa su estilo
- ENTONCES DEBE asignarse `PartyGame`
- Y renderizarse con la etiqueta `"Party Game"` y el icono `"🎉"`.

#### Escenario: Clasificar estilo Filler / Abstracto
- DADO un juego de corta duración (<30 min) o abstracto sin temática densa (ej. *Azul*, *Hive*, *Cascadia*)
- CUANDO se evalúa su estilo
- ENTONCES DEBE asignarse `FillerAbstract`
- Y renderizarse con la etiqueta `"Filler / Abstracto"` y el icono `"🧩"`.

#### Escenario: Clasificar estilo Narrativo / Campaña
- DADO un juego guiado por historia, decisiones ramificadas o legado de partidas (ej. *Arkham Horror LCG*, *Pandemic Legacy*)
- CUANDO se evalúa su estilo
- ENTONCES DEBE asignarse `NarrativeCampaign`
- Y renderizarse con la etiqueta `"Narrativo / Campaña"` y el icono `"📖"`.

---

### Requerimiento: Indicador de Modo Solitario Oficial
El agregado `Game` DEBE distinguir de forma explícita si incluye reglas y componentes para juego en solitario oficial.

#### Escenario: Juego con modo solitario oficial
- DADO un juego cuyo rango oficial comienza en 1 jugador o incluye un modo automa oficial
- CUANDO se renderizan los badges de la ficha
- ENTONCES DEBE mostrarse el badge `"👤 Modo Solitario Oficial"`
- Y el filtro `Top Solitario` DEBE incluir el juego en sus resultados.