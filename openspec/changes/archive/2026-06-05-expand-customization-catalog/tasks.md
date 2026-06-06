## 1. Accordion UI en CustomizationPanel

- [x] 1.1 Refactorizar HTML: reemplazar divs planos por estructura accordion (header button + content div)
- [x] 1.2 Agregar `@code`: campo `_openCat` (int), método `ToggleCategory(int index)` que cierra las demás
- [x] 1.3 Agregar CSS accordion en `app.css`: `.custom-category-content` con `max-height` + `overflow` + `transition`
- [x] 1.4 Agregar scroll interno para categorías de mano: `max-height: 300px; overflow-y: auto`

## 2. Remeras (8 nuevas)

- [x] 2.1 Agregar botones de las 8 remeras nuevas en la categoría accordion
- [x] 2.2 Agregar handlers C#: `SetShirtArgentina()`, `SetShirtBrasil()`, `SetShirtEspana()`, `SetShirtAlemania()`, `SetShirtCSharp()`, `SetShirtPython()`, `SetShirtJs()`, `SetShirtMatrix()`
- [x] 2.3 Agregar CSS en `avatar.css` para las 8 clases `.torso.shirt-*`

## 3. Gorros (6 nuevos)

- [x] 3.1 Agregar botones de los 6 gorros nuevos en categoría accordion
- [x] 3.2 Agregar handlers C#: `SetHatGorraArg()`, `SetHatViking()`, `SetHatMexicano()`, `SetHatEspacial()`, `SetHatVR()`, `SetHatCorona()`
- [x] 3.3 Agregar divs HTML en `AvatarHead.razor` para los 6 gorros nuevos
- [x] 3.4 Agregar CSS en `avatar.css` para las 6 clases `.hat-*`

## 4. Anteojos (4 nuevos, eliminar 2026)

- [x] 4.1 Eliminar botón y handler de "2026" (`SetGlasses2026`)
- [x] 4.2 Agregar botones de los 4 anteojos nuevos en categoría accordion
- [x] 4.3 Agregar handlers C#: `SetGlassesNerd()`, `SetGlassesFuturista()`, `SetGlasses3D()`, `SetGlassesMonoculo()`
- [x] 4.4 Agregar divs HTML en `AvatarHead.razor` para los 4 anteojos nuevos
- [x] 4.5 Agregar CSS en `avatar.css` para las 4 clases `.glasses-*`

## 5. Items de mano (16 nuevos por mano)

- [x] 5.1 Agregar botones de los 16 items con lista unificada y checkboxes I/D
- [x] 5.2 Agregar handlers: `SetHand(side, item)` genérico con array `HandItems`
- [x] 5.3 Agregar divs HTML en `AvatarArm.razor` para los 20 items con clase `stay-upright`
- [x] 5.4 Agregar CSS en `avatar.css` para los 16 items + pokemon

## 6. Pokémon como hand items

- [x] 6.1 Crear carpeta `wwwroot/images/pokemon/` con placeholder `.gitkeep`
- [x] 6.2 Agregar estructura HTML condicional en `AvatarArm.razor`: detecta `handItem.StartsWith("pokemon-")`
- [x] 6.3 Agregar handlers: el sistema de `HandItems` soporta cualquier ID, los Pokémon se agregan al array
- [x] 6.4 Agregar CSS: `.item-pokemon img { width:50px; height:50px; object-fit:contain }`
- [x] 6.5 Agregar div `<div class="item item-pokemon stay-upright">` + `<img>` en `AvatarArm.razor`

## 7. Verificación

- [x] 7.1 Compilar `dotnet build` — 0 errores, 0 warnings
- [ ] 7.2 Probar accordion: abrir/cerrar categorías, solo una visible a la vez
- [ ] 7.3 Probar visualmente: cada remera, gorro, anteojos, item de mano se renderiza correctamente
- [ ] 7.4 Probar tracking: items `stay-upright` compensan rotación del brazo
- [ ] 7.5 Probar Pokémon: si hay imágenes en la carpeta, se muestran como items
