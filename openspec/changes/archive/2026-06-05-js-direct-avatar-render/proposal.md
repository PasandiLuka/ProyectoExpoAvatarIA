## Why

El avatar 2D no se mueve durante el tracking porque cada frame dispara `InvokeAsync(StateHasChanged)`, que encadena un render completo de Blazor en el thread de UI de WASM. A 30 FPS, cada render tarda ~50ms — la cola crece sin parar y el DOM nunca se actualiza porque Blazor nunca llega al tope de la cola.

Mientras tanto, el esqueleto verde sí se mueve porque usa `_js.InvokeVoidAsync("avatarCamera.drawSkeleton")`, que dibuja directo en un `<canvas>` sin pasar por el pipeline de Blazor.

La solución: mover la aplicación de CSS transforms de las partes animadas del avatar (brazos, torso, cabeza) al mismo mecanismo que ya funciona para el esqueleto — JavaScript directo al DOM, sin Blazor de por medio. La parte estática (personalización de ropa, accesorios) se queda en Blazor.

## What Changes

- **Nuevo archivo `avatarAnim.js`**: recibe ángulos desde C# vía JS interop y aplica `element.style.transform` directo al DOM de los divs del avatar. Sin diff de Blazor, sin cola de renders.
- **`AvatarRenderer.razor`**: `HandleLandmarks` reemplaza `InvokeAsync(StateHasChanged)` por `_js.InvokeVoidAsync("avatarAnim.update", angles)`. Los campos de ángulos se mantienen para el modo demo.
- **`AvatarArm.razor`**: quitar `style` inline con el `rotate(ShoulderRotation)` — JS lo maneja. Dejar solo el posicionamiento estático.
- **`AvatarRenderer.razor` template**: quitar `style="@TorsoTransform"` del `body-wrapper` — JS lo maneja. El `TorsoTransform` property se elimina.
- **`AvatarTorso.razor`**: quitar `HeadTransform` del binding — JS lo maneja.
- **`avatar.css`**: agregar clases `.arm-static-left`, `.arm-static-right`, `.body-wrapper-static` con el posicionamiento que antes iba inline.
- **Reactivar `transition: transform` en CSS**: ahora que JS aplica los transforms, el CSS transition da suavizado gratuito sin costo de Blazor.

## Impact

- **`wwwroot/js/avatarAnim.js`** — nuevo, ~50 líneas
- **`AvatarRenderer.razor`** — modificar `HandleLandmarks`, quitar `style` inline del template, inyectar `IJSRuntime`, eliminar `TorsoTransform` property
- **`AvatarArm.razor`** — quitar `ShoulderRotation` y `ElbowRotation` del `style` inline
- **`AvatarTorso.razor`** — quitar parámetro `HeadTransform`
- **`AvatarHead.razor`** — mantener `ExpressionClass` (cambia con expresión, no a 30 FPS)
- **`avatar.css`** — agregar clases de posición estática, reactivar `transition: transform`

### Lo que NO cambia
- `TrackingService.cs`, `CameraService.cs`, `Models.cs` — el pipeline de datos sigue igual
- `server/server.py` — sin cambios
- Personalización (shirt, hair, hat, glasses) — sigue en Blazor, no son tiempo real
- Expresiones faciales — el `ExpressionClass` se sigue pasando por Blazor (cambia a baja frecuencia)
- Modo demo — los sliders siguen usando `AvatarState` + `HandleAvatarStateChange` como antes
