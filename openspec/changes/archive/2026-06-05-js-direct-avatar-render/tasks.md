## 1. Crear modulo JS `avatarAnim.js`

- [x] 1.1 Crear `AvatarExpo/wwwroot/js/avatarAnim.js` con `window.avatarAnim = { ... }`
- [x] 1.2 Implementar `update(angles)` que aplica `element.style.transform` a: `#body-wrapper`, `#arm-left`, `#arm-right`, `#forearm-left`, `#forearm-right`, `#head`
- [x] 1.3 Implementar `resetToNeutral()` que llama `update` con todos los angulos en 0
- [x] 1.4 Agregar `<script>` tag en `wwwroot/index.html` para cargar `avatarAnim.js` (antes o despues del script de Blazor, pero antes de que se use)

## 2. Separar posicionamiento estatico del transform dinamico

- [x] 2.1 En `avatar.css`, crear clase `.arm-static-pos { position: absolute; top: 20px; z-index: 15; }`
- [x] 2.2 En `AvatarArm.razor`, reemplazar el `style` inline (`Style` parameter) por `class="arm-container @Side arm-static-pos"`
- [x] 2.3 En `AvatarArm.razor`, eliminar el `transform: rotate(ShoulderRotation)` del style del container principal (JS lo aplica)
- [x] 2.4 En `AvatarArm.razor`, eliminar el `transform: rotate(ElbowRotation)` del style del `.arm-lower` (JS lo aplica)
- [x] 2.5 En `AvatarRenderer.razor`, quitar `style="@TorsoTransform"` del `#body-wrapper` (JS lo aplica)
- [x] 2.6 En `AvatarTorso.razor`, quitar parametro `HeadTransform` y su binding en el template
- [x] 2.7 En `AvatarRenderer.razor`, quitar `HeadTransform="@HeadTransform"` del `<AvatarTorso>`
- [x] 2.8 Eliminar property `TorsoTransform` y campo `HeadTransform` de `AvatarRenderer.razor`

## 3. Modificar AvatarRenderer para enviar angulos a JS

- [x] 3.1 Agregar `@inject IJSRuntime JS` en `AvatarRenderer.razor`
- [x] 3.2 En `HandleLandmarks`, reemplazar `InvokeAsync(StateHasChanged)` por `_ = JS.InvokeVoidAsync("avatarAnim.update", new { leftShoulder, ... })` con null-check en JS
- [x] 3.3 Mantener el `InvokeAsync(StateHasChanged)` solo para cambios de expresion (cuando `ExpressionClass` cambia) — no cada frame
- [x] 3.4 En `HandleAvatarStateChange`, agregar llamada a `avatarAnim.update()` con los angulos demo (para que los sliders funcionen)
- [x] 3.5 En `ResetToNeutral`, reemplazar o complementar `InvokeAsync` con `avatarAnim.resetToNeutral()`

## 4. Reactivar CSS transitions y ajustar suavizado

- [x] 4.1 En `avatar.css`, agregar `transition: transform 0.05s ease-out;` a `.arm-container`
- [x] 4.2 En `avatar.css`, agregar `transition: transform 0.05s ease-out;` a `.arm-lower`
- [x] 4.3 En `avatar.css`, agregar `transition: transform 0.1s ease-out;` a `.body-wrapper`
- [x] 4.4 Reducir `LerpFactor` de `0.3` a `0.6` o `0.8` en `AvatarRenderer.razor` (la transicion CSS ahora suaviza; el Lerp de C# puede ser mas rapido)
- [x] 4.5 Verificar que `.head` ya tiene `transition: transform 0.1s ease-out;` (linea 62)

## 5. Verificacion

- [x] 5.1 Compilar `dotnet build` — 0 errores, 0 warnings
- [ ] 5.2 Probar tracking: el avatar 2D se mueve sincronizado con el esqueleto verde (sin latencia de 2s)
- [ ] 5.3 Probar tracking: los brazos, torso y cabeza responden a los movimientos del usuario
- [ ] 5.4 Probar modo demo: los sliders de torso y cabeza mueven el avatar instantaneamente
- [ ] 5.5 Probar modo demo: los sliders de brazos mueven el avatar
- [ ] 5.6 Probar personalizacion: cambiar remera, pelo, sombrero, anteojos sigue funcionando (Blazor)
- [ ] 5.7 Probar expresiones: sonrisa/sorpresa/enojo se reflejan en el avatar durante tracking
- [ ] 5.8 Verificar DevTools Console: sin errores de JS ("Cannot read property 'style' of null")
