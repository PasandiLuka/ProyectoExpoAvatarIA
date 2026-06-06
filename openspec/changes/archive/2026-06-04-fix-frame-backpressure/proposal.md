## Why

El pipeline de tracking acumula frames en el servidor porque el cliente dispara a 30 FPS (cada 33ms) pero MediaPipe procesa a ~55ms por frame. Como no hay frame dropping, los frames se encolan en el buffer TCP del WebSocket y se procesan en orden — cada uno con más retraso que el anterior. El avatar muestra poses de hace 200-500ms, produciendo un efecto de "quedarse atrás en el tiempo".

Además, al detener el tracking, el esqueleto verde dibujado en el `<canvas>` persiste porque nadie llama a `clearRect`. El canvas retiene el último frame dibujado hasta que se limpie explícitamente.

## What Changes

- **`server.py`**: antes de procesar un frame con MediaPipe, drenar todos los frames pendientes del WebSocket y quedarse solo con el más reciente (frame dropping). Esto garantiza que el servidor siempre procese la pose más actual, no una cola de poses viejas.
- **`camera.js` `stop()`**: agregar `clearRect` al `#skeleton-canvas` para que al detener la cámara desaparezca el esqueleto.
- **`CameraPanel.razor`**: al detener tracking, llamar a JS para limpiar el skeleton canvas como safety net.

## Impact

- **`server/server.py`**: modificar `handle_connection` — drenar WebSocket antes de `pose.process()`
- **`AvatarExpo/wwwroot/js/camera.js`**: modificar `stop()` — agregar limpieza del skeleton canvas
- **`AvatarExpo/Components/CameraPanel.razor`**: modificar `ToggleTracking()` — limpiar canvas al detener
