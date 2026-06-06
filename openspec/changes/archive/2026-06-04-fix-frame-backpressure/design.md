## Context

El pipeline de tracking sufre de acumulación de frames por ausencia de backpressure. El cliente dispara frames a 30 FPS (33ms) mientras el servidor procesa a ~55ms (MediaPipe pose + face_mesh). Sin frame dropping, los frames no procesados se acumulan en el buffer TCP del WebSocket y se procesan secuencialmente, cada uno con más latencia acumulada que el anterior.

Además, el skeleton canvas retiene el último frame dibujado al detener el tracking porque es un buffer stateful que nunca recibe `clearRect`.

## Goals / Non-Goals

**Goals:**
- Eliminar la acumulación de frames en el servidor mediante frame dropping
- Limpiar el skeleton canvas al detener el tracking
- Cero cambios en el cliente Blazor (salvo el cleanup del canvas)
- Mantener compatibilidad con el pipeline actual (WebSocket, landmarks, avatarAnim.js)

**Non-Goals:**
- Modificar `setInterval` en el cliente
- Cambiar el mecanismo de envío de frames (sigue siendo `toBlob` + `invokeMethodAsync`)
- Mover MediaPipe a un thread separado
- Modificar el sistema de RTT/FPS adjustment

## Decisions

### Decision 1: Frame dropping en el servidor vía drenado de WebSocket

En vez de `async for message in websocket` (que procesa todos los mensajes en orden), se usa un patrón de "drain then process":

```python
msg = await websocket.recv()           # bloquear en el primer frame
while True:                             # drenar todos los pendientes
    try:
        msg = await asyncio.wait_for(websocket.recv(), timeout=0.001)
    except asyncio.TimeoutError:
        break                           # no hay más frames, msg es el último
# procesar solo msg (el más reciente)
```

Esto reemplaza el `async for` actual. El timeout de 1ms es suficiente para detectar si hay frames pendientes sin agregar latencia perceptible.

**Alternativa considerada**: Usar `websocket.recv()` no-bloqueante con `asyncio.Queue(maxsize=1)`. Descartada porque agrega complejidad innecesaria — el patrón drain-then-process es más directo y legible.

**Alternativa considerada**: Frame dropping en el cliente (no enviar si el anterior no terminó). Descartada porque requiere modificar `camera.js` + `CameraService` + `TrackingService` en cascada, y no resuelve frames ya en vuelo por la red.

### Decision 2: Limpiar skeleton canvas en `camera.js stop()`

Agregar al final de `stop()`:

```javascript
var sc = document.getElementById('skeleton-canvas');
if (sc) {
    var ctx = sc.getContext('2d');
    if (ctx) ctx.clearRect(0, 0, 640, 480);
}
```

Es el lugar natural porque `stop()` ya se encarga de limpiar el resto del estado de la cámara (interval, video tracks). El skeleton canvas es parte de ese estado.

### Decision 3: Safety net en `CameraPanel.razor`

En `ToggleTracking()`, al detener, agregar:

```csharp
await JS.InvokeVoidAsync("eval",
    "(function(){var c=document.getElementById('skeleton-canvas');if(c)c.getContext('2d').clearRect(0,0,640,480);})()");
```

Esto cubre edge cases donde `stop()` no se ejecuta correctamente (ej: el usuario cierra la pestaña, el browser crashea, o `stop()` lanza una excepción antes de llegar al clearRect).

**Alternativa considerada**: Exponer una función `clearSkeleton()` en `camera.js` y llamarla desde C#. Descartada por overkill — es una línea de JS inline para un cleanup simple.

## Risks / Trade-offs

- **[Riesgo bajo] Pérdida de frames**: El frame dropping descarta frames intermedios. En tracking de pose en tiempo real esto es deseable (siempre querés la pose más reciente). No hay información crítica en los frames descartados.
- **[Riesgo bajo] Timeout de 1ms**: Si el sistema está bajo carga extrema, 1ms puede no ser suficiente para recibir el siguiente frame del buffer TCP. El peor caso: se procesa un frame "casi reciente" en vez del último. El lag sería de ~1 frame adicional (~33ms), imperceptible.
- **[Riesgo bajo] `eval` en C#**: Usar `eval` para ejecutar JS inline es aceptable para un cleanup simple. Si se prefiere evitar `eval`, se puede mover a una función en `camera.js`.
