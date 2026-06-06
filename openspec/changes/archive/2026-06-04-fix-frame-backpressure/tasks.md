## 1. Frame dropping en servidor

- [x] 1.1 Reemplazar `async for message in websocket:` por loop con drenado en `handle_connection` de `server.py`
- [x] 1.2 Agregar `import asyncio` si no está ya (debería estarlo)
- [x] 1.3 Verificar que el drenado no rompe el manejo de mensajes `str` (ping)
- [x] 1.4 Verificar que el frame descartado no deja basura en el buffer de `np.frombuffer`

## 2. Limpiar skeleton canvas al detener

- [x] 2.1 En `camera.js` `stop()`, agregar `clearRect` al `#skeleton-canvas` antes de limpiar el video
- [x] 2.2 En `CameraPanel.razor` `ToggleTracking()`, agregar cleanup del skeleton canvas al detener tracking

## 3. Verificación

- [x] 3.1 Probar tracking: mover brazos rápido frente a la cámara, verificar que el avatar responde sin lag acumulativo
- [x] 3.2 Probar tracking: mantener posición fija 5 segundos, verificar que el avatar no "deriva" (no procesa frames viejos)
- [x] 3.3 Probar detener: al clickear "Detener", el esqueleto verde desaparece inmediatamente
- [x] 3.4 Probar detener: al clickear "Detener", el avatar vuelve a posición neutral
- [x] 3.5 Probar reconexión: volver a iniciar tracking después de detener, verificar que funciona normalmente
- [x] 3.6 Verificar server console: sin errores de Python por el drenado
