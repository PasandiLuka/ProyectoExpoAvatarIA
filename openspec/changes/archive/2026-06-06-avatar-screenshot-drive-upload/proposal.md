## Why

Actualmente no existe ninguna forma de capturar el estado visual del avatar (postura, personalización, items equipados) como imagen estática. Los usuarios que interactúan con el avatar en la expo necesitan poder tomarse una "foto" con su avatar personalizado y que esa imagen se almacene automáticamente en una carpeta de Google Drive para su posterior acceso, galería pública, o procesamiento (impresión, sharing, etc.).

## What Changes

- **Captura de avatar vía html2canvas**: Se agrega la librería `html2canvas` para renderizar el DOM del avatar (divs CSS) a un canvas y exportarlo como PNG blob.
- **Marca de agua con logo ET12**: Antes de exportar, se superpone el logo de la ET12 (`imgs/et12.svg`) con transparencia sobre la imagen capturada. La marca de agua es translúcida y se posiciona sobre el personaje.
- **Botón "Sacar Foto"**: Se agrega un botón en la UI que dispara la captura del elemento `#avatar-wrapper`.
- **Endpoint de upload en el servidor Python**: Nuevo endpoint HTTP `POST /upload-screenshot` que recibe la imagen PNG en base64 y la sube a Google Drive usando la API de Google Drive con autenticación por Service Account.
- **Feedback visual**: Indicador de progreso (capturando... subiendo... listo) y vista previa de la imagen capturada antes del upload.

## Capabilities

### New Capabilities

- `avatar-screenshot`: Sistema de captura del avatar como imagen PNG usando html2canvas en el frontend Blazor WASM.
- `drive-upload`: Endpoint en el servidor Python para recibir imágenes y subirlas a Google Drive vía Service Account.

### Modified Capabilities

Ninguno. Es funcionalidad nueva que no modifica el pipeline de tracking ni la personalización existente.

## Impact

- `AvatarExpo/wwwroot/index.html` — agregar `<script>` de html2canvas
- `AvatarExpo/Components/AvatarRenderer.razor` — wrapper `#avatar-wrapper` con `@ref` para que html2canvas capture el DOM; nuevo método JS interop `captureAvatar()`
- `AvatarExpo/Components/CameraPanel.razor` — botón "Sacar Foto" con indicador de progreso
- `AvatarExpo/wwwroot/js/camera.js` — nueva función `captureAvatarToBlob()` que usa html2canvas + superpone marca de agua ET12
- `AvatarExpo/wwwroot/imgs/et12.svg` — ya existe, se usa como marca de agua
- `server/server.py` — nuevo endpoint HTTP POST `/upload-screenshot`
- `server/google_drive.py` — nuevo módulo con la lógica de autenticación y upload a Google Drive
- `server/requirements.txt` — agregar `google-api-python-client`, `google-auth`, `aiohttp` (servidor HTTP asyncio)
