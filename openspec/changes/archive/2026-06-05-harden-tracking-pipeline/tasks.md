## 1. Blindaje de ReceiveLoop en TrackingService.cs

- [x] 1.1 Agregar `using` para `System.Diagnostics` si no existe (para `Debug.WriteLine` como alternativa)
- [x] 1.2 Agregar `catch (Exception ex)` al final del bloque try-catch en `ReceiveLoop()` (despues de `catch (WebSocketException)`)
- [x] 1.3 En el nuevo catch: `Console.WriteLine($"[TrackingService] ReceiveLoop error: {ex}");` seguido de `break;` (el bloque post-loop ya maneja reconexion via `ReconnectLoop()`)
- [x] 1.4 Envolver el bloque `if (data?.P != null)` dentro de `ReceiveLoop` en un try/catch interno que loguee y continue (no break) para que un handler que falla no mate el loop

## 2. Logging de diagnostico en TrackingService

- [x] 2.1 Agregar `Console.WriteLine($"[TrackingService] Conectando a {_serverUrl}...")` al inicio de `TryConnect()`
- [x] 2.2 Agregar `Console.WriteLine("[TrackingService] Conectado, esperando landmarks...")` despues de iniciar `_receiveTask` y `_heartbeatTask` en `TryConnect()`
- [x] 2.3 Agregar un contador `_frameCount` en `ReceiveLoop` y loguear `Console.WriteLine($"[TrackingService] Landmarks OK | frame={_frameCount} | fps={CurrentFps}")` cada 60 frames (~2 segundos a 30fps)
- [x] 2.4 Agregar `Console.WriteLine($"[TrackingService] Desconectado: {ex?.Message}")` en el bloque post-loop de `ReceiveLoop` que activa reconexion

## 3. Eliminar doble suavizado CSS

- [x] 3.1 En `avatar.css`, eliminar `transition: transform 0.1s ease-out;` de `.arm-container` (linea 110)
- [x] 3.2 En `avatar.css`, eliminar `transition: transform 0.1s ease-out;` de `.arm-lower` (linea 135)
- [x] 3.3 En `avatar.css`, eliminar `transition: transform 0.2s ease-out;` de `.body-wrapper` (linea 26)
- [x] 3.4 Verificar que `.head` conserva su `transition: transform 0.1s ease-out;` (la cabeza no usa Lerp, necesita transicion CSS)

## 4. Proteccion en AvatarRenderer.HandleLandmarks

- [x] 4.1 Envolver el cuerpo de `HandleLandmarks()` en un try/catch
- [x] 4.2 En el catch: `Console.WriteLine($"[AvatarRenderer] HandleLandmarks error: {ex.Message}");` y retornar sin actualizar angulos (el avatar mantiene su ultima posicion valida)
- [x] 4.3 Agregar validacion: si `double.IsNaN()` o `double.IsInfinity()` en cualquier angulo calculado, loguear warning y usar el valor anterior en vez de NaN

## 5. Indicador de pipeline health en CameraPanel

- [x] 5.1 Agregar campo `private bool _hasRecentLandmarks;` en CameraPanel.razor
- [x] 5.2 Suscribirse a `TrackingService.OnLandmarksReceived` con un handler que sete `_hasRecentLandmarks = true`
- [x] 5.3 Agregar un timer o contador que resetee `_hasRecentLandmarks = false` si pasan 3 segundos sin landmarks
- [x] 5.4 Modificar el HTML debajo del canvas: agregar segunda linea con `FPS: @TrackingService.CurrentFps | Landmarks: @(_hasRecentLandmarks ? "OK" : "Sin datos")`
- [x] 5.5 Suscribirse a `TrackingService.OnStatusChanged` para mostrar el estado (ya esta suscripto via `HandleState`, verificar que `OnStatusChanged` se use)

## 6. Verificacion

- [x] 6.1 Compilar `dotnet build` — 0 errores, 0 warnings
- [ ] 6.2 Probar tracking: abrir DevTools → Console, verificar que aparecen logs de conexion y landmarks
- [ ] 6.3 Probar tracking: mover brazos frente a la camara, verificar que el avatar responde sin latencia extra (sin el doble suavizado)
- [ ] 6.4 Probar resiliencia: desconectar el servidor Python mientras el tracking esta activo, verificar que la UI muestra "Reconectando..." y se recupera al reiniciar el servidor
- [ ] 6.5 Probar indicador health: verificar que "Landmarks: OK" aparece cuando los datos fluyen y cambia a "Sin datos" al parar el servidor
- [ ] 6.6 Probar modo demo: verificar que sliders de torso/cabeza/brazos siguen funcionando sin las transiciones CSS
