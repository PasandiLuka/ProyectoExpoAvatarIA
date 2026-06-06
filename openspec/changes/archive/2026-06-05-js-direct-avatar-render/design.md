## Context

El pipeline de tracking funciona: el servidor Python envía landmarks, `TrackingService` deserializa, `LandmarkParser` convierte, y `HandleLandmarks` calcula ángulos. El problema es el último eslabón: aplicar esos ángulos al DOM.

Actualmente se hace vía `InvokeAsync(StateHasChanged)` que encadena un render completo de Blazor WASM en el thread de UI. A 30 FPS esto satura el renderer (~50ms por render en WASM), la cola crece y los frames nunca se pintan. El esqueleto verde sí funciona porque usa `_js.InvokeVoidAsync("avatarCamera.drawSkeleton")` — JS directo al canvas, sin Blazor.

## Goals / Non-Goals

**Goals:**
- Aplicar CSS transforms de brazos, torso y cabeza directamente desde JS, saltando el render de Blazor
- Mantener los ángulos calculados en C# (Atan2, Lerp, mirror) — solo cambia cómo llegan al DOM
- Mantener personalización (shirt, hair, hat, glasses) en Blazor — no son tiempo real
- Sincronizar el frame rate del avatar con el del esqueleto verde
- Reactivar `transition: transform` en CSS para suavizado visual gratuito

**Non-Goals:**
- Modificar el servidor Python
- Modificar el pipeline de datos (TrackingService, LandmarkParser)
- Reescribir el cálculo de ángulos en JavaScript
- Modificar el modo demo (sigue usando Blazor para los sliders)
- Migrar las expresiones faciales a JS (la frecuencia de cambio es baja, Blazor alcanza)

## Decisions

### Decision 1: Nuevo módulo JS `avatarAnim.js`

Un módulo liviano (~50 líneas) expuesto como `window.avatarAnim` que recibe un objeto con todos los ángulos y aplica `element.style.transform` a cada div del avatar. La estructura:

```javascript
window.avatarAnim = {
    update: function(angles) {
        var bw = document.getElementById('body-wrapper');
        if (bw) bw.style.transform =
            'perspective(800px) rotateY(' + angles.torsoY + 'deg) ' +
            'rotateZ(' + angles.torsoZ + 'deg) scale(2)';

        var al = document.getElementById('arm-left');
        if (al) al.style.transform = 'rotate(' + angles.leftShoulder + 'deg)';

        var ar = document.getElementById('arm-right');
        if (ar) ar.style.transform = 'rotate(' + angles.rightShoulder + 'deg)';

        var fl = document.getElementById('forearm-left');
        if (fl) fl.style.transform = 'rotate(' + angles.leftElbow + 'deg)';

        var fr = document.getElementById('forearm-right');
        if (fr) fr.style.transform = 'rotate(' + angles.rightElbow + 'deg)';

        var h = document.getElementById('head');
        if (h) h.style.transform = 'rotate(' + angles.headTilt + 'deg)';
    },

    resetToNeutral: function() {
        this.update({
            torsoY: 0, torsoZ: 0,
            leftShoulder: 0, leftElbow: 0,
            rightShoulder: 0, rightElbow: 0,
            headTilt: 0
        });
    }
};
```

**¿Por qué getElementById y no un framework?** Porque son 6 elementos fijos y no necesitamos diffing ni reactividad. El `id` ya existe en los elementos (`id="arm-left"`, `id="forearm-left"`, etc.). Solo se agrega `id="body-wrapper"` (ya existe) y `id="head"` (ya existe).

### Decision 2: Qué envía C# a JS

`HandleLandmarks` calcula los ángulos exactamente igual que ahora. La única diferencia: en vez de `InvokeAsync(StateHasChanged)`, llama a:

```csharp
_ = _js.InvokeVoidAsync("avatarAnim.update", new
{
    leftShoulder = LeftShoulderAngle,
    leftElbow = LeftElbowAngle,
    rightShoulder = RightShoulderAngle,
    rightElbow = RightElbowAngle,
    torsoY = TorsoYAngle,
    torsoZ = TorsoZAngle,
    headTilt = _headTiltZ
});
```

El `new {}` anónimo se serializa a JSON automáticamente por `IJSRuntime`. Es un objeto plano de 7 doubles — ~100 bytes, insignificante.

También se agrega `_js` como dependencia en `AvatarRenderer` (ya está en DI como `IJSRuntime`).

### Decision 3: Separar posicionamiento estático del transform dinámico

Actualmente los elementos tienen `style` inline que mezcla posicionamiento estático (que Blazor debería manejar) con `transform: rotate()` (que JS debería manejar). Hay que separarlos para que Blazor y JS no se pisen.

**Brazo (`AvatarArm.razor`):**
```html
<!-- Antes -->
<div id="arm-left" class="arm-container left"
     style="position:absolute;top:20px;z-index:15;transform:rotate(45deg)">

<!-- Después -->
<div id="arm-left" class="arm-container left arm-static-pos">
```

**Torso (`AvatarRenderer.razor`):**
```html
<!-- Antes -->
<div id="body-wrapper" class="body-wrapper" style="@TorsoTransform">

<!-- Después -->
<div id="body-wrapper" class="body-wrapper">
```

**CSS nuevo:**
```css
.arm-static-pos {
    position: absolute;
    top: 20px;
    z-index: 15;
}
```

El `TorsoTransform` property se elimina completamente de `AvatarRenderer.razor`.

### Decision 4: Reactivar CSS transitions

Como JS ahora es dueño exclusivo del `transform`, podemos reactivar las transiciones CSS que quitamos en `harden-tracking-pipeline`:

```css
.arm-container { transition: transform 0.05s ease-out; }
.arm-lower    { transition: transform 0.05s ease-out; }
.body-wrapper { transition: transform 0.1s ease-out; }
.head         { transition: transform 0.05s ease-out; }
```

Con 0.05s en vez de 0.1s, el suavizado es más rápido (menos "persecución") pero suficiente para evitar saltos. El `Lerp(0.3)` de C# ya no es necesario — podemos bajarlo a `Lerp(0.6)` o incluso quitarlo, porque la transición CSS ahora es el único suavizado.

### Decision 5: Expresiones faciales se quedan en Blazor

Las expresiones (`smile`, `surprise`, `angry`) cambian a baja frecuencia (~1 cambio cada varios segundos). No se benefician del bypass de Blazor. `ExpressionClass` sigue pasándose como parámetro Blazor a `AvatarHead`.

### Decision 6: Modo demo sigue usando Blazor

Cuando `TrackingService.IsConnected` es false, `HandleAvatarStateChange` se encarga de setear los ángulos y llamar `InvokeAsync`. Pero ahora `InvokeAsync` no aplica transforms (JS lo hace). Hay dos opciones:

**Opción A (elegida):** `HandleAvatarStateChange` también llama a `avatarAnim.update()` con los ángulos demo. Así el camino es uniforme: siempre JS es dueño del `transform`, C# calcula y le pasa los ángulos.

```csharp
private void HandleAvatarStateChange()
{
    if (!TrackingService.IsConnected)
    {
        // ... setear ángulos demo ...
        _ = _js.InvokeVoidAsync("avatarAnim.update", new { ... });
    }
    // Ya no llama a InvokeAsync(StateHasChanged) para los transforms.
    // Solo llama InvokeAsync si cambió la personalización (expression, ropa).
}
```

Esto también resuelve el bug de los sliders demo: al mover un slider, `HandleAvatarStateChange` llama a `avatarAnim.update()` que aplica el transform inmediatamente.

## Risks / Trade-offs

- **[Riesgo bajo] Elemento no encontrado en el DOM**: `getElementById` retorna null si el elemento no existe. El código JS verifica con `if (el)` antes de asignar `style.transform`. Si un elemento falta, simplemente no se anima — sin crash.
- **[Riesgo bajo] C# y JS desincronizados**: Si JS aplica un transform y luego Blazor re-renderiza el componente padre, Blazor podría pisar el `style` de JS. Para evitarlo, nos aseguramos de que el template Razor no tenga `style` inline en los elementos animados.
- **[Riesgo bajo] `_js` puede ser null en Blazor SSR**: `IJSRuntime` está disponible en WASM pero no en pre-rendering. `AvatarRenderer` corre solo en WASM (es un componente interactivo), así que `_js` siempre está disponible. De todas formas se verifica `_js != null` antes de invocar.
- **[Trade-off] Dos fuentes de verdad para los ángulos**: Los campos de `AvatarRenderer` (LeftShoulderAngle, etc.) existen en C# pero JS los aplica. Esto es aceptable porque C# sigue necesitando los campos para: (a) el modo demo vía sliders, (b) logging/telemetría, (c) posibles features futuras (grabación, replay).
