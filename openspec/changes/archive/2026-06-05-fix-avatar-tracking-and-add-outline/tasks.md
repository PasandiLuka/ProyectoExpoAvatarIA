## 1. Corregir offset de -90° en ángulos de hombro

- [x] 1.1 En `AvatarRenderer.razor`, agregar `- 90.0` a `leftTargetShoulder` (línea 132)
- [x] 1.2 Agregar `- 90.0` a `rightTargetShoulder` (línea 136)
- [x] 1.3 Verificar que `leftTargetElbow` y `rightTargetElbow` NO necesitan offset (se cancela en la resta)

## 2. Agregar head tracking en HandleLandmarks

- [x] 2.1 En `HandleLandmarks`, calcular `headTiltZ = Math.Clamp((data.RSy - data.LSy) * 30.0, -15, 15)` (inclinación lateral basada en diferencia de altura de hombros)
- [x] 2.2 Suavizar con `Lerp` usando una variable `_headTiltZ` (mismo `LerpFactor = 0.3`)
- [x] 2.3 Asignar `HeadTransform = $"transform: rotate({_headTiltZ:F1}deg);"`

## 3. Agregar variable de estado para head tilt con Lerp

- [x] 3.1 Agregar campo `private double _headTiltZ;` en `AvatarRenderer.razor`
- [x] 3.2 Inicializar en 0 en `ResetToNeutral()`

## 4. Arreglar sliders de torso y cabeza en CameraPanel

- [x] 4.1 Reemplazar `@bind="TorsoSlider" @bind:event="oninput" @bind:after="SyncSlidersToState"` por `@oninput="OnTorsoInput" value="@TorsoSlider"`
- [x] 4.2 Reemplazar `@bind="HeadSlider" @bind:event="oninput" @bind:after="SyncSlidersToState"` por `@oninput="OnHeadInput" value="@HeadSlider"`
- [x] 4.3 Agregar método `OnTorsoInput(ChangeEventArgs e)` que parsea el valor, asigna a `AvatarState.DemoTorsoY` y notifica
- [x] 4.4 Agregar método `OnHeadInput(ChangeEventArgs e)` que parsea el valor, asigna a `AvatarState.DemoHeadTilt` y notifica
- [x] 4.5 Limpiar `SyncSlidersToState` para no duplicar lógica (o eliminarla si ya no se usa)

## 5. Agregar contorno negro al avatar en CSS

- [x] 5.1 Agregar `filter: drop-shadow(0 0 1.5px black) drop-shadow(0 0 1.5px black);` a `.torso` en `avatar.css`
- [x] 5.2 Agregar el mismo `filter` a `.head`
- [x] 5.3 Agregar `filter: drop-shadow(0 0 1.5px black);` a `.arm-upper` y `.arm-lower`
- [x] 5.4 Agregar `filter: drop-shadow(0 0 1.5px black);` a `.hand`
- [x] 5.5 Verificar que el contorno es visible sobre el fondo `#2a2a35` del panel central

## 6. Verificación

- [x] 6.1 Compilar `dotnet build` — 0 errores, 0 warnings
- [ ] 6.2 Probar tracking: el avatar sigue los movimientos del usuario con ángulos correctos
- [ ] 6.3 Probar tracking: la cabeza se inclina lateralmente al mover los hombros
- [ ] 6.4 Probar modo demo: el slider de rotación de torso gira el avatar en 3D
- [ ] 6.5 Probar modo demo: el slider de inclinación de cabeza rota la cabeza
- [ ] 6.6 Verificar visualmente: el avatar tiene un contorno negro visible sobre el fondo oscuro
