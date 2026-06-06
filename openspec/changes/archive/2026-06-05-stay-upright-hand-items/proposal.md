## Why

Los items en las manos (fernet, botella de agua, copa) rotan con el brazo y no se ven naturales. Se necesita que ciertos items (bebidas) siempre apunten hacia arriba independientemente de la posición del brazo, mientras que otros (reloj, pulsera) sigan la rotación de la mano.

La solución anterior con CSS fijo (`rotate(90deg)` / `rotate(-90deg)`) no sirve: no contrarresta la rotación del brazo y no es opt-in por item.

## What Changes

- **`avatarAnim.js`**: en `update()`, buscar elementos `.item.stay-upright.active` y aplicarles `rotate(±90° - shoulderAngle - elbowAngle)` para que siempre apunten hacia arriba
- **`avatar.css`**: eliminar las reglas fijas `.arm-container.left/right .item.active`; agregar estilo base para `.item.stay-upright`
- **`AvatarArm.razor`**: agregar clase `stay-upright` a fernet, water, cup (no a items futuros como reloj)

## Impact

- `wwwroot/js/avatarAnim.js` — ~6 líneas en `update()`
- `wwwroot/css/avatar.css` — eliminar 2 líneas, agregar comentario
- `Components/AvatarArm.razor` — +2 líneas (`stay-upright`)
