## Context

El cambio anterior (`fix-screenshot-timestamp-and-capture-area`) pasó de `#avatar-wrapper` a `.panel-central`, pero `.panel-central` es demasiado grande e incluye UI. `.avatar-container` es el punto medio ideal: contiene el avatar completo sin UI, y tiene `width: 100%; height: 100%` con `overflow: hidden` en su padre.

## Goals / Non-Goals

**Goals:**
- Captura incluye avatar completo (torso, cabeza, brazos, items) sin clipping
- No incluye elementos de UI ("Avatar Render", skeleton canvas)
- Watermark ET12 correctamente centrado
- No se rompen degradados CSS ni SVG

**Non-Goals:**
- No se modifica el servidor ni el timestamp

## Decisions

### 1. Selector `.avatar-container`

Contenedor intermedio entre el panel y el wrapper:

```
.panel-central         ← demasiado grande (UI incluida)
  .avatar-container    ← ← ← JUSTO (avatar completo, sin UI)
    #avatar-wrapper     ← muy chico (180x250, clipea brazos/items)
```

### 2. Watermark centrado en el canvas

En vez de posicionarlo a `height * 0.25` (que depende del tamaño del canvas), centrarlo:

```js
var x = (canvas.width - wmWidth) / 2;
var y = (canvas.height - wmHeight) / 2;
```

Funciona para cualquier tamaño de canvas.

## Risks / Trade-offs

- **`.avatar-container` tiene `overflow: hidden` en su padre** → `.panel-central` tiene `overflow: hidden`. html2canvas captura la región visible del elemento, así que si los brazos se extienden más allá de `.panel-central` igual se clipean. Pero eso ya pasaba antes.
