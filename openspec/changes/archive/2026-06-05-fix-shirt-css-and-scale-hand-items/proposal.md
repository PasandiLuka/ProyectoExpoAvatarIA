## Why

Tres bugs del catálogo expandido:

1. **Remeras Alemania y España**: Colores incorrectos. Alemania usa blanco en vez de la franja negra-roja-dorada real. España usa un cuadrado amarillo sobre rojo en vez de las franjas horizontales rojo-amarillo-rojo.
2. **Items de mano muy chicos**: Los 16 items nuevos son difíciles de ver. El sable láser y la guitarra deberían ser ~tamaño del torso para ser visibles y atractivos.
3. **Pokémon no seleccionables**: No hay botones en el panel para elegir Pokémon como items de mano. La carpeta `images/pokemon/` existe pero los nombres no están en la lista `HandItems`.

## What Changes

- **`avatar.css`**: Corregir colores de Alemania (negro-rojo-dorado) y España (rojo-amarillo-rojo). Escalar todos los items de mano ×1.5. Sable láser y guitarra al tamaño del torso (~250px).
- **`CustomizationPanel.razor`**: Agregar lista `PokemonItems` con 6 Pokémon predefinidos (pikachu, charmander, bulbasaur, squirtle, eevee, mew) como opciones seleccionables con I/D.

## Impact

- `wwwroot/css/avatar.css` — corregir 2 remeras + escalar 16 items
- `Components/CustomizationPanel.razor` — agregar array `PokemonItems` con 6 entradas
