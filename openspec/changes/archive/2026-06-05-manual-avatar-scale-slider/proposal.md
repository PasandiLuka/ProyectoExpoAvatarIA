## Why

El avatar se escala automáticamente al 85% del panel central. Pero a veces el usuario quiere un avatar más grande o más chico según su preferencia o el espacio disponible. Un slider manual con override sobre el scale automático permite ajustarlo a gusto.

El slider aplica scale al `body-wrapper` que contiene torso, brazos, cabeza y manos — el escalado es proporcional, no deforma partes individuales.

## What Changes

- **`avatarAnim.js`**: nueva propiedad `manualScale` + método `setManualScale(value)`. Si `manualScale` no es null, `update()` usa ese valor en vez del cálculo dinámico.
- **`CameraPanel.razor`**: nuevo menú hamburguesa colapsable "Tamaño Avatar" con slider 0.5–3 + botón "Auto" para volver al modo dinámico.
- **`avatar.css` / `app.css`**: estilos para el menú hamburguesa colapsable.

## Impact

- `wwwroot/js/avatarAnim.js` — +6 líneas (propiedad + método)
- `Components/CameraPanel.razor` — +~20 líneas (HTML hamburguesa + slider + @code)
- `wwwroot/css/avatar.css` — +~10 líneas (estilos hamburguesa)
