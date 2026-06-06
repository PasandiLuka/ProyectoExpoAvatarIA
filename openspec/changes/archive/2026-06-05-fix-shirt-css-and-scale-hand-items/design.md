## Context

El CSS de las remeras de Alemania y España no refleja correctamente las banderas. Los items de mano nuevos son proporcionalmente chicos comparados con el avatar (torso 180×250, items 20-50px). El sable láser y la guitarra deberían ser elementos destacados.

Los Pokémon no aparecen como opciones porque `HandItems` no incluye entradas para ellos. El sistema de renderizado condicional en `AvatarArm.razor` (`StartsWith("pokemon-")`) ya está implementado, solo faltan los botones en el panel.

## Goals / Non-Goals

**Goals:**
- Alemania: franjas horizontales negro (top), rojo (centro), dorado (#ffcc00) (bottom)
- España: franjas horizontales rojo (top), amarillo (centro, doble altura), rojo (bottom)
- Items de mano: dimensiones ×1.5 para todos
- Sable láser: 15×220px con glow azul
- Guitarra: mantener tamaño actual (150×250) que ya es correcto
- Pokémon: 6 opciones predefinidas en el panel

**Non-Goals:**
- Auto-detección de imágenes Pokémon (WASM no puede escanear filesystem)
- Modificar el HTML de `AvatarArm.razor` (ya soporta Pokémon)

## Decisions

### Decision 1: Alemania — bandera correcta con linear-gradient

```css
.torso.shirt-alemania { 
    background: linear-gradient(to bottom, #000 33%, #dd0000 33%, #dd0000 66%, #ffcc00 66%); 
}
```

Tres franjas horizontales iguales: negro, rojo, dorado.

### Decision 2: España — franjas horizontales

```css
.torso.shirt-espana { 
    background: linear-gradient(to bottom, #c60b1e 25%, #ffc400 25%, #ffc400 75%, #c60b1e 75%); 
}
```

La franja amarilla central ocupa el doble que las rojas (proporción real de la bandera).

### Decision 3: Escala ×1.5 para todos los items

Cada dimensión de los 16 items nuevos se multiplica por 1.5. Ejemplos:
- Pelota: 45×45 → 68×68
- Laptop: 50×35 → 75×53
- Microfono: 18×40 → 27×60

### Decision 4: Sable láser tamaño torso

```css
.item-sable { width: 15px; height: 220px; background: #00f; border-radius: 8px; box-shadow: 0 0 10px #0ff, 0 0 20px #00f, 0 0 40px #008; }
.item-sable::after { bottom: -15px; left: -6px; width: 27px; height: 20px; }
```

Hoja de 220px (torso es 250px) con glow multicapa.

### Decision 5: Pokémon — usar la carpeta `wwwroot/imgs/Pokemon/`

Las imágenes reales están en `wwwroot/imgs/Pokemon/` (no en `images/pokemon/`). Se actualiza el path en `AvatarArm.razor` y se agregan los 8 nombres al array `PokemonItems`: treecko, eevee, rowlet, lapras, zapdos, nidoqueen, piplup, squirtle.

## Risks / Trade-offs

- **[Riesgo bajo] Escala ×1.5**: Items más grandes pueden solaparse entre sí (solo uno está activo a la vez). Sin impacto.
- **[Riesgo bajo] Pokémon sin imagen**: Si el PNG no existe, el `<img>` muestra el alt text. El usuario ve un espacio vacío. Se mitiga poniendo nombres claros en el array.
