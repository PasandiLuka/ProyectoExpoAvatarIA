## 1. Cleanup global de CSS

- [x] 1.1 Reemplazar todos los `filter: drop-shadow(0 0 2px black)` por `box-shadow` equivalente en `.torso`, `.head`, `.arm-upper`, `.arm-lower`, `.hand`
- [x] 1.2 Quitar `bottom: 50px` de `.item-sable` — unificar con `bottom: 20px` de `.item`
- [x] 1.3 Agregar regla global para SVG rendering en `.item svg`, `.hat svg`, `.glasses svg`

## 2. Gorros — SVG inline (6 gorros) + CSS refinado (3 gorros)

- [x] 2.1 **Vaquero** (`.hat-cowboy`): SVG con ala curvada, copa con hendidura, banda de cuero marrón oscuro
- [x] 2.2 **Vikingo** (`.hat-vikingo`): SVG con casco metálico con gradiente, banda con remaches, cuernos curvados
- [x] 2.3 **Mexicano** (`.hat-sombrero-mexicano`): SVG con ala elíptica, copa cónica, banda decorativa, borla
- [x] 2.4 **Espacial** (`.hat-casco-espacial`): SVG con esfera + radialGradient, visor con gradiente azul, anillo del cuello
- [x] 2.5 **VR** (`.hat-diadema-vr`): SVG con banda, visores con brillo azul, sensores, cable
- [x] 2.6 **Corona** (`.hat-corona`): SVG con puntas, gradiente dorado, gemas rojas/azules, base
- [x] 2.7 **Chef** (`.hat-chef`): CSS refinado con pliegues radial-gradient, borde inferior, sombra
- [x] 2.8 **Cap** (`.hat-cap`): CSS refinado con visor mejorado, volumen, gradiente
- [x] 2.9 **Gorra Argentina** (`.hat-gorra-argentina`): CSS refinado con sol dorado en centro

## 3. Anteojos — SVG inline (4 anteojos) + CSS refinado (2 anteojos)

- [x] 3.1 **Sol** (`.glasses-sun`): SVG Wayfarer con lentes degradados, reflejo, puente, patillas
- [x] 3.2 **Nerd** (`.glasses-nerd`): SVG con marcos rectangulares gruesos, tinte azul, puente doble
- [x] 3.3 **Futurista** (`.glasses-futurista`): SVG con visor curveado cyan→magenta, líneas HUD
- [x] 3.4 **3D** (`.glasses-3d`): SVG con marco de cartón, lentes rojo/azul, texto "3D"
- [x] 3.5 **Googly** (`.glasses-googly`): CSS refinado con ojos más grandes, pupilas mejor posicionadas
- [x] 3.6 **Monóculo** (`.glasses-monoculo`): CSS refinado con aro dorado, cadena con repeating-linear-gradient

## 4. Items de mano — SVG inline (7 items)

- [x] 4.1 **Guitarra** (`.item-guitarra`): SVG con cuerpo 8-shape, boca con roseta, 6 cuerdas, mástil con trastes, clavijero
- [x] 4.2 **Fernet** (`.item-fernet`): SVG con botella curveada, etiqueta "FERNET BRANCA", tapa roja, texto detallado
- [x] 4.3 **Laptop** (`.item-laptop`): SVG con pantalla, bisel, teclado, trackpad, indicador LED
- [x] 4.4 **Celular** (`.item-celular`): SVG con marco redondeado, grid de apps coloridos, cámara, botón home
- [x] 4.5 **Sable Láser** (`.item-sable`): SVG con empuñadura detallada (emitter, grip, pommel), hoja con glow filter. Ya no usa `bottom: 50px`
- [x] 4.6 **Micrófono** (`.item-microfono`): SVG con grille de puntos, cuerpo texturizado, conector XLR
- [x] 4.7 **Copa Mundial** (`.item-cup`): SVG mejorado con gradiente dorado, asas curvas, texto "FIFA WORLD CUP", base amplia

## 5. Items de mano — CSS refinado (13 items)

- [x] 5.1 **Water**: Bordes redondeados, tapa azul, gradiente de agua, gotas de condensación
- [x] 5.2 **Mate**: Textura de calabaza radial-gradient, brillo en bombilla, sombra mejorada
- [x] 5.3 **Pelota**: Panelado con pentágono central, brillo especular
- [x] 5.4 **Bandera**: Adorno dorado en punta del mástil, sombra en bandera
- [x] 5.5 **Trofeo**: clip-path mejorado, gradiente metálico, asas curvas
- [x] 5.6 **Teclado**: Grid de teclas más fino, borde metálico
- [x] 5.7 **Mouse**: Scroll wheel, separación de botones, curvatura ergonómica
- [x] 5.8 **Libro**: Lomo visible, cinta marcadora roja colgante
- [x] 5.9 **Pizza**: Borde de masa, pepperonis con box-shadow, queso con brillo
- [x] 5.10 **Hamburguesa**: Semillas de sésamo con radial-gradient, queso, lechuga
- [x] 5.11 **Pochoclos**: Popcorns individuales con radial-gradient, líneas definidas
- [x] 5.12 **Pokebola**: Brillo especular, línea divisoria, anillo metálico en botón
- [x] 5.13 **Pokémon**: drop-shadow suave para integración visual

## 6. Peinados — rediseño de los 9 estilos

- [x] 6.1 **Style 1**: Corto prolijo clásico con box-shadow para volumen
- [x] 6.2 **Style 2**: Flequillo asimétrico barrido, clip-path angular
- [x] 6.3 **Style 3**: Corto rectangular buzzcut, gradiente oscuro
- [x] 6.4 **Style 4**: Semilargo con volumen, cubre laterales, background degradado
- [x] 6.5 **Style 5**: Cresta/pico central con clip-path triangular, laterales transparentes
- [x] 6.6 **Style 6**: Afro voluminoso con box-shadow para textura de rizos
- [x] 6.7 **Style 7**: Rapado/calvo, solo sombra sutil en cuero cabelludo
- [x] 6.8 **Style 8**: Largo lacio con raya al medio, clip-path ancho
- [x] 6.9 **Style 9**: Cola/recogido atrás, frente despejada, box-shadow para el rodete

## 7. Remeras — detalles adicionales

- [x] 7.1 **Boca**: Número "12" en dorado, escudo circular en pecho
- [x] 7.2 **River**: Número "9" en rojo con text-shadow
- [x] 7.3 **Argentina**: Número "10" en negro
- [x] 7.4 **Brasil**: Número "10" en amarillo sobre círculo verde
- [x] 7.5 **España**: Número "6" en amarillo con text-shadow rojo
- [x] 7.6 **Alemania**: Número "13" en blanco
- [x] 7.7 **C#**: Ícono "#" grande (48px), fondo púrpura
- [x] 7.8 **Python**: Ícono de serpiente con radial-gradient dual
- [x] 7.9 **JS**: Texto "{JS}" con llaves, fondo amarillo
- [x] 7.10 **Matrix**: Lluvia de caracteres con repeating-linear-gradient vertical, texto ">_"
- [x] 7.11 **Short**: Fondo gris (mantiene diseño simple)
- [x] 7.12 **Long**: Fondo oscuro (mantiene diseño simple)

## 8. Actualización de componentes Razor

- [x] 8.1 `AvatarHead.razor`: Reemplazados 6 gorros + 4 anteojos por SVG inline detallados. Mantenida estructura de clases condicionales
- [x] 8.2 `AvatarArm.razor`: Reemplazados 7 items complejos por SVG inline. Agregado markup de guitarra. Mantenido `stay-upright` en todos
- [x] 8.3 `AvatarHead.razor`: Clases CSS de peinados se mantienen (solo cambió el CSS)
- [x] 8.4 `AvatarTorso.razor`: Sin cambios (pasa parámetros, no tiene markup de accesorios)

## 9. CSS final — limpieza y organización

- [x] 9.1 Eliminadas definiciones CSS de accesorios migrados a SVG (~25 reglas removidas)
- [x] 9.2 Refinado CSS de 13 items mano + 3 gorros + 2 anteojos + 9 peinados + 12 remeras
- [x] 9.3 Agregada regla para SVG items: `.item svg`, `.hat svg`, `.glasses svg { width:100%; height:100%; display:block; }`
- [x] 9.4 Agregadas variables CSS: `--gold`, `--gold-dark`, `--silver`, `--leather`, `--metal`
- [x] 9.5 Organizado con secciones comentadas: `=== TORSO & SHIRTS ===`, `=== HAIR ===`, `=== HATS ===`, etc.
- [x] 9.6 `app.css`: Sin cambios (panel de personalización intacto)

## 10. Verificación

- [x] 10.1 Compilar Blazor: `dotnet build` — 0 errores, 0 warnings
- [x] 10.2 Verificar visualmente cada gorro (10): alternar en el panel, confirmar que se ve correcto, proporcionado, y reconocible
- [x] 10.3 Verificar visualmente cada anteojo (7): alternar en el panel, confirmar alineación sobre los ojos
- [x] 10.4 Verificar visualmente cada item de mano (27): equipar en mano izquierda y derecha, confirmar que se ve reconocible
- [x] 10.5 Verificar peinados (9): confirmar que los 9 estilos son visualmente distintos entre sí
- [x] 10.6 Verificar remeras (12): confirmar que los números e íconos se ven claramente
- [x] 10.7 Verificar tracking: items `stay-upright` compensan rotación correctamente (SVG y CSS)
- [x] 10.8 Verificar captura: `Sacar Foto` con accesorios SVG equipados produce imagen correcta vía html2canvas
- [x] 10.9 Verificar performance: no hay lag ni flickering al rotar brazos con SVG items equipados
- [x] 10.10 Verificar combinaciones: equipar gorro + anteojos + items en ambas manos simultáneamente
