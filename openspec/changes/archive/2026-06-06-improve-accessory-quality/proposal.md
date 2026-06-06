## Why

El catálogo actual de accesorios (10 gorros, 7 anteojos, 20+ items de mano, 9 peinados, 12 remeras) está implementado enteramente con CSS puro (divs + border-radius + pseudo-elements). Si bien esto es rápido y ligero, el resultado visual es pobre:

- Muchos objetos son irreconocibles o apenas se distinguen (guitarra invisible, casco espacial es un círculo translúcido, VR es una barrita negra, cowboy es dos óvalos)
- El nivel de detalle es inconsistente: el mate y la pokébola están bien logrados, pero la laptop, el celular, el fernet, y la guitarra son rectángulos genéricos
- Las copas/trofeos tienen formas extrañas y poco realistas
- Varios accesorios no se distinguen entre sí (peinados 6-9 son casi idénticos, anteojos de sol son solo rectángulos negros)
- La calidad visual no está a la altura de una exposición tecnológica

El proyecto ya demostró que los SVG inline funcionan (la Copa Mundial usa `<svg>` en `AvatarArm.razor`), pero este approach no se aplicó al resto del catálogo.

## What Changes

- **Refactor de gorros a SVG inline**: Vaquero, Vikingo, Mexicano, Espacial, VR, Corona reciben SVG detallados con curvas reales, sombras, y detalles. Chef, Cap, Gorra Argentina se refinan con CSS de mayor calidad.
- **Refactor de anteojos a SVG inline**: Sol, Nerd, Futurista, 3D reciben SVG con marcos, reflejos, y puentes nasales. Googly y Monóculo se refinan con CSS.
- **Refactor de items de mano a SVG inline**: Guitarra, Fernet, Laptop, Celular, Sable Láser, Micrófono, Copa Mundial reciben SVG detallados con formas realistas. El resto (mate, pelota, bandera, trofeo, teclado, mouse, libro, pizza, hamburguesa, pochoclos, pokebola, water) se refinan con CSS mejorado y pseudo-elementos adicionales.
- **Refactor de peinados**: Los 9 estilos de pelo se rediseñan para ser más distinguibles entre sí, usando clip-path más variados, sombras para volumen, y degradados sutiles.
- **Refactor de remeras**: Se agregan detalles a las camisetas de fútbol (números, escudos simulados con CSS) y a las de programación (íconos más grandes y reconocibles).
- **Corrección de la guitarra**: Actualmente no tiene CSS — se implementa como SVG inline con cuerpo de guitarra, boca, cuerdas y mástil.
- **Corrección de alineación**: El sable láser tiene `bottom: 50px` hardcodeado; se unifica el posicionamiento de todos los items de mano en `bottom: 20px` desde la clase `.item`.
- **Eliminación de `drop-shadow` por `box-shadow`**: La propiedad `filter: drop-shadow(0 0 2px black)` causa problemas de rendimiento y recorte en algunos navegadores; se reemplaza por `box-shadow` en todos los componentes del avatar.

## Capabilities

### New Capabilities

- `accessory-svg-render`: Sistema de renderizado de accesorios vía SVG inline para objetos con formas complejas (curvas, orgánicas, multi-capa).
- `accessory-quality-refactor`: Rediseño completo del CSS y SVG de todos los accesorios para alcanzar un nivel de detalle consistente y profesional.

### Modified Capabilities

- `avatar-customization`: Los accesorios existentes cambian su implementación visual (CSS→SVG o CSS refinado), manteniendo los mismos IDs, clases, y API de `AvatarState`. El panel de personalización no cambia su estructura.
- `avatar-rendering`: Los componentes `AvatarHead.razor` y `AvatarArm.razor` cambian su markup (divs CSS → SVG inline), pero mantienen los mismos parámetros y lógica de visibilidad.

## Impact

- `wwwroot/css/avatar.css` — refactor completo (~245 líneas → ~180 líneas): se eliminan definiciones CSS reemplazadas por SVG, se refinan las que quedan, se eliminan `drop-shadow` duplicados
- `Components/AvatarHead.razor` — reemplazo de divs de gorros/anteojos por SVG inline (~52 → ~120 líneas)
- `Components/AvatarArm.razor` — reemplazo de divs de items complejos por SVG inline; corrección de item-guitarra (~51 → ~200 líneas)
- `Components/AvatarTorso.razor` — sin cambios (pasa parámetros, no renderiza accesorios directamente)
- `Components/AvatarRenderer.razor` — sin cambios
- `Components/CustomizationPanel.razor` — sin cambios (IDs y handlers se mantienen)
- `Services/AvatarState.cs` — sin cambios
