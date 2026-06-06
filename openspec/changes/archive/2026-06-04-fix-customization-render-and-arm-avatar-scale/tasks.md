## 1. Refactorizar AvatarTorso — recibir ShirtStyle como parámetro

- [x] 1.1 Agregar `[Parameter] public string? ShirtStyle { get; set; }` a `AvatarTorso.razor`
- [x] 1.2 Cambiar `class="torso @AvatarState.ShirtStyle"` por `class="torso @ShirtStyle"` en el markup
- [x] 1.3 Eliminar `@inject AvatarState AvatarState` de `AvatarTorso.razor`

## 2. Refactorizar AvatarHead — recibir HairStyle, HairColor, HatStyle, GlassesStyle como parámetros

- [x] 2.1 Agregar `[Parameter]` para `HairStyle`, `HairColor`, `HatStyle`, `GlassesStyle` en `AvatarHead.razor`
- [x] 2.2 Reemplazar todas las referencias a `AvatarState.HairStyle` → `HairStyle`, `AvatarState.HairColor` → `HairColor`, etc. en el markup
- [x] 2.3 Eliminar `@inject AvatarState AvatarState` de `AvatarHead.razor`

## 3. Refactorizar AvatarArm — recibir HandItem como parámetro

- [x] 3.1 Agregar `[Parameter] public string? LeftHandItem` y `[Parameter] public string? RightHandItem` a `AvatarArm.razor`
- [x] 3.2 Cambiar referencias `AvatarState.LeftHandItem` → `LeftHandItem` y `AvatarState.RightHandItem` → `RightHandItem` en el markup
- [x] 3.3 Eliminar `@inject AvatarState AvatarState` de `AvatarArm.razor`

## 4. Actualizar AvatarRenderer — pasar todos los parámetros

- [x] 4.1 Agregar paso de `ShirtStyle="@AvatarState.ShirtStyle"` a `<AvatarTorso>` en el markup
- [x] 4.2 Agregar paso de `HairStyle`, `HairColor`, `HatStyle`, `GlassesStyle` a `<AvatarHead>` vía `AvatarTorso`
- [x] 4.3 Agregar paso de `LeftHandItem` y `RightHandItem` a ambos `<AvatarArm>`
- [x] 4.4 Verificar que `HandleAvatarStateChange` sigue llamando `InvokeAsync(StateHasChanged)` para disparar re-render al cambiar AvatarState

## 5. Corregir transparencia del brazo superior

- [x] 5.1 Eliminar `style="background: inherit;"` del `<div class="arm-upper">` en `AvatarArm.razor`
- [x] 5.2 Agregar `background-color: var(--skin-color);` a `.arm-upper` en `avatar.css`
- [x] 5.3 Agregar selectores CSS `.torso.shirt-long ~ .arm-container .arm-upper { background-color: #2b2d42; }` y `.torso.shirt-river ~ .arm-container .arm-upper { background-color: #ffffff; }` en `avatar.css`

## 6. Escalar el avatar

- [x] 6.1 Agregar `transform-origin: center bottom;` a `.body-wrapper` en `avatar.css`
- [x] 6.2 Modificar `TorsoTransform` en `AvatarRenderer.razor` para incluir `scale(2)` al final de la cadena de transform

## 7. Verificación

- [x] 7.1 Compilar con `dotnet build` — 0 errores, 0 warnings
- [x] 7.2 Verificar en navegador: cambios de pelo, remera, gorros, anteojos se reflejan inmediatamente al clickear botones (sin tocar expresión/sliders)
- [x] 7.3 Verificar en navegador: cambios de objetos de mano se reflejan inmediatamente al clickear (sin tocar sliders)
- [x] 7.4 Verificar en navegador: el brazo superior tiene color de piel visible con remera manga corta
- [x] 7.5 Verificar en navegador: el avatar ocupa significativamente más espacio en el panel central
- [x] 7.6 Verificar en navegador: expresiones faciales y sliders del simulador siguen funcionando correctamente
