## Why

El catálogo de personalización actual (4 remeras, 4 gorros, 4 anteojos, 4 items por mano) es muy limitado para una experiencia atractiva. Con la temática Mundial 2026 y la especialidad en computación, hay una gran oportunidad de expandir el catálogo con items temáticos que enriquezcan la experiencia del avatar. Además, el panel de personalización actual muestra todas las categorías en simultáneo — al triplicar el catálogo, se vuelve inusable sin una reorganización de la UI.

## What Changes

- **UI Accordion**: Refactorizar `CustomizationPanel` con categorías colapsables. Solo una categoría visible a la vez. Cada categoría tiene botones en grid 2-3 columnas con scroll interno si excede altura máxima.
- **+8 Remeras**: Argentina, Brasil, España, Alemania, C#, Python, JavaScript, Matrix/Binario. Total: 12 remeras.
- **+6 Gorros**: Gorra Argentina, Vikingo, Sombrero Mexicano, Casco Espacial, Diadema VR, Corona. Total: 10 gorros.
- **+4 Anteojos** (reemplaza "2026"): Nerd, Futurista/Cyberpunk, 3D (rojo/azul), Monóculo. Total: 7 anteojos.
- **+16 Items por mano**: Mate, Pelota, Bandera Argentina, Trofeo Mundial, Laptop, Teclado, Mouse, Micrófono, Guitarra, Espada Láser, Celular, Libro, Pizza, Hamburguesa, Pochoclos, Pokébola. Total: 20 items por mano.
- **Pokémon como hand items**: El usuario proveerá imágenes. Se renderizan con `<img>` dentro de `.item`, mismo bounding box que los items CSS (~50x100px). Clase `stay-upright` para que compensen rotación del brazo.

## Capabilities

### New Capabilities

- `customization-catalog`: Define todos los items de personalización (IDs, nombres, CSS asociado) y el sistema de accordion para el panel.
- `pokemon-hand-items`: Sistema para cargar imágenes de Pokémon como items de mano, renderizadas dentro del bounding box estándar con soporte stay-upright.

### Modified Capabilities

Ninguno — esto es pura expansión de contenido y refactor de UI. Los specs existentes no cambian.

## Impact

- `Components/CustomizationPanel.razor` — refactor completo a accordion + ~100 nuevos handlers
- `wwwroot/css/avatar.css` — +80 líneas de CSS para nuevos items (remeras, gorros, anteojos, mano)
- `Components/AvatarArm.razor` — +32 `<div>` items nuevos + soporte para `<img>` (Pokémon)
- `Components/AvatarHead.razor` — nuevos anteojos (4 CSS classes)
- `Components/AvatarTorso.razor` — sin cambios (ShirtStyle se pasa como clase CSS)
- `Services/AvatarState.cs` — sin cambios (las propiedades string ya soportan cualquier valor)
- `wwwroot/css/app.css` — +20 líneas para estilos del accordion
