## Why

El cambio `hybrid-local-storage` implemento el guardado local en `FotosAvatares/` y la subida a Drive en background via `asyncio.create_task`. Esto resolvio la experiencia inmediata: el usuario ya no espera segundos por el ACK del servidor.

Sin embargo, con conexiones a internet inestables (el escenario tipico de la expo), la subida a Drive sigue fallando de forma silenciosa y sin recuperacion:

- **Sin timeout en el cliente HTTP de Google**: `drive.files().create().execute()` no tiene timeout configurado. Con internet inestable, la llamada puede colgarse minutos, ocupando un hilo del threadpool sin liberarlo.
- **Thread pool se satura**: si varias fotos quedan pendientes de subida, cada una bloquea un hilo del `ThreadPoolExecutor` por defecto. Con 5-10 hilos, el pool se agota rapidamente y las subidas se serializan o se pierden.
- **Fotos huerfanas si el servidor se reinicia**: las tareas `asyncio.create_task()` son volatiles. Si el servidor crashea o se reinicia, las fotos pendientes en `FotosAvatares/` nunca se suben a Drive. No hay reconciliacion.
- **El cliente nunca sabe si Drive completo**: el ACK `screenshot_result` llega con la URL local. El resultado real de Drive se loguea al servidor pero nunca alcanza al cliente. El usuario no puede distinguir "guardada localmente" de "sincronizada con Drive".
- **Backoff corto para internet inestable**: los reintentos actuales esperan 2s y 4s. Para internet movil o WiFi de expo con micro-cortes frecuentes, se necesita backoff mas largo y con jitter.

## What Changes

- **Timeout explicito en Drive API**: Configurar `timeout=30` (connect) y `timeout=60` (read) en el cliente HTTP subyacente de la API de Google Drive, para que llamadas colgadas no bloqueen hilos indefinidamente.
- **Cola de upload persistente basada en filesystem**: En lugar de `asyncio.create_task` volatil, usar el propio directorio `FotosAvatares/` como cola. Cada foto pendiente de subir tiene un archivo `.pending` marcador. Al completar exitosamente la subida, se renombra a `.uploaded`. Al iniciar el servidor, se escanean las fotos con `.pending` y se reintentan.
- **Backoff exponencial con jitter**: Cambiar el backoff fijo (2s, 4s) por backoff exponencial con jitter aleatorio: `delay = min(300, (2 ** attempt) + random(0, 1000) / 1000)`. Esto evita tormentas de reintentos y maneja mejor micro-cortes largos.
- **Notificacion diferida al cliente via WebSocket**: Cuando el upload a Drive completa (exito o fallo definitivo), enviar un mensaje WebSocket `{"type":"drive_sync_result", "filename":"...", "success":true|false, "url":"..."}` para que el frontend pueda mostrar estado de sincronizacion.
- **UI mas precisa**: Cambiar el texto "Subiendo a Drive..." por "Guardando..." y agregar un indicador de sincronizacion (icono de nube) que muestre si la foto esta solo en local o tambien en Drive.

## Capabilities

### Modified Capabilities

- `drive-upload`: El modulo `google_drive.py` gana timeout HTTP, cola persistente via filesystem, y backoff con jitter.
- `avatar-screenshot`: El handler en `server.py` ahora coordina con la cola persistente y envia notificacion diferida de Drive al cliente.
- `screenshot-ui`: `CameraPanel.razor` escucha el nuevo evento `OnDriveSyncResult` para actualizar el indicador de sincronizacion.

### New Capabilities

- `upload-queue`: Cola persistente basada en archivos marcadores (`.pending` / `.uploaded`) en `FotosAvatares/`. Escaneo al iniciar, procesamiento en background con throttling, reintentos con jitter.

## Impact

- `server/google_drive.py` — agregar timeout HTTP al cliente de Drive, funcion `resume_pending_uploads()` que escanea y reintenta, nuevas funciones `mark_uploaded()` / `mark_failed()` usando archivos marcadores.
- `server/server.py` — al iniciar, llamar `resume_pending_uploads()`. En el handler de screenshot, crear marcador `.pending` antes de lanzar upload. Enviar `drive_sync_result` al completar/fail.
- `AvatarExpo/Services/TrackingService.cs` — nuevo evento `OnDriveSyncResult` y handler en `ReceiveLoop` para parsear mensajes `drive_sync_result`.
- `AvatarExpo/Components/CameraPanel.razor` — hook al nuevo evento, icono de sincronizacion (check / nube tachada), texto de estado mas preciso.
- `AvatarExpo/wwwroot/index.html` — si se agrega un icono SVG inline, referenciarlo.
