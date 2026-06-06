## Why

**Avatar overflow:** El avatar se desborda del panel central porque `.panel-central` no tiene `overflow: hidden` y `avatarAnim.js` usa `scale(2)` hardcodeado. Los brazos rotados, la cabeza y las manos se salen del contenedor, especialmente en pantallas chicas o con movimientos amplios.

**WebSocket hardcodeado:** `ws://localhost:8765` está fijo en `appsettings.json` y en el fallback de `Program.cs`. Cuando otro dispositivo accede a la app (ej: `http://192.168.1.5:5000`), el WebSocket intenta conectar a `localhost` de ESE dispositivo, no del servidor. La app solo funciona en la máquina local.

## What Changes

**Avatar responsive:**
- `avatar.css`: `.panel-central` + `overflow: hidden`
- `avatarAnim.js`: reemplazar `scale(2)` hardcodeado por escala dinámica basada en tamaño del contenedor

**WebSocket dinámico:**
- `Program.cs`: construir URL del WebSocket a partir del host HTTP (`builder.HostEnvironment.BaseAddress`) en vez de leer de config estática

## Impact

- `AvatarExpo/wwwroot/css/avatar.css` — agregar `overflow: hidden` + ajustar padding
- `AvatarExpo/wwwroot/js/avatarAnim.js` — escala dinámica
- `AvatarExpo/Program.cs` — 1 línea, cambiar `config["WebSocket:Url"]` por derivación del host HTTP
