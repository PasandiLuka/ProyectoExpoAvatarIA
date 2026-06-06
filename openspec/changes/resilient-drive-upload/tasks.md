## 1. Timeout en cliente HTTP de Google Drive

- [x] 1.1 En `server/google_drive.py`, crear funcion `_build_drive_service(creds)` que configure un `Http` con `timeout=30` (connect) y `http.timeout` equivalente para read (usar `timeout` del objeto `Http` en googleapiclient)
- [x] 1.2 Modificar `upload_to_drive()` (linea 63-81) para usar `_build_drive_service(creds)` en lugar de `build("drive", "v3", credentials=creds)`
- [x] 1.3 Verificar que la excepcion de timeout (`socket.timeout`) es capturada correctamente por `upload_to_drive_with_retry()`

## 2. Marcadores de estado en filesystem

- [x] 2.1 En `server/google_drive.py`, crear funcion `mark_pending(png_path: str)`: escribe archivo vacio `{png_path}.pending`
- [x] 2.2 Crear funcion `mark_uploaded(png_path: str)`: renombra `{png_path}.pending` a `{png_path}.uploaded` via `os.rename()`
- [x] 2.3 Crear funcion `clear_pending(png_path: str)`: elimina `{png_path}.pending` si existe (para usar cuando `.uploaded` ya existe)
- [x] 2.4 Agregar helpers `_has_uploaded(png_path)` y `_has_pending(png_path)` que verifican existencia de marcadores

## 3. Backoff con jitter

- [x] 3.1 En `server/google_drive.py`, modificar `upload_to_drive_with_retry()`:
  - Cambiar `max_retries=3` a `max_retries=5`
  - Reemplazar `delay = 2 ** (attempt + 1)` por `base_delay = min(120, 2 ** (attempt + 2))` con jitter: `delay = base_delay + random.uniform(0, base_delay * 0.5)`
  - Agregar `import random`
- [x] 3.2 Asegurar que el loop reintenta tambien en caso de `result["success"] == False` (no solo excepciones)

## 4. Escaneo y reanudacion de uploads pendientes al iniciar

- [x] 4.1 En `server/google_drive.py`, crear `async def resume_pending_uploads()`:
  - Escanear `FotosAvatares/` buscando archivos `*.png.pending`
  - Para cada uno, verificar que el PNG existe y que no tiene `.uploaded`
  - Si tiene `.uploaded`, borrar el `.pending` huerfano
  - Si el PNG existe sin `.uploaded`, leerlo y lanzar upload con reintentos
  - Usar `asyncio.Semaphore(2)` para limitar concurrencia
  - Al completar exitosamente, llamar `mark_uploaded(png_path)`
- [x] 4.2 En `server/server.py`, en `main()` (linea 221), despues de iniciar el servidor, llamar `asyncio.create_task(resume_pending_uploads())`

## 5. Notificacion diferida al cliente via WebSocket

- [x] 5.1 En `server/server.py`, crear `async def upload_and_notify(websocket, img_bytes, filename, local_path)`:
  - Llamar `upload_to_drive_with_retry(img_bytes, filename)`
  - Si exito: `mark_uploaded(local_path)`, enviar `drive_sync_result` con `success: true` y URL de Drive
  - Si fallo: enviar `drive_sync_result` con `success: false`
  - Manejar `WebSocket` cerrado (el cliente pudo desconectarse): capturar `ConnectionClosed` y loguear
- [x] 5.2 Modificar handler de screenshot en `handle_connection()` (linea 136-159):
  - Despues de guardar localmente, crear marcador `.pending` con `mark_pending(local_path)`
  - Reemplazar `asyncio.create_task(upload_to_drive_with_retry(...))` por `asyncio.create_task(upload_and_notify(websocket, img_bytes, filename, local_path))`
  - La respuesta `screenshot_result` sigue igual (ACK inmediato con URL local)
- [x] 5.3 Formato del mensaje `drive_sync_result`:
  ```json
  {"type": "drive_sync_result", "filename": "...", "success": true/false, "url": "..."}
  ```

## 6. Cliente: evento OnDriveSyncResult y handler en ReceiveLoop

- [x] 6.1 En `AvatarExpo/Services/TrackingService.cs`, agregar nuevo evento:
  - `public event Action<string, bool, string>? OnDriveSyncResult;` // (filename, success, driveUrl)
- [x] 6.2 En `ReceiveLoop()` (linea 99-197), agregar parsing de `type == "drive_sync_result"`:
  - Extraer `filename` (string), `success` (bool), `url` (string)
  - Disparar `OnDriveSyncResult?.Invoke(filename, success, url)`
  - No es necesario bloquear en `screenshot_result`; `drive_sync_result` es independiente y puede llegar en cualquier momento

## 7. UI: estado de sincronizacion

- [x] 7.1 En `AvatarExpo/Components/CameraPanel.razor`, agregar:
  - En el bloque `@code`: enum `DriveSyncState { None, Pending, Synced, Failed }` y campo `private DriveSyncState _driveSyncState`
  - Campos `private string _lastDriveFilename` y `private string _lastDriveUrl`
- [x] 7.2 Cambiar texto de estado en `TakeScreenshot()`: `_captureStatus = "Guardando..."` (linea 403) en lugar de "Subiendo a Drive..."
- [x] 7.3 Cambiar texto de exito al recibir ACK: en el handler de `OnScreenshotResult` (linea 414), cambiar "Error al subir la foto" por "Error al guardar la foto" y setear `_driveSyncState = DriveSyncState.Pending`
- [x] 7.4 Suscribirse a `TrackingService.OnDriveSyncResult` en `OnInitializedAsync()` o `OnAfterRenderAsync()`:
  - Si `success == true`: `_driveSyncState = DriveSyncState.Synced; _lastDriveUrl = driveUrl;`
  - Si `success == false`: `_driveSyncState = DriveSyncState.Failed;`
  - Llamar `StateHasChanged()` e `InvokeAsync()` para actualizar UI
- [x] 7.5 En el template Razor, mostrar icono de sincronizacion debajo del mensaje "Foto guardada":
  - `DriveSyncState.Pending`: `<span title="Sincronizando con Drive...">&#x2601;&#xFE0F;</span>`
  - `DriveSyncState.Synced`: `<span title="Sincronizada con Drive">&#x2705;</span>`
  - `DriveSyncState.Failed`: `<span title="Solo guardada localmente">&#x26A0;&#xFE0F;</span>`
  - `DriveSyncState.None`: no mostrar nada
- [x] 7.6 En `EndCapture()`, limpiar `_driveSyncState = DriveSyncState.None`

## 8. Verificacion

- [x] 8.1 Probar flujo con internet estable: tomar foto → ver PNG local + .pending → esperar upload → ver .uploaded + icono ✅ en UI
- [x] 8.2 Probar timeout de Drive: simular desconexion de internet → tomar foto → ver .pending persiste → reconectar → el servidor debe reanudar uploads pendientes al reiniciar
- [x] 8.3 Probar reanudacion al iniciar: dejar fotos con .pending → reiniciar servidor → verificar que los uploads se reanudan
- [x] 8.4 Probar backoff con jitter: loguear delays para confirmar que varian entre intentos
- [x] 8.5 Probar semaforo de concurrencia: tomar 5 fotos rapidas → verificar que solo 2 uploads corren simultaneamente (ver logs)
- [x] 8.6 Probar notificacion diferida: verificar que el mensaje `drive_sync_result` llega al cliente y la UI muestra el icono correcto
- [x] 8.7 Probar que `dotnet build` compila sin errores
