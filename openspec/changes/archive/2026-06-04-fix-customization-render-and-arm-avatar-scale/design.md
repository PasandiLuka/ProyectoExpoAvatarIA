## Context

El avatar se renderiza mediante 4 componentes Blazor anidados (`AvatarRenderer` → `AvatarTorso` → `AvatarHead`, y `AvatarArm`×2 en paralelo). Actualmente, los datos de personalización (pelo, remera, gorros, anteojos, objetos de mano) son leídos por los componentes hijos directamente desde `@inject AvatarState`, mientras que los datos cinemáticos (ángulos, expresión) fluyen como `[Parameter]` desde `AvatarRenderer`.

Esto crea un desacople en el mecanismo de re-render: Blazor solo re-renderiza componentes hijos cuando sus `[Parameter]` cambian (optimización `ShouldRender`). Como los parámetros cinemáticos no cambian al modificar solo la personalización, los hijos nunca se re-renderizan, y los cambios de `AvatarState` leídos vía `@inject` nunca se aplican al DOM.

Además, el segmento superior del brazo (`arm-upper`) tiene `background: inherit` que hereda de un ancestro sin background definido, resultando en transparencia. Y el avatar ocupa solo ~15% del panel central (180px en un panel de ~1152px).

## Goals / Non-Goals

**Goals:**
- Que los cambios de personalización se reflejen inmediatamente en el avatar sin depender de interacciones no relacionadas (expresión, sliders)
- Que el brazo superior tenga color visible (piel por defecto, manga larga cuando corresponda)
- Que el avatar ocupe más espacio visual (~2× mediante `scale(2)`)

**Non-Goals:**
- Modificar el protocolo WebSocket o el servidor Python
- Cambiar la arquitectura de servicios (AvatarState sigue siendo Singleton)
- Agregar nuevas opciones de personalización
- Hacer el avatar responsive (se usa escala fija)

## Decisions

### Decisión 1: Pasar datos de personalización como `[Parameter]` en vez de `@inject`

**Alternativas consideradas:**

| Opción | Pros | Contras |
|--------|------|---------|
| A. Parámetros explícitos | Flujo de datos unidireccional, Blazor detecta cambios, idiomático | Más código boilerplate en AvatarRenderer |
| B. `ShouldRender() => true` en cada hijo | Mínimo cambio de código | Re-render innecesario en cada frame de tracking, desperdicia CPU |
| C. `@key` en componentes hijos | Fuerza recreación de componente | Performance pésima, flickering visual |
| D. `StateHasChanged()` manual en cada hijo | Control granular | Acoplamiento, difícil de mantener |

**Decisión:** Opción A — Parámetros explícitos. Es el patrón canónico de Blazor: los datos fluyen hacia abajo como parámetros, los eventos fluyen hacia arriba. `@inject` se reserva para servicios (lógica), no para datos de renderizado.

**Arquitectura resultante:**

```
AvatarState (Singleton, escritura + OnChange)
       │
       ▼
AvatarRenderer (lee TODO de AvatarState, pasa como [Parameter])
       │
  ┌────┼──────────────┐
  ▼    ▼              ▼
Torso  Arm(L)        Arm(R)
  │
  ▼
Head (recibe params: HairStyle, HairColor, HatStyle, GlassesStyle)
```

### Decisión 2: Usar `scale(2)` en CSS transform para agrandar el avatar

**Alternativas consideradas:**

| Opción | Pros | Contras |
|--------|------|---------|
| A. `transform: scale()` en cadena existente | Mínimo cambio (1 línea), mantiene proporciones | Puede causar clipping si el contenedor es chico |
| B. Reescribir todos los px en CSS (2×) | Control preciso, sin clipping | ~30 valores a cambiar, frágil, propenso a errores |
| C. Usar `vw`/`vh` en vez de px | Responsive | Requiere rediseño completo del CSS |

**Decisión:** Opción A. Agregar `scale(2)` al final de la cadena de transform existente en `TorsoTransform` y setear `transform-origin: center bottom` en `.body-wrapper`. El contenedor `.panel-central` tiene altura del 60% del viewport (~648px en 1080p), suficiente para un avatar escalado a 500px. El ancho del panel (~1152px) excede ampliamente los 360px del avatar escalado.

### Decisión 3: Color de brazo superior con CSS por tipo de remera

**Decisión:** Eliminar `style="background: inherit;"` del markup. Usar CSS puro con el selector de hermano general `~` ya existente:

```css
.arm-upper { background-color: var(--skin-color); }                    /* default: piel */
.torso.shirt-long ~ .arm-container .arm-upper { background-color: #2b2d42; }  /* manga larga */
.torso.shirt-river ~ .arm-container .arm-upper { background-color: #ffffff; }  /* River */
```

## Risks / Trade-offs

- **[Riesgo] `scale(2)` puede causar que el avatar sobresalga del panel en viewports pequeños** → Mitigación: El panel central tiene 60% del viewport; en 720p son 432px de alto, suficiente para 500px del avatar (con padding-bottom de 80px, el total es ~580px, que excede). Se puede ajustar a `scale(1.5)` si es necesario.
- **[Riesgo] Pasar 6+ parámetros nuevos hace el markup de AvatarRenderer más verboso** → Mitigación: Es aceptable; la claridad del flujo de datos justifica la verbosidad.
- **[Trade-off] `AvatarHead` pierde su `@inject AvatarState` y ahora depende totalmente de parámetros** → Esto es deseable: hace el componente más testeable y predecible.

## Open Questions

- ¿`scale(2)` o `scale(1.5)`? Depende de la preferencia visual. Se implementará `scale(2)` inicialmente y se ajustará según feedback.
