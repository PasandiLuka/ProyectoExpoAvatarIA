## Context

html2canvas renderiza en 2D. El avatar usa `perspective: 800px` en `.panel-central` y `transform-style: preserve-3d` en `#avatar-wrapper`. Al capturar, html2canvas calcula mal posiciones 3D, corrompe blending de capas (opacidad), y recalcula degradados/SVG incorrectamente sobre el stacking context 3D.

## Goals / Non-Goals

**Goals:**
- Captura con avatar 100% opaco
- Accesorios (CSS degradados, SVG) renderizan correctamente
- Sin UI en la foto
- Watermark centrado
- El 3D se restaura inmediatamente después de capturar (sin flicker visible)

**Non-Goals:**
- La foto no tiene profundidad 3D (es plana). Es un trade-off aceptado.

## Decisions

### 1. Desactivar 3D antes de capturar, restaurar después

```js
// Antes
panel.style.perspective = 'none';
wrapper.style.transformStyle = 'flat';
await html2canvas(container, ...);
// Después
panel.style.perspective = '';
wrapper.style.transformStyle = '';
```

La operación es sincrónica e instantánea. El usuario no percibe el cambio porque html2canvas es asíncrono y el navegador no hace repaint entre set/unset de estas propiedades en la misma microtask.

### 2. Mantener `.avatar-container` como selector

Ya lo tenemos de `fix-screenshot-capture-to-avatar-container`. Funciona bien para excluir UI. Solo agregamos el toggle 3D.

## Risks / Trade-offs

- **Foto plana vs 3D** → aceptado. La alternativa (WebGL offscreen rendering) es viable pero compleja y fuera de scope.
- **Si html2canvas tarda, el 3D se ve desactivado** → el `await html2canvas()` bloquea. Si tarda 2-3 segundos, el avatar se vería plano en pantalla ese tiempo. Mitigación: el botón muestra "Capturando..." y la UI está bloqueada, así que el usuario no interactúa.
