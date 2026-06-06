## Why

Desactivar `perspective` como propiedad CSS no es suficiente: `avatarAnim.js` aplica `perspective(800px)` como **función dentro del transform inline** del `#avatar-wrapper` (30 veces por segundo). Las propiedades inline tienen prioridad máxima y sobreescriben cualquier CSS. Para eliminar el renderizado 3D que corrompe html2canvas, hay que quitar la función `perspective()` del string de transform inline antes de capturar.

## What Changes

- **camera.js**: antes de `html2canvas`, guardar el `transform` inline del `#avatar-wrapper`, remover `perspective(Xpx)` del string, y restaurarlo después de capturar. Mantiene el resto del transform (`rotateY`, `rotateZ`, `scale`) para que el avatar conserve su pose en la foto.

## Capabilities

### Modified Capabilities
- `screenshot-upload`: la captura elimina temporalmente la función `perspective()` del transform inline del wrapper para garantizar renderizado 2D limpio

## Impact

- `AvatarExpo/wwwroot/js/camera.js` — `captureAvatarToBase64()`: agregar strip/restore del transform inline
