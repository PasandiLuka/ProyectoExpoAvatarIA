## Context

La escala del avatar actualmente es 100% dinámica: `avatarAnim.update()` calcula `Math.min(cw/180, ch/400) * 0.85` cada frame. No hay forma de que el usuario ajuste manualmente el tamaño.

Se agrega un override manual con prioridad sobre el cálculo automático. El slider se ubica en la sección inferior del `CameraPanel`, colapsado detrás de un botón hamburguesa para no ocupar espacio permanente.

## Goals / Non-Goals

**Goals:**
- Slider 0.5–3 para control manual del scale del avatar
- Botón "Auto" para reactivar el cálculo dinámico
- Menú colapsable (hamburguesa) que no ocupe espacio cuando no se usa
- El escalado es proporcional (afecta todo el body-wrapper, no partes individuales)

**Non-Goals:**
- Escalar partes individuales (brazos, cabeza por separado)
- Persistir el valor entre sesiones
- Animación de transición al cambiar el scale

## Decisions

### Decision 1: `manualScale` en `avatarAnim.js` como override

```javascript
window.avatarAnim = {
    manualScale: null,  // null = automático

    setManualScale: function (value) {
        this.manualScale = value;
    },
    
    update: function (angles) {
        var scale = this.manualScale;
        if (scale == null) {
            // cálculo dinámico existente
        }
        // aplicar scale...
    }
};
```

`null` = modo automático (dinámico). Cualquier número = override manual. Simple, sin estados complejos.

### Decision 2: Menú hamburguesa en `CameraPanel`

Estructura HTML:
```html
<div class="scale-menu">
    <button class="scale-menu-toggle" @onclick="ToggleScaleMenu">≡ Tamaño</button>
    <div class="scale-menu-content @(ScaleMenuOpen ? "open" : "")">
        <input type="range" min="0.5" max="3" step="0.1" 
               @oninput="OnScaleInput" value="@CurrentScale" />
        <button @onclick="SetAutoScale">Auto</button>
    </div>
</div>
```

La hamburguesa usa CSS `max-height` + `overflow: hidden` para el collapse animado, sin JS.

### Decision 3: Slider usa JS interop directo

`CameraPanel` ya tiene `@inject IJSRuntime JS`. Al mover el slider, llama:
```csharp
await JS.InvokeVoidAsync("avatarAnim.setManualScale", value);
```

El valor se lee del `oninput` del range y se convierte a double. "Auto" llama con `null` explícito (o con un valor sentinel como -1 que JS interpreta como null).

**Alternativa considerada**: Pasar el valor por `AvatarState`. Descartada: agrega un ciclo innecesario C# → Blazor → JS cuando podemos ir directo C# → JS.

## Risks / Trade-offs

- **[Riesgo bajo] Slider 0.5 mínimo**: A 0.5 el avatar es muy chico pero visible. No se rompe.
- **[Riesgo bajo] Slider 3 máximo**: A escala 3 en pantalla chica el avatar se recorta (overflow: hidden). Aceptable — el usuario eligió ese tamaño.
- **[Riesgo bajo] JS interop por cada movimiento del slider**: `oninput` dispara ~60 veces/segundo al arrastrar. Cada llamada JS interop son ~0.1ms. Sin impacto en performance.
