## Why

html2canvas falla sistemáticamente con el avatar: CSS 3D, degradados, SVG inline, posición absoluta de brazos/items. Cuatro iteraciones no lo resolvieron. `getDisplayMedia` captura exactamente lo que el usuario ve en pantalla — píxel perfecto, sin reconstrucción artificial.

## What Changes

- **Nuevo método de captura**: `navigator.mediaDevices.getDisplayMedia({ preferCurrentTab: true })` al iniciar la app. Stream persistente (1 solo diálogo de permiso). Cada "Sacar Foto" usa `ImageCapture.grabFrame()` para obtener un bitmap del frame actual, lo recorta al `getBoundingClientRect()` del avatar, aplica watermark, y retorna base64.
- **Fallback a html2canvas**: si `getDisplayMedia` falla (Firefox, usuario deniega permiso), se usa html2canvas con dimensiones calculadas por bounding box.
- **Se eliminan las 3 capas de desactivación 3D**: ya no son necesarias.

## Capabilities

### Modified Capabilities
- `screenshot-upload`: la captura usa `getDisplayMedia` para obtener un frame real del renderizado del navegador, recortado al área del avatar, con fallback a html2canvas

## Impact

- `AvatarExpo/wwwroot/js/camera.js` — nuevo método `captureAvatarToBase64()`, inicialización del stream en `start()`, limpieza en `stop()`
