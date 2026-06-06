## Context

El cambio `hybrid-local-storage` ya implemento la base: guardado local inmediato en `FotosAvatares/` y subida a Drive en background via `asyncio.create_task`. Sin embargo, para internet inestable este enfoque tiene gaps criticos:

1. `google_drive.py:76` — `drive.files().create().execute()` no tiene timeout. Con internet inestable se cuelga indefinidamente.
2. `google_drive.py:88` — `loop.run_in_executor(None, upload_to_drive, ...)` usa el `ThreadPoolExecutor` por defecto (limitado a min(32, os.cpu_count() + 4) hilos). Uploads colgados saturan el pool.
3. `server.py:151` — `asyncio.create_task(...)` es volatil. Si el servidor se reinicia, las tareas pendientes se pierden.
4. `server.py:146-150` — el ACK al cliente contiene `local:FotosAvatares/...`. El resultado real de Drive nunca llega al cliente.
5. `CameraPanel.razor:403` — el texto "Subiendo a Drive..." es engañoso, ya que la respuesta es sobre guardado local.

### Estado actual del codigo

```
FotosAvatares/           ← directorio existente, ya en .gitignore
├── avatar-20260606-143022.png
├── avatar-20260606-143030.png
└── ...

server/server.py         ← handler de screenshot ya guarda local + fire-and-forget
server/google_drive.py   ← upload_to_drive_with_retry con backoff 2s/4s, sin timeout
```

### Estado actual del flujo

```
Cliente                    Servidor                        Drive
   │                          │                              │
   ├─ WS screenshot ─────────▶│                              │
   │                          ├─ save local ────▶ FotosAvatares/
   │                          ├─ ACK "local:..." ─▶ cliente  │
   │                          └─ create_task ────▶ upload ──▶│
   │                          │                    (sin timeout)
   │                          │                    (volatil: si crashea → se pierde)
```

## Goals / Non-Goals

**Goals:**
- Timeout explicito (30s connect, 60s read) en el cliente HTTP de Google Drive
- Cola de upload persistente: fotos en `FotosAvatares/` con marcador `.pending` sobreviven reinicios del servidor
- Al iniciar el servidor, escanear y reanudar uploads pendientes
- Backoff exponencial con jitter para manejar micro-cortes largos (WiFi de expo)
- Notificar al cliente via WebSocket cuando Drive completa o falla definitivamente (mensaje `drive_sync_result`)
- UI: texto mas preciso ("Guardando...") nuevo icono de sincronizacion

**Non-Goals:**
- Servir las fotos via HTTP (sin cambios)
- Limpieza automatica de fotos viejas (sin cambios)
- Galeria o visualizador en el frontend (sin cambios)
- Progreso de upload en tiempo real (solo estado final: sincronizado o pendiente)
- Soportar upload desde multiples clientes simultaneos (sin cambios)

## Decisions

### Decision 1: Timeout en el cliente HTTP de Google Drive

**Problema**: `drive.files().create().execute()` usa el cliente HTTP por defecto de `google-api-python-client`, que no tiene timeout configurado.

**Solucion**: Construir un `Http` personalizado con timeouts y pasarlo al `build()`:

```python
from googleapiclient.http import build_http

def _build_drive_service(creds):
    http = build_http()
    http.timeout = 30        # connect timeout
    http_read_timeout = 60   # read timeout (upload de PNG ~500KB no necesita mas)
    return build("drive", "v3", credentials=creds, http=http)
```

Alternativa considerada: usar `socket.setdefaulttimeout()`. Descartado porque afecta globalmente, incluido el WebSocket.

**Timeouts elegidos**:
| Timeout | Valor | Justificacion |
|---------|-------|---------------|
| Connect | 30s | Suficiente para TCP handshake incluso en 3G |
| Read | 60s | PNG ~500KB, incluso a 10KB/s toma ~50s |

Si el timeout se alcanza, la excepcion `socket.timeout` o `urllib3.exceptions.ReadTimeoutError` es capturada por el retry wrapper.

### Decision 2: Cola de upload persistente basada en filesystem

**Problema**: Las tareas `asyncio.create_task()` son volatiles. Si el server se reinicia, se pierden.

**Solucion**: Usar el propio `FotosAvatares/` como cola persistente con archivos marcadores:

```
FotosAvatares/
├── avatar-20260606-143022.png          ← PNG guardado
├── avatar-20260606-143022.png.pending  ← marcador: pendiente de subir
├── avatar-20260606-143030.png
├── avatar-20260606-143030.png.pending
├── avatar-20260606-142000.png
└── avatar-20260606-142000.png.uploaded ← marcador: ya subido a Drive
```

**Flujo**:

```
Al guardar localmente:
  ├─ Escribir PNG a FotosAvatares/<filename>
  ├─ Crear archivo marcador .pending: open(f"{local_path}.pending", "w").close()
  └─ ACK al cliente (ya existente)

Upload en background (por cada .pending):
  ├─ Leer PNG
  ├─ upload_to_drive(...) con timeout
  ├─ Si exito: os.rename(f"{path}.pending", f"{path}.uploaded")
  ├─ Si fallo temporal: reintentar con backoff, .pending se mantiene
  └─ Si fallo definitivo (3 intentos con backoff largo): log, .pending se mantiene

Al iniciar el servidor:
  └─ resume_pending_uploads():
       ├─ Glob: FotosAvatares/*.png.pending pero sin .uploaded correspondiente
       ├─ Para cada uno: crear asyncio.Task de upload con reintentos
       └─ Procesar con throttling (max 2 uploads concurrentes)
```

**Ventajas del enfoque**:
- Sin dependencias externas (no Redis, no SQLite, no archivo JSON de estado)
- El estado es auto-contenido: si un archivo no tiene `.uploaded`, esta pendiente
- Facil de debuggear: `ls FotosAvatares/` muestra el estado visualmente
- Atómico: `os.rename()` es atómico en Linux

**Throttling**: `asyncio.Semaphore(2)` limita a 2 uploads concurrentes para no saturar el thread pool ni la API de Drive.

**Limpieza de marcadores**: Opcionalmente, luego de cierto tiempo (ej. 24h), los `.uploaded` pueden eliminarse para no acumular archivos vacios. Esto queda fuera de scope como decision de diseño (se implementa en tarea separada si se requiere).

### Decision 3: Backoff exponencial con jitter

**Problema**: Backoff fijo (2s, 4s) no maneja bien micro-cortes de WiFi de expo (que pueden durar 10-30s).

**Solucion**: Backoff exponencial con jitter aleatorio:

```python
import random

MAX_BACKOFF = 120  # maximo 2 minutos

for attempt in range(max_retries):
    try:
        result = await loop.run_in_executor(None, upload_to_drive, image_bytes, filename)
        if result["success"]:
            return True
    except Exception as e:
        if attempt < max_retries - 1:
            base_delay = min(MAX_BACKOFF, 2 ** (attempt + 2))  # 4, 8, 16, 32, 64, 128...
            jitter = random.uniform(0, base_delay * 0.5)
            delay = base_delay + jitter
            await asyncio.sleep(delay)
```

**Plan de reintentos**:
| Intento | Delay base | Con jitter (~) | Tiempo acumulado |
|---------|-----------|----------------|-----------------|
| 1 (fallo) | 4s | 4-6s | ~5s |
| 2 (fallo) | 8s | 8-12s | ~15s |
| 3 (fallo) | 16s | 16-24s | ~35s |
| 4 (fallo) | 32s | 32-48s | ~75s |
| 5 (fallo) | 64s | 64-96s | ~155s |

Con max_retries=5, el upload puede reintentar durante ~2.5 minutos antes de rendirse. Esto cubre micro-cortes de WiFi tipicos (10-60s).

**Jitter**: Evita que multiples uploads reintenten al mismo tiempo (thundering herd). Cada intento tiene un componente aleatorio.

### Decision 4: Notificacion diferida al cliente

**Problema**: El cliente solo recibe `screenshot_result` con URL local. No sabe si Drive completo.

**Solucion**: Agregar un nuevo tipo de mensaje WebSocket `drive_sync_result`:

```json
{
  "type": "drive_sync_result",
  "filename": "avatar-20260606-143022.png",
  "success": true,
  "url": "https://drive.google.com/file/d/XXX/view"
}
```

Este mensaje se envia DESPUES de que el upload a Drive completa (o falla definitivamente). El cliente lo recibe en `ReceiveLoop` y dispara un nuevo evento.

**Flujo completo con notificacion**:

```
Cliente                    Servidor                        Drive
   │                          │                              │
   ├─ WS screenshot ─────────▶│                              │
   │                          ├─ save local + .pending       │
   │                          ├─ ACK "local:..." ─▶ cliente  │
   │  UI: "Guardando..."      │                              │
   │  UI: ✅ checkmark local  │                              │
   │                          ├─ upload con timeout ────────▶│
   │                          │  (background, throttled)     │
   │                          │  (retry con jitter)          │
   │                          │◀─────── exito ────────────── │
   │                          ├─ rename .pending → .uploaded │
   │                          ├─ WS drive_sync_result ──────▶│
   │  UI: ☁️ sincronizado     │                              │
   │                          │                              │
   │              ... o si falla definitivamente ...          │
   │                          │◀─────── fallo x5 ─────────── │
   │                          ├─ .pending se mantiene        │
   │                          ├─ WS drive_sync_result ──────▶│
   │  UI: ☁️❌ pendiente      │  (success: false)           │
```

**Manejo en el cliente**:

- `TrackingService.cs`: nuevo evento `OnDriveSyncResult(string filename, bool success, string driveUrl)`
- En `ReceiveLoop`, parsear `type == "drive_sync_result"` y disparar el evento
- `CameraPanel.razor`: suscribirse a `OnDriveSyncResult`, actualizar icono de estado

### Decision 5: Cambios en la UI

**Textos**:
| Antes | Despues | Momento |
|-------|---------|---------|
| "Subiendo a Drive..." | "Guardando..." | Durante la captura y envio al servidor |
| "Foto subida" | "Foto guardada" | Cuando llega el ACK local |
| (no existia) | Icono ☁️ con tooltip "Sincronizando..." | Entre ACK local y drive_sync_result |
| (no existia) | Icono ✅ verde con tooltip "Sincronizada con Drive" | Al recibir drive_sync_result success |
| (no existia) | Icono ❌ con tooltip "Solo guardada localmente" | Al recibir drive_sync_result failure |

**Implementacion del icono**: Un `<span>` con clase CSS que muestra un emoji/unicode (☁️ ✅ ❌) segun `_driveSyncState` (enum: `Pending`, `Synced`, `Failed`). Sin dependencia de librerias externas.

**Modelo en CameraPanel.razor**:

```csharp
private enum DriveSyncState { None, Pending, Synced, Failed }
private DriveSyncState _driveSyncState = DriveSyncState.None;
private string _lastDriveUrl = "";
```

### Decision 6: Escaneo de pendientes al iniciar

Al arrancar `server.py`, antes de aceptar conexiones, se ejecuta `resume_pending_uploads()`:

```python
async def resume_pending_uploads():
    photos_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "FotosAvatares"))
    if not os.path.isdir(photos_dir):
        return
    
    pending_files = []
    for f in os.listdir(photos_dir):
        if f.endswith(".png.pending"):
            png_name = f[:-8]  # quitar ".pending"
            png_path = os.path.join(photos_dir, png_name)
            pending_path = os.path.join(photos_dir, f)
            uploaded_path = png_path + ".uploaded"
            # Si ya tiene .uploaded, limpiar el .pending huerfano
            if os.path.isfile(uploaded_path):
                os.remove(pending_path)
            elif os.path.isfile(png_path):
                pending_files.append((png_path, png_name))
    
    if pending_files:
        print(f"[Resume] {len(pending_files)} uploads pendientes encontrados")
    
    semaphore = asyncio.Semaphore(2)
    async def upload_one(png_path, png_name):
        async with semaphore:
            with open(png_path, "rb") as f:
                img_bytes = f.read()
            success = await upload_to_drive_with_retry(img_bytes, png_name)
            if success:
                pending_path = png_path + ".pending"
                uploaded_path = png_path + ".uploaded"
                os.rename(pending_path, uploaded_path)
    
    tasks = [asyncio.create_task(upload_one(p, n)) for p, n in pending_files]
    # No esperar: fire-and-forget en background
```

**Nota**: Las tareas de resumen se lanzan como fire-and-forget despues de iniciar el servidor. No bloquean el arranque. El `drive_sync_result` de estas fotos reanudadas no se envia al cliente (no hay cliente conectado que las haya originado). Si en el futuro se quiere, se puede asociar un `client_id` al marcador `.pending`.

## Riesgos y trade-offs

| Riesgo | Mitigacion |
|--------|-----------|
| **Marcadores huerfanos**: `.pending` sin `.png` | `resume_pending_uploads()` verifica que el PNG exista |
| **Marcadores duplicados**: `.pending` + `.uploaded` coexisten | `resume_pending_uploads()` borra `.pending` si `.uploaded` existe |
| **Race condition**: dos uploads procesan el mismo archivo | `asyncio.Semaphore(2)` + el hecho de que `os.rename` es atomico |
| **Espacio en disco**: `.pending` y `.uploaded` son archivos vacios (0 bytes). El impacto es negligible (~0 bytes por foto) |
| **Read timeout de 60s insuficiente**: para PNG > 5MB en conexion muy lenta | Los PNG del avatar son ~200-500KB. 60s a 3.3KB/s es suficiente. Si se excede, el retry lo reintenta |
| **Notificacion diferida no llega**: WebSocket se cae entre ACK local y drive_sync_result | El cliente muestra "pendiente" (icono ☁️). En la proxima foto, el handler de screenshot_result puede disparar un refresh de estado |

## Diagrama de arquitectura final

```
┌───────── Blazor (Cliente) ─────────────────────────────────────────┐
│                                                                      │
│  CameraPanel.razor                                                   │
│  ├── _captureStatus: "Guardando..." / "Foto guardada"                │
│  ├── _driveSyncState: None → Pending → Synced/Failed                │
│  ├── Iconos: ✅ local / ☁️ sincronizando / ✅☁️ sincronizada         │
│  └── Suscripciones: OnScreenshotResult + OnDriveSyncResult           │
│                                                                      │
│  TrackingService.cs                                                  │
│  ├── OnScreenshotResult(bool, string)   ← existente                  │
│  └── OnDriveSyncResult(string, bool, string)  ← NUEVO               │
│                                                                      │
└──────────────────────┬───────────────────────────────────────────────┘
                       │ WebSocket
                       ▼
┌───────── Python (Server) ───────────────────────────────────────────┐
│                                                                      │
│  server.py                                                           │
│  ├── main():                                                         │
│  │   └── asyncio.create_task(resume_pending_uploads())  ← NUEVO     │
│  │                                                                   │
│  ├── handle_connection():                                            │
│  │   ├── save PNG local + crear .pending              ← MODIFICADO  │
│  │   ├── ACK screenshot_result (ya existente)                        │
│  │   ├── create_task(upload_with_notification(...))   ← MODIFICADO  │
│  │   └── al completar: enviar drive_sync_result       ← NUEVO       │
│  │                                                                   │
│  └── upload_with_notification():                      ← NUEVO       │
│      ├── upload_to_drive_with_retry(img, filename)                   │
│      ├── si exito: .pending → .uploaded + notificar                  │
│      └── si fallo: notificar fallo                                   │
│                                                                      │
│  google_drive.py                                                     │
│  ├── _build_drive_service(): timeout 30/60            ← MODIFICADO  │
│  ├── upload_to_drive_with_retry(): jitter backoff     ← MODIFICADO  │
│  ├── resume_pending_uploads(): escanear + reintentar  ← NUEVO       │
│  │   └── Semaphore(2) throttling                                     │
│  └── mark_uploaded() / clear_pending(): helpers       ← NUEVO       │
│                                                                      │
└──────────────────────┬───────────────────────────────────────────────┘
                       │ HTTPS (timeout 30/60)
                       ▼
┌───────── Google Drive API ──────────────────────────────────────────┐
│  drive.files().create()                                              │
│  ├── resumable=True                                                  │
│  └── timeout: connect=30s, read=60s                                  │
└──────────────────────────────────────────────────────────────────────┘

┌───────── FotosAvatares/ (Filesystem) ───────────────────────────────┐
│                                                                      │
│  avatar-20260606-143022.png          ← PNG (siempre)                │
│  avatar-20260606-143022.png.pending  ← 0 bytes, pendiente de subir  │
│  avatar-20260606-143030.png                                          │
│  avatar-20260606-143030.png.uploaded ← 0 bytes, ya subido a Drive   │
│  avatar-20260606-142000.png                                          │
│  avatar-20260606-142000.png.pending  ← quedo pendiente, se reanuda  │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```
