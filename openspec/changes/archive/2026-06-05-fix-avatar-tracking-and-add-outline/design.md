## Context

El sistema de tracking envía landmarks de MediaPipe al cliente Blazor WASM vía WebSocket. El servidor incluye 6 landmarks clave (`ls`, `rs`, `le`, `re`, `lw`, `rw` en coordenadas normalizadas) en el campo `p`, y 33 landmarks completos + conexiones en el campo `skeleton`. El skeleton se dibuja correctamente en el canvas overlay de la cámara, pero el avatar 2D (compuesto por `<div>` con CSS transforms) no refleja los movimientos.

El avatar se renderiza con divs posicionados con CSS `transform`: los brazos usan `rotate()`, el torso usa `rotateY()` y `rotateZ()`, y la cabeza usa `rotate()`.

## Goals / Non-Goals

**Goals:**
- Corregir el offset de -90° en el cálculo de ángulos de hombro para que coincidan con el sistema de coordenadas CSS
- Agregar rotación de cabeza (inclinación lateral) durante el tracking
- Hacer que los sliders de torso y cabeza funcionen en modo demo
- Agregar contorno negro (silueta) a todas las partes del avatar

**Non-Goals:**
- Modificar el servidor Python (el pipeline de tracking ya funciona)
- Cambiar la estructura de datos del esqueleto
- Agregar rotación de cabeza 3D (solo inclinación lateral 2D)

## Decisions

### Decisión 1: Offset de -90° en ángulos de hombro

El problema: `Atan2(y, x)` retorna 0° cuando el vector apunta a la derecha (eje X positivo). Pero en CSS, las partes del avatar parten apuntando hacia abajo (0° = posición original hacia abajo). La conversión es:

```
CSS_angle = Atan2_degrees - 90°
```

Para el codo (ángulo relativo), el offset se cancela en la resta:
```
GlobalElbow - ShoulderAngle = (TrueGlobalElbow - 90) - (TrueShoulder - 90) = TrueGlobalElbow - TrueShoulder
```

Por lo tanto solo hay que ajustar el ángulo de hombro, no el de codo.

Se agrega `- 90.0` a `leftTargetShoulder` y `rightTargetShoulder` en `HandleLandmarks`.

### Decisión 2: Head tracking con inclinación lateral

Se calcula la inclinación de la cabeza usando la diferencia de altura (y) entre los hombros, ya que `ProcessedLandmarks` no incluye landmarks faciales de posición (solo expresión).

```
headTiltZ = (RSy - LSy) * 30  → clamp(-15, 15)
```

Esto produce una inclinación sutil de la cabeza en dirección opuesta a la inclinación de hombros (contra-balance natural). Se aplica mediante:

```
HeadTransform = $"transform: rotate({headTiltZ:F1}deg);"
```

### Decisión 3: Sliders con `@oninput` explícito

Se reemplaza el patrón `@bind="X" @bind:event="oninput" @bind:after="Sync"` por handlers `@oninput` explícitos que:
1. Parsean el valor del input a double
2. Asignan directamente a `AvatarState`
3. Llaman a `AvatarState.NotifyStateChanged()`

Esto elimina cualquier problema de timing con el sistema de binding de Blazor y asegura que los sliders de torso y cabeza respondan inmediatamente.

### Decisión 4: Contorno negro con `filter: drop-shadow()`

Se usa `filter: drop-shadow(0 0 1px black)` o `box-shadow` en las partes principales del avatar. `drop-shadow` es preferible porque sigue la forma del elemento (incluyendo border-radius), creando una silueta más natural que `outline` o `border`.

Alternativa: múltiples `box-shadow` en 8 direcciones para simular un contorno. Se evaluará cuál se ve mejor en la práctica.

## Risks / Trade-offs

- **[Riesgo bajo] Offset -90°**: Si el offset es incorrecto, los brazos apuntarán en direcciones incorrectas. Se verifica con la posición neutral (brazos abajo) que debe dar ~0°.
- **[Riesgo bajo] Head tracking**: La inclinación calculada desde hombros es una aproximación. Para tracking preciso se necesitarían landmarks faciales de posición, pero esto requiere modificar el servidor.
- **[Riesgo bajo] Sliders**: El cambio a `@oninput` explícito es más verboso pero más controlable y evita bugs de binding.
- **[Trade-off] Performance del outline**: `drop-shadow()` tiene costo de GPU pero en un avatar estático de pocos elementos es insignificante.
