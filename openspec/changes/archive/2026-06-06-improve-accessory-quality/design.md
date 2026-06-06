## Context

El avatar se renderiza con divs HTML + CSS. Cada accesorio es un elemento DOM posicionado absolutamente dentro de la cabeza o la mano. La animación aplica `transform: rotate()` vía JavaScript (`avatarAnim.js`), y los items de mano usan la clase `stay-upright` para compensar la rotación del brazo (el JS calcula `rotate(-shoulderAngle - elbowAngle)`).

El sistema de personalización asigna strings a `AvatarState` (ej: `HatStyle = "hat-cowboy"`) que los componentes Razor mapean a clases CSS condicionales. Este mecanismo **no cambia** — los mismos IDs, los mismos handlers en `CustomizationPanel.razor`, el mismo `AvatarState`. Solo cambia el markup interno de los componentes y el CSS asociado.

La Copa Mundial (`.item-cup`) ya demuestra el approach SVG inline en `AvatarArm.razor:10-15`:
```html
<div class="item item-cup stay-upright @(handItem == "cup" ? "active" : "")">
    <svg viewBox="0 0 100 200" xmlns="http://www.w3.org/2000/svg">
        <path d="M 30,180 L 70,180 L 60,100 L 40,100 Z" fill="#b8860b"/>
        <circle cx="50" cy="70" r="30" fill="#ffd700"/>
        <path d="M 20,60 Q 50,150 80,60" fill="none" stroke="#b8860b" stroke-width="10"/>
    </svg>
</div>
```

Este patrón se extiende a todos los accesorios complejos: el SVG reemplaza los pseudo-elementos CSS dentro del mismo `<div class="item item-xxx stay-upright">`. El contenedor mantiene `position: absolute; bottom: 20px; z-index: 25; display: none/block`, heredado de la clase `.item`.

## Goals / Non-Goals

**Goals:**
- Reemplazar accesorios con formas complejas por SVG inline detallados (guitarra, fernet, laptop, celular, sable, micrófono, gorros complejos, anteojos complejos)
- Refinar accesorios que se mantienen en CSS con más capas, sombras, y detalles (mate, pelota, bandera, trofeo, teclado, mouse, libro, comida, pokebola, water, gorros simples, peinados)
- Que cada accesorio sea inmediatamente reconocible y tenga un nivel de detalle consistente
- Corregir la guitarra (actualmente invisible: no tiene CSS)
- Corregir alineación del sable láser (quitar `bottom: 50px` hardcodeado)
- Reemplazar `filter: drop-shadow()` por `box-shadow` en todo el avatar (mejor rendimiento, sin recortes)
- Mantener compatibilidad total con `stay-upright`, tracking, y el pipeline de personalización existente
- Los mismos IDs, clases, y handlers — `CustomizationPanel.razor` no se toca

**Non-Goals:**
- Agregar nuevos accesorios (mismo catálogo, mejor calidad)
- Modificar `AvatarState.cs` o `CustomizationPanel.razor`
- Agregar animaciones de transición al cambiar accesorios
- Usar imágenes externas (salvo los Pokémon existentes)
- Modificar el servidor Python
- Agregar 3D, WebGL, o canvas rendering

## Decisions

### Decision 1: Criterio para elegir SVG vs CSS

```
¿El objeto tiene curvas complejas o formas orgánicas?
  ├── SÍ → SVG inline
  │   Ej: guitarra (cuerpo curvado), cowboy (ala con curvatura),
  │       fernet (botella con curvas), sable (empuñadura detallada)
  │
  └── NO → CSS refinado
      Ej: pelota (esfera), teclado (rectángulo), pizza (círculo),
          pokebola (esfera bicolor), libro (rectángulo)
```

**Van a SVG:** Guitarra, Fernet, Laptop, Celular, Sable Láser (empuñadura), Micrófono, Copa Mundial (mejora), Vaquero, Vikingo, Mexicano, Espacial, VR, Corona, Sol, Nerd, Futurista, 3D

**Se quedan en CSS (refinado):** Chef, Cap, Gorra Argentina, Googly, Monóculo, Mate, Pelota, Bandera, Trofeo, Teclado, Mouse, Libro, Pizza, Hamburguesa, Pochoclos, Pokebola, Water

### Decision 2: Convención de viewBox para SVG

Todos los SVG usan `viewBox="0 0 100 100"` como estándar. Esto permite pensar en porcentajes y escalar uniformemente. El `<div>` contenedor define el tamaño visual con CSS (`width`/`height` en px), y el SVG se adapta vía `width: 100%; height: 100%`.

Excepción: items muy altos (sable, bandera, botellas) usan `viewBox="0 0 100 200"` para acomodar la proporción vertical.

```css
.item svg {
    width: 100%;
    height: 100%;
    display: block;
}
```

### Decision 3: Estructura de markup para SVG items

Cada item SVG mantiene el mismo contenedor externo que los items CSS:

```html
<div class="item item-xxx stay-upright @(handItem == "xxx" ? "active" : "")">
    <svg viewBox="0 0 100 100">...</svg>
</div>
```

El CSS de `.item` (position, z-index, display control) se aplica al div wrapper y no cambia. El SVG se estila con `width: 100%; height: 100%` dentro del div.

### Decision 4: Gorros — SVG inline para los complejos

| GORRO | APPROACH | DETALLES CLAVE |
|-------|----------|----------------|
| Vaquero | SVG | Ala curvada con `d="M..."`, copa con hendidura, banda de cuero, sombra bajo ala |
| Chef | CSS ref. | Píldora blanca + `::before` con `radial-gradient` para pliegues + `::after` con borde inferior |
| Cap | CSS ref. | Semicírculo con gradiente para volumen, visor con `border-radius` asimétrico y sombra |
| Gorra Arg | CSS ref. | Misma forma que Cap, gradiente celeste/blanco/sol |
| Vikingo | SVG | Casco metálico con banda y remaches, cuernos curvados con `path`, gradiente metálico |
| Mexicano | SVG | Ala ancha elíptica, copa cónica, banda decorativa con patrón, borla colgante |
| Espacial | SVG | Esfera con `radialGradient` para reflejo especular, visor oscuro con brillo, anillo del cuello |
| VR | SVG | Banda con sensores, dos visores rectangulares con brillo azul, cable sugiriendo conexión |
| Corona | SVG | Puntas con `polygon`, gradiente dorado, gemas (círculos rojos/azules), base con patrón |

### Decision 5: Anteojos — SVG inline para los complejos

| ANTEOJOS | APPROACH | DETALLES CLAVE |
|----------|----------|----------------|
| Sol | SVG | Marco Wayfarer negro, lentes con gradiente oscuro + reflejo blanco diagonal, puente, patillas |
| Googly | CSS ref. | Círculos blancos más grandes, pupilas con `::after` animables, borde negro |
| Nerd | SVG | Marco rectangular grueso negro, lentes con leve tinte, puente doble, destello en lente |
| Futurista | SVG | Visor único curveado, gradiente cyan→magenta, líneas HUD en el visor, marco metálico |
| 3D | SVG | Marco de cartón (marrón), lente rojo izq + azul der, puente plegado |
| Monóculo | CSS ref. | Aro dorado más detallado, cadena con eslabones vía `repeating-linear-gradient`, reflejo |

### Decision 6: Items de mano — prioridad a los peores

Los items que van a SVG son los que actualmente son irreconocibles o invisibles:

| ITEM | ESTADO ACTUAL | APPROACH | DETALLES SVG |
|------|---------------|----------|--------------|
| Guitarra | **INVISIBLE** | SVG | Cuerpo con curva de guitarra acústica (8-shaped), boca circular, 6 cuerdas, mástil con trastes, clavijero |
| Fernet | Rectángulo negro | SVG | Botella con curvas, etiqueta con texto "FERNET", tapa roja, nivel de líquido oscuro |
| Laptop | Rectángulo gris | SVG | Pantalla con bisel, teclado con grid de teclas, trackpad, logo en la tapa |
| Celular | Rectángulo negro | SVG | Marco redondeado, pantalla con íconos, cámara frontal, botón home/sensor |
| Sable Láser | OK pero desalineado | SVG (empuñadura) | Empuñadura cilíndrica con emitter ring, grip texturizado, pommel; hoja con gradiente + glow |
| Micrófono | Aceptable | SVG | Cuerpo con líneas de grip, grille con patrón de puntos, anillo, conector XLR abajo |
| Copa Mundial | SVG (mejorable) | SVG ref. | Copa más proporcionada, base más ancha, asas curvas, brillo dorado, detalles del trofeo |

Items que se refinan en CSS:

| ITEM | MEJORA CSS |
|------|-----------|
| Water | Bordes redondeados, tapa azul, nivel de agua con gradiente, gotas de condensación |
| Mate | Ya es bueno. Agregar textura a la calabaza y brillo metálico a la bombilla |
| Pelota | Agregar líneas de paneles hexagonales vía `background` con múltiples gradientes |
| Bandera | Agregar adorno dorado en la punta del mástil, mejorar proporción de la bandera |
| Trofeo | Mejorar `clip-path` para copa más estilizada, agregar asas, gradiente metálico |
| Teclado | Mejorar grid de teclas con `repeating-linear-gradient` más fino, agregar borde metálico |
| Mouse | Agregar rueda scroll, botones izquierdo/derecho diferenciados, curvatura ergonómica |
| Libro | Agregar lomo con líneas, separación de páginas, cinta marcadora colgante |
| Pizza | Agregar borde de masa (anillo marrón), pepperonis mejor distribuidos, queso con sombra |
| Hamburguesa | Agregar semillas de sésamo en el pan, lechuga con borde ondulado, queso derritiéndose |
| Pochoclos | Agregar popcorn individuales como círculos blancos sobre el contenedor de rayas |
| Pokebola | Agregar brillo especular, botón central con anillo metálico, línea divisoria |

### Decision 7: Peinados — 9 estilos claramente distinguibles

Cada estilo de pelo debe ser inconfundible visualmente:

| ESTILO | DESCRIPCIÓN VISUAL |
|--------|-------------------|
| 1 (default) | Pelo corto clásico, arco suave sobre la frente |
| 2 (flequillo asimétrico) | Barrido hacia un lado, más largo, bordes angulares |
| 3 (corto prolijo) | Bajo y rectangular, línea recta |
| 4 (semilargo) | Cubre orejas, bordes redondeados, con volumen |
| 5 (pico/cresta) | Forma triangular en el centro, corto a los lados |
| 6 (afro/voluminoso) | Más alto y ancho que la cabeza, textura con bordes irregulares vía `box-shadow` |
| 7 (rapado/calvo) | Casi invisible — solo una sombra sutil en el cuero cabelludo |
| 8 (largo lacio) | Cubre hasta los hombros, laterales caídos, raya al medio |
| 9 (cola/recogido) | Pelo tirado hacia atrás, volumen atrás, frente despejada |

Cada estilo usa:
- `clip-path` para la silueta base
- `background: linear-gradient(...)` para volumen
- `box-shadow` para profundidad y textura
- `border-radius` para bordes suaves

### Decision 8: Remeras — detalles adicionales

Las camisetas de fútbol reciben:
- Número grande en el centro (`::after` con `content`, `font-size`, `font-weight`)
- Escudo simulado en el pecho (`::before` con formas geométricas pequeñas)
- Mejor gradiente para imitar tela

Las camisetas de programación reciben:
- Iconos/letras más grandes y centrados
- Fondo con textura sutil (patrón de puntos o líneas para simular tela)

### Decision 9: Eliminar `filter: drop-shadow()` → `box-shadow`

El avatar usa múltiples `filter: drop-shadow(0 0 2px black)` en `.torso`, `.head`, `.arm-upper`, `.arm-lower`, `.hand`. Esto causa:
- Bajo rendimiento (filter fuerza repaint completo)
- Recortes en algunos navegadores cuando el elemento rota
- Bordes borrosos en la captura con html2canvas

Se reemplazan todos por `box-shadow`:
```css
/* Antes */
.torso { filter: drop-shadow(0 0 2px black) drop-shadow(0 0 2px black); }

/* Después */
.torso { box-shadow: 0 0 2px black, 0 0 2px black; }
```

Esto da el mismo efecto visual (doble borde negro para el outline) con mejor rendimiento.

### Decision 10: Posicionamiento unificado de items de mano

Actualmente:
- `.item` tiene `bottom: 20px` como regla general
- `.item-sable` sobreescribe con `bottom: 50px`

Se elimina la sobreescritura del sable. Todos los items usan `bottom: 20px`. El SVG del sable se dibuja con la empuñadura en la parte inferior del viewBox y la hoja hacia arriba, compensando naturalmente la posición.

## Risks / Trade-offs

- **[Riesgo bajo] Aumento de markup en .razor**: Los SVG inline son más líneas que los divs CSS. `AvatarArm.razor` pasa de ~51 a ~200 líneas, `AvatarHead.razor` de ~52 a ~120. Se mitiga manteniendo los SVG limpios y bien formateados.
- **[Riesgo bajo] Performance de SVG**: Renderizar 2-4 SVG simultáneos (mano izq + der + cabeza) es trivial para el navegador. Los SVG son estáticos (no se animan vía SMIL), solo rotan con CSS transforms.
- **[Riesgo bajo] Compatibilidad con html2canvas**: html2canvas soporta SVG inline correctamente. La captura de pantalla no se ve afectada.
- **[Riesgo medio] Tiempo de implementación**: ~12-15 horas estimadas por la cantidad de SVG a diseñar (7 gorros, 5 anteojos, 7 items de mano). Se prioriza calidad sobre velocidad.
- **[Riesgo bajo] Regresiones visuales**: Al cambiar el markup de accesorios que ya funcionan, existe riesgo de desalineación. Se mitiga manteniendo los mismos IDs y estructura de contenedores.
