## Context

El servidor WebSocket Python recibe frames JPEG binarios del cliente Blazor WASM. Procesa cada frame con MediaPipe Pose y Face Mesh. El bug: un `continue` mal indentado en `server.py:102` descarta todos los mensajes binarios — solo los mensajes de texto (ping) son procesados. Corregido el indent, el servidor procesa frames correctamente.

Para el skeleton overlay, MediaPipe ya detecta los 33 landmarks de pose en cada frame. El servidor actualmente solo envía 6 landmarks clave (hombros, codos, muñecas). Para el esqueleto necesitamos enviar todos los landmarks y sus conexiones.

## Goals / Non-Goals

**Goals:**
- Corregir el bug de indentación para que el tracking funcione
- Mostrar un esqueleto verde en tiempo real sobre la vista de cámara
- Incluir los 33 landmarks de pose en el JSON de respuesta
- Dibujar líneas de conexión entre landmarks siguiendo la topología de MediaPipe

**Non-Goals:**
- Mostrar la malla facial (solo pose skeleton)
- Dibujar el esqueleto sobre el avatar (solo sobre la cámara)
- Modificar el pipeline de tracking del avatar (eso ya funciona una vez corregido el bug)

## Decisions

### Decisión 1: Enviar landmarks en coordenadas normalizadas (0-1)

El servidor envía landmarks normalizados. El cliente convierte a píxeles usando las dimensiones del canvas. Esto mantiene el protocolo independiente de resolución.

### Decisión 2: Usar un `<canvas>` overlay en vez de SVG/DOM

Un canvas permite dibujar líneas y puntos eficientemente sin crear/eliminar elementos DOM en cada frame. El canvas se posiciona con `position: absolute` sobre el `<video>` de la cámara.

### Decisión 3: Estructura del campo `skeleton` en JSON

```json
{
  "skeleton": {
    "landmarks": [[0.5, 0.6, 0.1], [0.4, 0.5, 0.0], ...],
    "connections": [[11, 12], [11, 13], [13, 15], ...],
    "width": 640,
    "height": 480
  }
}
```

- `landmarks`: Array de 33 arrays [x, y, z] normalizados
- `connections`: Pares de índices que conectan landmarks
- `width`/`height`: Dimensiones del frame original para conversión a píxeles

### Decisión 4: Conexiones definidas en el servidor

Las conexiones del esqueleto de MediaPipe Pose son fijas (33 landmarks, conjunto conocido de pares). Se definen como constante en el servidor y se envían en cada frame. Esto es ~1KB adicional por frame, aceptable.

## Risks / Trade-offs

- **[Riesgo] Overhead de JSON**: Enviar 33 landmarks en vez de 6 aumenta el payload de ~200 bytes a ~1.5KB → Mitigación: El payload sigue siendo pequeño comparado con el frame JPEG (~15-30KB)
- **[Riesgo] Rendimiento del canvas**: Dibujar 33 puntos + ~11 líneas en cada frame → Mitigación: Operación trivial (< 1ms), el canvas se limpia y redibuja eficientemente
