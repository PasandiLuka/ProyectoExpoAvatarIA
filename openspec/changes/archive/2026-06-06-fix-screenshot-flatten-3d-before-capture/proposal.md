## Why

Tres iteraciones de cambio de selector (`#avatar-wrapper` → `.panel-central` → `.avatar-container`) no resolvieron el problema de raíz: html2canvas no soporta CSS 3D (`perspective`, `transform-style: preserve-3d`). El renderizado en contexto 3D corrompe opacidad, posicionamiento, degradados y SVG. La solución correcta es desactivar temporalmente el 3D antes de capturar y restaurarlo después.

## What Changes

- **camera.js**: antes de `html2canvas`, desactivar `perspective` en `.panel-central` y `transform-style` en `#avatar-wrapper`. Después de capturar, restaurar ambos. El avatar se captura plano (2D) pero con colores, degradados, SVG y accesorios fieles al diseño.

## Capabilities

### Modified Capabilities
- `screenshot-upload`: la captura se realiza en modo 2D plano desactivando temporalmente las propiedades CSS 3D del avatar, garantizando renderizado correcto de colores, opacidad y accesorios

## Impact

- `AvatarExpo/wwwroot/js/camera.js` — `captureAvatarToBase64()`: agregar toggle de `perspective` y `transform-style` antes/después de html2canvas
