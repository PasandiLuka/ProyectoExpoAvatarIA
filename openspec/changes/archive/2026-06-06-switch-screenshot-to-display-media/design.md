## Context

html2canvas no puede con el avatar. `getDisplayMedia` es la API nativa del navegador para capturar píxeles reales de la pantalla/pestaña. Devuelve un `MediaStream` del cual se pueden extraer frames con `ImageCapture.grabFrame()`.

## Goals / Non-Goals

**Goals:**
- Captura píxel-perfect del avatar (3D, sombras, SVG, degradados incluidos)
- Un solo diálogo de permiso al inicio de la sesión
- Recorte automático al área del avatar
- Fallback a html2canvas si el navegador no soporta `getDisplayMedia`

**Non-Goals:**
- No se modifica el servidor ni el flujo de subida a Drive
- No se elimina html2canvas (se mantiene como fallback)

## Decisions

### 1. Stream persistente (1 diálogo, no cada foto)

```js
// En start() o en la primera captura:
this.displayStream = await navigator.mediaDevices.getDisplayMedia({
    video: { width: 1920, height: 1080, frameRate: 30 },
    preferCurrentTab: true
});
```

El stream se guarda en `avatarCamera.displayStream`. Los frames se extraen con `ImageCapture.grabFrame()`. El diálogo de permiso aparece una sola vez. Chrome muestra un indicador sutil en la barra de pestañas mientras el stream esté activo.

### 2. Recorte al área del avatar

```js
var wrapper = document.getElementById('avatar-wrapper');
var rect = wrapper.getBoundingClientRect();
var dpr = window.devicePixelRatio || 1;

var cropped = document.createElement('canvas');
cropped.width = rect.width * dpr * 2;   // scale 2x
cropped.height = rect.height * dpr * 2;

cropped.getContext('2d').drawImage(
    bitmap, 0, 0,                     // full bitmap
    rect.left * dpr, rect.top * dpr, rect.width * dpr, rect.height * dpr,  // source
    0, 0, cropped.width, cropped.height  // dest
);
```

Pero `ImageCapture.grabFrame()` devuelve un `ImageBitmap`, no un `VideoFrame`. El `ImageBitmap` tiene las dimensiones del stream de video, no del viewport. Hay que mapear coordenadas.

**Simplificación:** usar un `<video>` oculto que reproduzca el stream, y `ctx.drawImage(video, ...)` con source rect del bounding box del avatar. Esto evita problemas de mapeo de coordenadas.

### 3. Flujo de captura

```
Sacar Foto → countdown → 
  ↓
video frame → canvas temporal → recortar al avatar wrapper → 
  ↓
watermark → toDataURL → base64 → WebSocket → Drive
```

### 4. Fallback

Si `getDisplayMedia` lanza (Firefox, permiso denegado, etc.), se usa html2canvas con dimensiones calculadas por bounding box.

```js
try {
    return await this.captureWithDisplayMedia();
} catch {
    return await this.captureWithHtml2canvas();
}
```

## Risks / Trade-offs

- **Diálogo de permiso** → 1 vez al inicio, no cada foto. Chrome muestra indicador de sharing.
- **El recorte puede desalinearse si el avatar se mueve** → el bounding box se calcula en el momento de la captura, así que siempre está actualizado.
- **Firefox/Safari no soportan `preferCurrentTab`** → cae en fallback html2canvas, sin diálogo intrusivo.
- **`ImageCapture` no disponible en Firefox** → fallback a `<video>` + `drawImage`.
