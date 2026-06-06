## Context

Dos problemas independientes que afectan la usabilidad de la app en entornos reales (no solo desarrollo local):

1. El avatar no está contenido en su panel: los brazos, cabeza y manos se desbordan al rotar, y el `scale(2)` fijo no se adapta a distintos tamaños de pantalla.
2. La URL del WebSocket está hardcodeada a `localhost`, impidiendo que otros dispositivos en la red usen la app.

## Goals / Non-Goals

**Goals:**
- El avatar nunca se sale del panel central, sin importar el movimiento del usuario
- El avatar ocupa ~85% del panel central, aprovechando el espacio disponible
- La URL del WebSocket se construye dinámicamente a partir del host HTTP
- Cero configuración manual: funciona automáticamente desde cualquier dispositivo en la red

**Non-Goals:**
- Redibujar el avatar con SVG/canvas (sigue siendo CSS divs)
- Modificar la estructura de los componentes Blazor
- Agregar UI para configurar la URL del WebSocket

## Decisions

### Decision 1: `overflow: hidden` en `.panel-central`

Es la forma más simple y efectiva de contener el avatar. Todo lo que exceda los límites del panel se recorta. Como el avatar está centrado, el recorte es simétrico.

### Decision 2: Escala dinámica en `avatarAnim.js`

En vez de `scale(2)` hardcodeado, se calcula:

```javascript
var container = document.querySelector('.avatar-container');
var scale = Math.min(container.clientWidth / 180, container.clientHeight / 400) * 0.85;
```

- `180` = ancho base del body-wrapper (sin scale)
- `400` = alto estimado del avatar completo (torso 250 + head 160 + margen)
- `0.85` = factor de seguridad para que nunca toque los bordes

La escala se recalcula en cada `update()` para adaptarse a redimensiones de ventana.

**Alternativa considerada**: CSS `transform: scale(var(--avatar-scale))` con una variable CSS seteada desde JS. Descartada por complejidad innecesaria — JS ya maneja el transform, agregar la escala ahí es trivial.

### Decision 3: WebSocket URL desde `BaseAddress`

```csharp
var wsUrl = "ws://" + new Uri(builder.HostEnvironment.BaseAddress).Host + ":8765";
```

`builder.HostEnvironment.BaseAddress` contiene la URL base desde la que se sirvió la app WASM. Si el usuario accede desde `http://192.168.1.5:5000`, `BaseAddress` es `http://192.168.1.5:5000/`, y el host extraído es `192.168.1.5`.

**Alternativa considerada**: Leer `window.location.hostname` desde JS. Descartada porque `BaseAddress` ya contiene esa información y no requiere JS interop adicional.

**Alternativa considerada**: Mantener `appsettings.json` y esperar que el usuario lo configure. Descartada porque requiere conocimiento técnico y no es "plug and play".

## Risks / Trade-offs

- **[Riesgo bajo] `overflow: hidden`**: Si el avatar está mal centrado, partes podrían quedar recortadas incluso en posición neutral. Se mitiga con el factor 0.85 en la escala y el centrado flex.
- **[Riesgo bajo] Escala dinámica**: Recalcular en cada `update()` es trivial (2 divisiones, 1 Math.min). Sin overhead perceptible.
- **[Riesgo bajo] WebSocket desde BaseAddress**: Si la app se sirve en HTTPS, `ws://` podría fallar (mixed content). Se podría necesitar `wss://` en el futuro. Por ahora, la app corre en HTTP local.
