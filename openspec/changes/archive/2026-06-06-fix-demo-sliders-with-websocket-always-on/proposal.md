## Why

Tras implementar WebSocket siempre conectado (`fix-screenshot-upload-flow`), los sliders de demo dejaron de funcionar. `AvatarRenderer.HandleAvatarStateChange()` usaba `!TrackingService.IsConnected` como condición para aplicar valores demo. Como ahora el WebSocket siempre está conectado, los sliders nunca se aplican. Solo los botones de expresión funcionan porque están fuera de ese `if`.

## What Changes

- `AvatarRenderer.razor`: reemplazar la condición `!TrackingService.IsConnected` por un flag local `_landmarksActive` que refleje si se están recibiendo landmarks reales (tracking activo). Se resetea con un timer de inactividad de 2s para cubrir el caso "detener cámara sin desconectar WebSocket".

## Capabilities

### Modified Capabilities
- `screenshot-upload`: el WebSocket siempre conectado no debe interferir con los sliders demo. Los valores demo se aplican cuando no hay tracking activo, independientemente del estado del WebSocket.

## Impact

- `AvatarExpo/Components/AvatarRenderer.razor` — `HandleAvatarStateChange()` y `HandleLandmarks()`. Una sola condición cambia.
