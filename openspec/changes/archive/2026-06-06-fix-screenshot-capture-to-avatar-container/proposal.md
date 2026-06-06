## Why

Capturar `.panel-central` en vez de `#avatar-wrapper` arregló el clipping del avatar pero introdujo 5 nuevos bugs: avatar traslúcido, watermark descentrado, pochoclos rotos (degradados CSS mal renderizados), sable láser cambió color, y el cartel "Avatar Render" de la UI aparece en la foto. La causa raíz es que el canvas es demasiado grande y contiene elementos de UI.

## What Changes

- **Selector de captura**: `.panel-central` → `.avatar-container` (contiene el avatar completo, excluye UI)
- **Watermark centrado**: posicionamiento centrado en el canvas en vez de `height * 0.25`

## Capabilities

### Modified Capabilities
- `screenshot-upload`: la captura debe ser del área del avatar (`.avatar-container`), excluyendo UI, con watermark correctamente centrado

## Impact

- `AvatarExpo/wwwroot/js/camera.js` — selector y posicionamiento de watermark
