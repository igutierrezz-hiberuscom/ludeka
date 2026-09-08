# Incremento 28: Generador y Publicador Directo de Posts para Instagram en Moderación

- **Identificador SDD:** `change-28-instagram-direct-publisher`
- **Estado:** ✅ **Archivado / Implementado y Verificado**
- **Puntos de la Especificación:** Automatización Omnicanal (INC-06), Moderación Editorial (INC-18), Meta Graph API v19.0 / Instagram Content Publishing API, Gobernanza y Auditoría (INC-20), Módulo 21 Especificación Viva.
- **Objetivo Principal:** Conectar la plataforma web de Ludeka directamente con la cuenta oficial de Instagram de la marca: permitir a los moderadores seleccionar cualquier sorteo o novedad validado y generar con un solo clic un borrador de publicación para Instagram. En el panel de moderación (`/admin/instagram`), el moderador puede inspeccionar una previsualización interactiva con la imagen de marca compuesta (1:1), editar el texto y hashtags sugeridos, y con un botón de aprobación final, disparar la publicación automática en la cuenta oficial de Instagram vía Meta Graph API.

---

## 1. Alcance Funcional y Técnico

1. **Flujo Editorial desde Moderación a Redes:**
   - En la bandeja de aprobación de sorteos (`/sorteos` o `/admin/sorteos`) y novedades (`/novedades`):
     - Botón de acción: `[ 📸 Crear Post de Instagram ]`.
     - Al pulsarlo, el sistema orquesta la creación de una entidad `InstagramPostDraft` asociada al elemento de origen.

2. **Compositor de Imagen de Marca Cuadrada (1:1 / 4:5):**
   - Reutilización y potenciación del motor de composición visual (desarrollado en INC-06 con SkiaSharp):
     - Montaje de la imagen con plantilla editorial: carátula del juego/sorteo, logotipo oficial de Ludeka en esquina superior, píldora distintiva (ej. "🎁 Sorteo Activo" / "📰 Novedad Editorial") y franja inferior con fechas límite o editorial responsable.
     - Posibilidad de alternar entre diseño claro u oscuro para el marco de la publicación.

3. **Zona de Trabajo de Instagram en Moderación (`/admin/instagram`):**
   - **Previsualizador Interactivo:** Mockup fidedigno de cómo lucirá el post en el feed de Instagram (imagen compuesta + copy completo).
   - **Generador Inteligente de Copy:** Texto estructurado automáticamente con emojis, mención a la cuenta organizadora, resumen del contenido, advertencia de fechas límite y lote de hashtags relevantes (`#juegosdemesa #boardgames #ludeka #sorteojuegos`).
   - **Editor en Vivo:** Campo de texto enriquecido para que el moderador personalice el mensaje, añada enlaces en bio o ajuste menciones antes de dar el visto bueno.

4. **Publicador Oficial vía Meta Graph API (`IInstagramPublisherClient`):**
   - Integración con Instagram Content Publishing API para cuentas profesionales/creadores:
     - Paso 1: Creación del contenedor de medios en Instagram (`POST /{ig-user-id}/media` con `image_url` pública de Ludeka y `caption`).
     - Paso 2: Publicación definitiva del contenedor (`POST /{ig-user-id}/media_publish`).
   - **Gestión de Respuestas y Trazabilidad:** Almacenamiento del identificador de Instagram (`InstagramMediaId`), enlace permanente (`Permalink`) al post publicado y marcado del sorteo/novedad con badge "Publicado en Instagram".
   - Control de excepciones: captura y reporte claro de posibles errores de Meta (token expirado, dimensiones de imagen no conformes o cuota excedida).

---

## 2. Criterios de Aceptación (Gherkin)

```gherkin
Escenario: Generación de borrador de Instagram desde un sorteo validado
  Dado un moderador que acaba de aprobar un sorteo externo de "Devir Iberia"
  Cuando pulsa el botón "[ 📸 Crear Post de Instagram ]"
  Entonces el sistema redirige a la zona de Instagram en moderación
  Y genera una previsualización con la imagen de marca compuesta
  Y sugiere automáticamente un texto con mención a "@deviriberia" y fecha límite de participación

Escenario: Edición y publicación exitosa en la cuenta de Instagram de Ludeka
  Dado un moderador revisando el borrador del post en "/admin/instagram"
  Cuando ajusta el copy añadiendo un hashtag personalizado y pulsa "[ 🚀 Validar y Publicar en Instagram ]"
  Entonces el cliente de Meta Graph API envía el medio y el texto a Instagram
  Y el post queda publicado en el feed oficial de Ludeka
  Y el estado del borrador pasa a "Published" con su URL permanente vinculada

Escenario: Registro en la auditoría del sistema
  Dado que un moderador publica un post de Instagram desde el panel
  Cuando la publicación se completa
  Entonces el registro de auditoría almacena la acción "InstagramPostPublished" con el ID del moderador, fecha y enlace resultante
```

---

## 3. Consideraciones Arquitectónicas y Dependencias

- **Seguridad y Credenciales:** Configuración de `MetaSettings:AccessToken`, `MetaSettings:InstagramAccountId` en variables seguras del entorno de producción.
- **Acceso a Imágenes:** La API de publicación de Instagram exige que la `image_url` sea públicamente accesible vía HTTPS por los servidores de Meta, requiriendo que la imagen generada se aloje en el servidor web público de Ludeka antes del dispatch.
