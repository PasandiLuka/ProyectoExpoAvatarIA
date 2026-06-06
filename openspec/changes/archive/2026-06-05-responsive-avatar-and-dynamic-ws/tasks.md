## 1. Contener avatar con overflow + escala dinámica

- [x] 1.1 En `avatar.css`, agregar `overflow: hidden` a `.panel-central`
- [x] 1.2 En `avatarAnim.js`, reemplazar `scale(2)` por escala dinámica calculada del tamaño del contenedor
- [x] 1.3 Verificar que el avatar ocupa ~85% del panel y nunca se desborda
- [x] 2.1 En `Program.cs`, reemplazar `config["WebSocket:Url"] ?? "ws://localhost:8765"` por derivación de `builder.HostEnvironment.BaseAddress`
- [x] 2.2 Probar: acceder desde otro dispositivo en la red → el WebSocket conecta al host correcto
- [x] 3.1 Compilar `dotnet build` — 0 errores, 0 warnings
- [ ] 3.2 Probar: mover brazos al máximo → el avatar no se sale del panel central
- [ ] 3.3 Probar: redimensionar la ventana del navegador → el avatar se adapta
- [ ] 3.4 Probar: acceder desde `http://<ip-local>:5000` → tracking funciona
