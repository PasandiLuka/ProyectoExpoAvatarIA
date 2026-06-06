## 1. TrackingService — WebSocket siempre conectado + feedback + evento de conexión

- [x] 1.1 Cambiar firma de `SendScreenshot(string base64)` de `Task` a `Task<bool>`
- [x] 1.2 Retornar `false` cuando `_ws?.State != WebSocketState.Open` (en vez de `return` vacío)
- [x] 1.3 Retornar `true` solo si el `SendAsync` se completa sin excepción
- [x] 1.4 Agregar evento `OnConnectionChanged(bool connected)` separado de `OnTrackingStateChanged`
- [x] 1.5 Disparar `OnConnectionChanged` en `TryConnect` (éxito=true, falla=false) y en `ReceiveLoop` al desconectarse (false)

## 2. CameraPanel — conexión WebSocket temprana + desacople de estado

- [x] 2.1 Llamar `TrackingService.Connect()` en `OnInitializedAsync()` para conectar al iniciar la app
- [x] 2.2 Modificar `ToggleTracking()` para que solo inicie/detenga la cámara, no el WebSocket
- [x] 2.3 Asegurar que `Disconnect()` no se llama al detener tracking (WebSocket se mantiene abierto)
- [x] 2.4 Eliminar suscripción a `OnTrackingStateChanged` en `CameraPanel` (el evento refleja estado de WebSocket, no de tracking)
- [x] 2.5 Manejar `IsTracking`/`IsDemoMode` exclusivamente en `ToggleTracking()` sin depender de eventos del WebSocket
- [x] 2.6 Suscribirse a `OnConnectionChanged` para habilitar/deshabilitar botón "Sacar Foto" según estado del WebSocket
- [x] 2.7 Botón "Sacar Foto" deshabilitado con texto "Sin conexion" cuando `!IsConnected`

## 3. CameraPanel — countdown pre-captura cancelable

- [x] 3.1 Agregar campo `_captureDelaySeconds` (default 5), `_countdownRemaining`, y `_captureCts`
- [x] 3.2 En `TakeScreenshot`, ejecutar countdown de N segundos actualizando texto del botón cada segundo
- [x] 3.3 Segundo click durante countdown cancela vía `_captureCts.Cancel()`, restaura botón a "Sacar Foto"
- [x] 3.4 Al finalizar countdown, disparar `CameraService.CaptureScreenshot()` y luego `SendScreenshot()`
- [x] 3.5 Si delay es 0, disparar captura inmediatamente sin countdown

## 4. CameraPanel — slider de delay en menú hamburguesa

- [x] 4.1 Agregar slider "Delay foto" (0-10, step 1) debajo de la sección de escala en el menú desplegable
- [x] 4.2 Mostrar valor actual del delay (ej: "5s") al lado del slider
- [x] 4.3 Deshabilitar slider cuando `!IsTracking` (modo demo): `disabled="@(!IsTracking)"`
- [x] 4.4 Renombrar toggle del menú de "≡ Tamaño Avatar" a "≡ Ajustes"

## 5. CameraPanel — timeout y manejo de errores

- [x] 5.1 Agregar timeout de seguridad: `Task.Delay(30000)` en `TakeScreenshot`
- [x] 5.2 Si `SendScreenshot` retorna `false`, mostrar "Error de conexion" y liberar botón
- [x] 5.3 Si `SendScreenshot` retorna `true`, esperar screenshot_result o timeout (lo que ocurra primero)
- [x] 5.4 Asegurar que `_isCapturing` se setea a `false` en todos los paths (éxito, error, timeout, excepción)
- [x] 5.5 Mostrar mensaje de error específico según el caso: "Error de conexion", "Error al subir la foto", "Timeout al subir la foto"

## 6. Verificación

- [x] 6.1 Probar "Sacar Foto" con tracking activo y delay 5s — countdown visible, imagen en Drive
- [x] 6.2 Probar "Sacar Foto" en modo demo — imagen en Drive sin countdown (slider deshabilitado)
- [x] 6.3 Probar "Sacar Foto" con delay 0s — captura inmediata
- [x] 6.4 Ajustar slider de delay a 3s y verificar countdown de 3 segundos
- [x] 6.5 Probar con server caído — mensaje de error y botón se rehabilita
- [x] 6.6 Verificar que detener tracking no cierra el WebSocket (screenshot sigue funcionando)
- [x] 6.7 Verificar que reconexión automática del WebSocket no altera la UI de tracking (modo demo se mantiene)
- [x] 6.8 Verificar que segundo click durante countdown cancela la captura y rehabilita el botón
- [x] 6.9 Verificar que botón "Sacar Foto" se deshabilita con texto "Sin conexion" cuando el server está caído
- [x] 6.10 Verificar que botón se rehabilita automáticamente cuando el server vuelve
