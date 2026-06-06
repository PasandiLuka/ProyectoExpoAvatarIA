## 1. Modificar avatarAnim.js

- [x] 1.1 Agregar `manualScale: null` a `window.avatarAnim`
- [x] 1.2 Agregar método `setManualScale(value)` que asigna `this.manualScale = value`
- [x] 1.3 En `update()`, si `this.manualScale != null`, usar ese valor en vez del cálculo dinámico
- [x] 2.1 Agregar HTML: botón toggle + div colapsable con slider range + botón Auto
- [x] 2.2 Agregar `@code`: `ScaleMenuOpen`, `CurrentScale`, `ToggleScaleMenu()`, `OnScaleInput()`, `SetAutoScale()`
- [x] 2.3 Agregar estilos CSS para `.scale-menu` (colapsable, animado)
- [x] 3.1 Compilar `dotnet build` — 0 errores, 0 warnings
- [ ] 3.2 Probar: abrir menú hamburguesa → slider visible
- [ ] 3.3 Probar: mover slider → avatar cambia de tamaño proporcionalmente
- [ ] 3.4 Probar: clickear "Auto" → vuelve al tamaño dinámico
- [ ] 3.5 Probar: cerrar menú → no ocupa espacio
