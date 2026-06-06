## Why

Ampliar el catálogo de accesorios en 25 items (5 por categoría) más 6 remeras nuevas incluyendo la remera "67" (meme comunitario), México (Zapdos como águila), EE.UU., Canadá (hoja de maple), Corea del Sur e Inglaterra.

## What Changes

- **Peinados**: 5 nuevos estilos CSS (mohawk, trenzas, rulos, fade, japonés)
- **Remeras**: 6 nuevas (67, México, EE.UU., Canadá, Corea, Inglaterra) — CSS con pseudo-elementos
- **Gorros**: 5 nuevos (bruja SVG, galera CSS, beanie CSS, pirata SVG, orejas gato SVG)
- **Anteojos**: 5 nuevos (redondos CSS, corazón SVG, estrella SVG, cibernético SVG, diva CSS)
- **Items mano**: 5 nuevos (tambor SVG, bastón SVG, antorcha SVG, escudo CSS, flores CSS)

## Capabilities

### New Capabilities
- `accessories-expansion`: 31 nuevos accesorios distribuidos en peinados (5), remeras (6), gorros (5), anteojos (5), e items de mano (5)

## Impact

- `AvatarExpo/Components/CustomizationPanel.razor` — nuevas entradas en listas + setter methods
- `AvatarExpo/Components/AvatarHead.razor` — markup SVG/CSS para gorros y anteojos nuevos
- `AvatarExpo/Components/AvatarArm.razor` — markup SVG/CSS para items de mano nuevos
- `AvatarExpo/wwwroot/css/avatar.css` — CSS para peinados, remeras, gorros, anteojos e items nuevos
