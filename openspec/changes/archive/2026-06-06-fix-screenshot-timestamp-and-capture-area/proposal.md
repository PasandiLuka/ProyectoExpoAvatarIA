## Why

El screenshot tiene tres defectos: (1) el timestamp del archivo usa UTC en vez de hora argentina (UTC-3), (2) el área de captura (`#avatar-wrapper` de 180x250px) clipea el avatar — solo se ve el pecho, y (3) los accesorios (items de mano, gorros, anteojos) se capturan parcialmente o no se ven porque están fuera del área de captura.

## What Changes

- **Timestamp en hora argentina**: `DateTime.UtcNow` → conversión a `Argentina Standard Time` (UTC-3) en `TrackingService.SendScreenshot()`
- **Área de captura ampliada**: `camera.js` captura `.panel-central` en vez de `#avatar-wrapper`, cubriendo brazos, items, y cabeza completos

## Capabilities

### Modified Capabilities
- `screenshot-upload`: el screenshot debe capturar el avatar completo (torso, cabeza, brazos, items) y usar timestamp en hora local argentina

## Impact

- `AvatarExpo/Services/TrackingService.cs` — `SendScreenshot()`: conversión de timezone
- `AvatarExpo/wwwroot/js/camera.js` — `captureAvatarToBase64()`: selector de captura ampliado
