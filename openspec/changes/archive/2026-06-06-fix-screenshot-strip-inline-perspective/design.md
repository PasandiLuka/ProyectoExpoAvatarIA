## Context

`avatarAnim.update()` escribe cada frame:
```js
bw.style.transform = 'perspective(800px) rotateY(X) rotateZ(Y) scale(Z)';
```
El `perspective(800px)` dentro del `transform` crea un contexto 3D a nivel de elemento que html2canvas no soporta. Las propiedades CSS (`perspective: none`, `transform-style: flat`) son ignoradas porque el inline style tiene prioridad.

## Goals / Non-Goals

**Goals:**
- Eliminar el contexto 3D del transform inline durante la captura
- Conservar la pose del avatar (rotateY, rotateZ, scale) en la foto
- Restaurar el transform original después de capturar

**Non-Goals:**
- No se modifica `avatarAnim.js`

## Decisions

### 1. Strip de `perspective()` del transform inline

```js
var bw = document.getElementById('avatar-wrapper');
var originalTransform = bw.style.transform;
bw.style.transform = bw.style.transform.replace(/perspective\([^)]+\)\s*/, '');
// ... html2canvas ...
bw.style.transform = originalTransform;
```

El regex `perspective\([^)]+\)\s*` remueve `perspective(800px)` y el espacio que le sigue. El resto del transform (`rotateY(X) rotateZ(Y) scale(Z)`) se conserva → el avatar mantiene su pose en la captura.

### 2. Combinado con las desactivaciones CSS existentes

Se mantienen `panel.style.perspective = 'none'` y `wrapper.style.transformStyle = 'flat'` del cambio anterior. El strip del inline es la capa adicional que faltaba.

## Risks / Trade-offs

- **Si el regex falla** (formato inesperado del transform) → el avatar se captura con perspectiva (comportamiento actual). No es crítico, solo no arregla el bug.
- **El transform se restaura en finally** → incluso si html2canvas falla, el 3D vuelve.
