## Why

El tracking de movimiento no funciona a pesar de que la cámara está conectada y el backend Python reporta "Client connected". El servidor descarta todos los frames binarios JPEG debido a un `continue` mal indentado en el loop de WebSocket. Adicionalmente, no hay feedback visual en la vista de cámara que muestre lo que MediaPipe está detectando.

## What Changes

- **Fix tracking bug**: Corregir indentación del `continue` en `server.py:102` para que solo salte mensajes de texto, permitiendo que los frames binarios JPEG lleguen al pipeline de MediaPipe
- **Add skeleton overlay**: Agregar un canvas overlay con esqueleto verde sobre la vista de cámara para visualizar los landmarks detectados por MediaPipe en tiempo real
- **Server sends full pose landmarks**: Modificar el payload JSON del servidor para incluir todos los 33 landmarks de pose con sus conexiones, permitiendo dibujar el esqueleto completo
- **Client renders skeleton**: Agregar función `drawSkeleton()` en `camera.js` que dibuja líneas verdes y puntos sobre un canvas transparente superpuesto al video

## Capabilities

### New Capabilities

- `skeleton-overlay`: Visualización en tiempo real del esqueleto detectado por MediaPipe sobre la vista de cámara, usando líneas verdes y puntos sobre un canvas overlay

### Modified Capabilities

- `websocket-tracking`: El payload JSON ahora incluye todos los 33 landmarks de pose y conexiones para el dibujo del esqueleto. La corrección del `continue` permite que los frames binarios sean procesados.

## Impact

- **Server**: `server.py` — fix indentación, agregar campo `skeleton` con landmarks completos y `connections`
- **Client JS**: `camera.js` — agregar canvas overlay y función `drawSkeleton()`
- **Client C#**: `CameraPanel.razor` — agregar `<canvas id="skeleton-canvas">` overlay sobre el video
- **Client C#**: `TrackingService.cs` / `Models.cs` — parsear el nuevo campo `skeleton` del JSON
- **Protocolo**: El JSON de respuesta agrega un campo `"skeleton"` con landmarks + connections (no **BREAKING** — campo nuevo aditivo)
