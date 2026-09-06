# Especificación: bgg-xmlapi-client

## Propósito
Implementar un cliente HTTP resiliente y parser LINQ-to-XML para consumir la API oficial BGG XMLAPI2 (`https://boardgamegeek.com/xmlapi2/`), extrayendo nombres alternativos en español, conteos de votos de escalabilidad, edad recomendada y enlaces de carátulas sin almacenamiento en disco propio.

## Requerimientos

### Requerimiento: Consulta Resiliente con Rate Limiting
El cliente `BggXmlApiClient` DEBE respetar un límite de llamadas seguro (máximo 2 peticiones por segundo) con política de reintentos exponenciales ante respuestas HTTP 202 (BGG queue) o 429.

#### Escenario: Manejo de respuesta HTTP 202 (En proceso en BGG)
- DADO que BGG responde con código HTTP 202 indicando que los datos se están compilando
- CUANDO el cliente procesa la petición
- ENTONCES DEBE esperar de forma exponencial (1s, 2s, 4s) antes de reintentar hasta un máximo de 3 veces.

---

### Requerimiento: Extracción Quirúrgica de Metadatos
El parser LINQ-to-XML DEBE extraer el nombre comercial en español prioritario si existe, las carátulas remotas oficiales, el rango de edad y la encuesta de jugadores recomendados.

#### Escenario: Parseo de encuesta de comensales recomendados
- DADO el XML devuelto por `/xmlapi2/thing?id=13&stats=1`
- CUANDO se parsea el elemento `<poll name="suggested_numplayers">`
- ENTONCES el parser DEBE tabular los votos de `Best`, `Recommended` y `Not Recommended` para cada número de jugadores
- Y asignar los estados correspondientes del semáforo.