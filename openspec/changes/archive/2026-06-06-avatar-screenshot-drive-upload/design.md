## Context

El avatar es una figura 2D renderizada con HTML/CSS puro: divs anidados con `transform: rotate()` aplicados vía JS interop desde `avatarAnim.js`. No usa `<canvas>` ni WebGL. Por lo tanto, para capturarlo como imagen necesitamos una librería que pueda "dibujar" el DOM a un canvas. `html2canvas` es la opción estándar para esto: recorre el DOM, calcula estilos computados, y los pinta en un `<canvas>`.

El proyecto ya tiene un servidor Python corriendo en `ws://localhost:8765` que recibe frames de cámara vía WebSocket y devuelve landmarks de MediaPipe. Para la funcionalidad de upload a Google Drive, necesitamos un endpoint HTTP (no WebSocket) porque el upload es una operación request-response puntual, no un stream continuo.

Google Drive requiere autenticación. Para un entorno de expo/kiosko, la opción más práctica es una **Service Account**: una cuenta de Google sin usuario humano, con acceso a una carpeta compartida de Drive. El servidor Python autentica con las credenciales de la Service Account (archivo JSON) y sube los archivos directamente.

## Goals / Non-Goals

**Goals:**
- Capturar el elemento `#avatar-wrapper` como imagen PNG vía html2canvas
- Superponer marca de agua translúcida con el logo de la ET12 sobre la imagen capturada
- Subir la imagen a una carpeta específica de Google Drive mediante un endpoint HTTP en el servidor Python
- Botón "Sacar Foto" en la UI con feedback visual (procesando, subiendo, éxito/error)
- Service Account de Google como método de autenticación (sin OAuth interactivo en el navegador)
- La imagen se nombra con timestamp para evitar colisiones

**Non-Goals:**
- Galería de fotos en el frontend (solo captura + upload)
- Edición o filtros de la imagen capturada
- Captura de la cámara web (solo el avatar, no el video feed)
- Upload desde múltiples clientes simultáneos (la expo tiene una sola instancia)
- Persistir las imágenes localmente en el servidor (van directo a Drive)

## Decisions

### Decision 1: html2canvas para captura del DOM

Alternativas consideradas:
- `dom-to-image` / `dom-to-image-more`: menos mantenido, problemas con transforms CSS 3D
- `SVG foreignObject` + serialización: complejo, no soporta bien ciertos estilos
- Screenshot nativo con `getDisplayMedia`: requiere permiso del usuario, captura toda la pantalla

**Se elige html2canvas** porque:
- Soporta `transform: rotate()`, `perspective`, y `rotateY`/`rotateZ` que el avatar usa extensivamente
- Biblioteca madura (30k+ stars en GitHub)
- API simple: `html2canvas(element).then(canvas => canvas.toBlob(...))`
- Se carga vía CDN (unpkg) en `index.html`, sin bundle de npm

### Decision 2: Endpoint HTTP en el servidor Python existente

Alternativas:
- Ejecutar un segundo servidor HTTP en otro puerto
- Usar el mismo WebSocket para enviar la imagen (mensaje binario frame único)

**Se elige agregar un endpoint HTTP al mismo proceso** en `server.py` porque:
- `aiohttp` ya está listado en la arquitectura (aunque no instalado actualmente, es liviano)
- La operación es request-response simple, WebSocket sería overkill
- Se puede correr en el mismo event loop de asyncio sin conflicto con el WebSocket server
- El endpoint HTTP puede correr en `http://localhost:8766` para no interferir con el WebSocket en `:8765`

Alternativa más simple: usar el módulo `http.server` de la stdlib de Python. No requiere dependencia extra. Suficiente para un solo endpoint POST.

**Re-evaluación**: Usar `aiohttp` en el mismo event loop que `websockets` es complejo (ambos son servers que compiten por el loop). Mejor enfoque:

**Decisión final**: El endpoint HTTP se implementa con `aiohttp` corriendo en un **hilo separado** (threading) sobre otro puerto (8766), o alternativamente, se extiende el WebSocket server para manejar un mensaje tipo `screenshot` con la imagen en base64. **Se elige extender el WebSocket** porque:
- No requiere nuevo puerto ni dependencias extra
- El WebSocket ya está conectado, es natural enviar la imagen por el mismo canal
- El mensaje de screenshot es un evento puntual, no interfiere con el flujo de frames
- Cero cambios en la infraestructura de red

El flujo sería:
1. Frontend captura avatar con html2canvas → PNG blob
2. Convierte blob a base64
3. Envía mensaje JSON por WebSocket: `{"type": "screenshot", "image": "<base64>", "filename": "avatar-20260605-143022.png"}`
4. Servidor recibe el mensaje, decodifica base64, sube a Google Drive
5. Servidor responde con `{"type": "screenshot_result", "success": true, "url": "..."}`

### Decision 3: Google Drive API con Service Account

Autenticación con Service Account de Google:
- Archivo `server/credentials.json` con la key de la Service Account (agregado a `.gitignore`)
- La Service Account tiene acceso de escritura a una carpeta específica de Drive
- El ID de la carpeta destino se configura en una variable de entorno `DRIVE_FOLDER_ID`
- Las imágenes se suben con MIME type `image/png`

Librerías necesarias (Python):
- `google-api-python-client` — cliente de Google Drive API v3
- `google-auth` — autenticación con Service Account

### Decision 4: UI — Botón en CameraPanel

El botón "Sacar Foto" se coloca en `CameraPanel.razor` (ya tiene los controles de tracking). El flujo visual:
1. Botón "📸 Sacar Foto" (habilitado solo si el avatar está siendo trackeado o en modo demo)
2. Al clickear → overlay "Procesando..." con spinner
3. Al recibir confirmación del servidor → overlay "¡Foto subida!" por 2 segundos
4. En caso de error → toast "Error al subir la foto"

El elemento capture target será `#avatar-wrapper`, que ya existe como contenedor en `AvatarRenderer.razor`. Se le agrega un `id` explícito si no lo tiene.

### Decision 5: Naming de archivos

Formato: `avatar-YYYYMMDD-HHMMSS.png` usando UTC. Esto evita colisiones y permite orden cronológico en Drive.

### Decision 6: Marca de agua con logo ET12

Una vez que html2canvas genera el canvas con el avatar, se dibuja encima el logo de la ET12 (`imgs/et12.svg`) usando la API de Canvas 2D:

1. Se carga el SVG como `Image` desde la ruta local
2. Se dibuja sobre el canvas con `ctx.globalAlpha = 0.15` (15% de opacidad, translúcido)
3. Se posiciona centrado y escalado para cubrir ~60% del ancho del canvas, sobre la zona del personaje
4. El resultado final se exporta como PNG (el alpha queda aplicado en la imagen)

Esto se hace del lado del frontend (JavaScript), después de html2canvas y antes de convertir a base64 para el upload. No requiere modificar el servidor.

Posición: centrado horizontalmente, verticalmente en el tercio superior (zona del torso/cabeza del avatar). Escala: `min(canvas.width * 0.6, 400)` px de ancho, manteniendo aspect ratio del SVG.

## Risks / Trade-offs

- **[Riesgo bajo] html2canvas y CSS transforms**: `perspective` y `rotateY`/`rotateZ` pueden no renderizarse exactamente igual que en pantalla. Se testeará con la configuración actual del avatar. Alternativa: aplanar transforms a 2D antes de capturar (complejo, solo si falla).
- **[Riesgo bajo] Tamaño de imagen**: PNG del avatar a buena resolución puede pesar 200-500KB. La conversión a base64 agrega ~33%. El mensaje WebSocket puede ser grande pero manejable para una sola imagen. Si supera el límite del WebSocket, se puede enviar en chunks.
- **[Riesgo medio] Credenciales de Google Drive**: La Service Account requiere configuración manual (crear proyecto en Google Cloud, habilitar Drive API, generar key, compartir carpeta). Se documentará en `server/README.md`.
- **[Riesgo bajo] Dependencia de red**: El upload a Google Drive requiere conectividad a internet. Si falla, se muestra error y la imagen se pierde (no hay cola de reintentos). Para la expo esto es aceptable porque la conexión debería ser estable.
- **[Riesgo bajo] html2canvas CDN**: Si el CDN de unpkg está caído, la funcionalidad no funciona. Se mitiga ofreciendo un fallback local (descargar el .js al proyecto).
