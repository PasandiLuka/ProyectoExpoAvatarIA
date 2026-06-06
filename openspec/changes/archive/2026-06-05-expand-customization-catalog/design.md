## Context

El `CustomizationPanel` actual muestra 6 categorías en una sola columna con scroll. Cada una tiene 3-4 botones. Al expandir a 12 remeras, 10 gorros, 7 anteojos y 20 items por mano, el panel sería inusable sin reorganizar la UI.

El sistema de personalización usa `AvatarState` con propiedades `string` (ej: `ShirtStyle = "shirt-argentina"`) que se mapean a clases CSS en los componentes del avatar. Este patrón escala trivialmente: agregar items nuevos es solo agregar botones + CSS.

## Goals / Non-Goals

**Goals:**
- UI Accordion: colapsar categorías, solo una abierta a la vez, scroll interno en categorías grandes (items de mano)
- Expandir catálogo: 12 remeras, 10 gorros, 7 anteojos, 20 items por mano
- Temática Mundial 2026 + Computación integrada en los diseños
- Pokémon como hand items renderizados con `<img>`, mismo tamaño que items CSS
- Mantener el sistema existente (`AvatarState` string properties + CSS classes) sin cambios estructurales

**Non-Goals:**
- Modificar `AvatarState.cs` (las propiedades string ya soportan cualquier valor)
- Agregar animaciones de transición al cambiar items
- Persistir selecciones entre sesiones
- Modificar el servidor Python
- Soporte para personalización de color de las remeras/gorros nuevos (usan CSS fijo)

## Decisions

### Decision 1: Accordion CSS-only con toggle booleano en Blazor

Cada categoría es un `<div>` con un botón header que togglea `bool _catOpen_<name>`. El contenido usa `max-height` + `overflow` CSS para colapsar/expandir. Cuando se abre una categoría, se cierran las demás seteando todos los flags a false menos el actual.

```razor
@code {
    private int _openCategory = -1; // índice de la categoría abierta
}
```

Ventajas: sin dependencia JS, animable con CSS transition, natural en Blazor. Las categorías con muchos items (mano: 20 items) tienen `max-height: 300px; overflow-y: auto` con scroll interno.

### Decision 2: Remeras — CSS backgrounds con clases

Cada remera es una clase CSS en `.torso`:
- `.shirt-argentina` — franjas celeste/blanco con `linear-gradient`
- `.shirt-brasil` — amarillo con detalles verdes
- `.shirt-espana` — rojo sólido
- `.shirt-alemania` — blanco con franja negra
- `.shirt-csharp` — fondo `#68217a` (púrpura oficial de C#) con "C#" en blanco via `::after`
- `.shirt-python` — azul `#306998` con amarillo `#ffd43b`
- `.shirt-javascript` — amarillo `#f7df1e` con texto "JS" negro via `::after`
- `.shirt-matrix` — verde `#00ff41` sobre negro, caracteres "01" via `::after`

Las remeras existentes (short, long, boca, river) se mantienen. Total: 12.

### Decision 3: Gorros — CSS shapes + pseudo-elements

Cada gorro es un div posicionado sobre la cabeza con shapes CSS:
- `hat-gorra-argentina` — gorra celeste/blanco
- `hat-vikingo` — cuernos via `::before` + `::after` con `border` tricks
- `hat-sombrero-mexicano` — ala ancha con `border-radius`
- `hat-casco-espacial` — esfera con visor oscuro
- `hat-diadema-vr` — banda con lentes pequeños
- `hat-corona` — triángulos dorados via `clip-path`

### Decision 4: Anteojos — CSS shapes

Se elimina `glasses-2026` (el texto "2026" quedaba raro). Reemplazos:
- `glasses-nerd` — frames negros gruesos con `border`
- `glasses-futurista` — visor único estilo cyberpunk con `linear-gradient`
- `glasses-3d` — lente rojo + lente azul
- `glasses-monoculo` — un solo círculo con cadena

### Decision 5: Hand items — CSS puro para items temáticos

Los 16 items nuevos se implementan como CSS puro (divs + pseudo-elements), mismo patrón que fernet/water/cup existentes. Dimensiones: ~40-60px ancho × 80-120px alto en su espacio local.

Lista completa de los 16 nuevos:
| ID | Nombre | CSS Key |
|----|--------|---------|
| mate | Mate | `item-mate` |
| pelota | Pelota de Fútbol | `item-pelota` |
| bandera | Bandera Argentina | `item-bandera` |
| trofeo | Trofeo Mundial | `item-trofeo` |
| laptop | Laptop | `item-laptop` |
| teclado | Teclado | `item-teclado` |
| mouse | Mouse | `item-mouse` |
| microfono | Micrófono | `item-microfono` |
| guitarra | Guitarra | `item-guitarra` |
| sable | Espada Láser | `item-sable` |
| celular | Celular | `item-celular` |
| libro | Libro | `item-libro` |
| pizza | Pizza | `item-pizza` |
| hamburguesa | Hamburguesa | `item-hamburguesa` |
| pochoclos | Pochoclos | `item-pochoclos` |
| pokebola | Pokébola | `item-pokebola` |

### Decision 6: Pokémon images — `<img>` con bounding box estándar

Para los Pokémon, el usuario proveerá imágenes (PNG/SVG). Se crea un item genérico `item-pokemon` que contiene un `<img>` tag con `src` seteado desde el path de la imagen. El CSS fuerza el tamaño:

```css
.item-pokemon img {
    width: 50px;
    height: 50px;
    object-fit: contain;
}
```

Las imágenes se colocan en `wwwroot/images/pokemon/`. El nombre del archivo determina el ID del item (ej: `pikachu.png` → `item-pokemon-pikachu`). Cada Pokémon es un botón separado en la categoría de mano.

### Decision 7: Lista unificada de items de mano con checkboxes L/R

En vez de duplicar la lista de 20 items en dos secciones (Mano Izquierda / Mano Derecha), se usa una sola lista. Cada item tiene dos botones chicos: **I** (izquierda) y **D** (derecha). Clickear I asigna el item a la mano izquierda, clickear D a la derecha. Ambos pueden tener el mismo item o items distintos.

```html
<div class="hand-item-row">
    <span class="hand-item-name">Mate</span>
    <button class="hand-btn @(LeftHandItem == "mate" ? "active" : "")" 
            @onclick="() => SetLeftHand("mate")">I</button>
    <button class="hand-btn @(RightHandItem == "mate" ? "active" : "")" 
            @onclick="() => SetRightHand("mate")">D</button>
</div>
```

Esto reduce a la mitad los botones y elimina la redundancia visual. Los botones I/D activos se resaltan con el color del tema.

Ventajas:
- Panel más compacto (20 items × 1 fila en vez de 20 × 2 columnas)
- No hay duplicación de código ni riesgo de desincronización
- El usuario ve de un vistazo qué tiene en cada mano

## Risks / Trade-offs

- **[Riesgo bajo] Panel saturado**: La categoría de mano tiene 20+ items. Se mitiga con `overflow-y: auto` y `max-height: 300px` — scroll dentro de la categoría.
- **[Riesgo bajo] Pokémon sin imágenes**: Si las imágenes no están en el path esperado, el `<img>` muestra alt text o espacio vacío. Se mitiga con `alt` descriptivo.
- **[Riesgo medio] Complejidad del CSS**: ~80 líneas nuevas de CSS con pseudo-elements. Mantener consistencia visual requiere testear cada item. Se prioriza simplicidad sobre realismo.
- **[Riesgo bajo] Accordion sin JS**: El toggle booleano en Blazor funciona perfecto con WASM. No hay riesgo de estado inconsistente ya que el estado vive en C#.
