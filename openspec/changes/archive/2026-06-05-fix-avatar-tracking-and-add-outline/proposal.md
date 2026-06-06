## Why

El tracking de movimiento con MediaPipe funciona correctamente (el esqueleto verde se mueve con el cuerpo del usuario), pero el avatar 2D no refleja estos movimientos. Tampoco funcionan los sliders de rotación de torso e inclinación de cabeza en el modo demo. Adicionalmente, el avatar necesita un contorno negro para diferenciarse del fondo oscuro.

## What Changes

- **Fix angle computation**: Los ángulos de hombro calculados con `Atan2` necesitan un offset de -90° para alinearse con el sistema de coordenadas CSS (donde 0° = apuntando hacia abajo). Sin este offset las articulaciones del avatar quedan desfasadas 90°.
- **Add head tracking**: `HandleLandmarks` actualmente no computa ni aplica rotación de cabeza durante el tracking. Se agregará cálculo de inclinación lateral de la cabeza usando los landmarks de hombros y nariz.
- **Fix demo sliders binding**: Reemplazar `@bind:after` con handlers `@oninput` explícitos que escriban directamente a `AvatarState` y disparen la notificación, evitando posibles problemas de timing con el binding de Blazor.
- **Add black outline to avatar**: Agregar `filter: drop-shadow()` o contornos CSS a todas las partes del avatar (torso, cabeza, brazos, manos) para crear una silueta negra visible sobre el fondo oscuro.

## Impact

- **AvatarRenderer.razor**: Corregir `HandleLandmarks` con offset -90°, agregar head tracking
- **CameraPanel.razor**: Reemplazar `@bind:after` con handlers `@oninput` explícitos en sliders de torso y cabeza
- **avatar.css**: Agregar contorno negro a `.torso`, `.head`, `.arm-upper`, `.arm-lower`, `.hand`
