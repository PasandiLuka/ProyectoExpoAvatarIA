## Context

El proyecto tiene un servidor Python (`server/server.py`) que actua como puente entre el frontend Blazor WASM y Google Drive. El flujo actual de screenshot es:

```
Cliente → WS mensaje "screenshot" (base64 PNG)
         → server.py recibe, llama a upload_to_drive() [SINCRONO, bloquea 1-5s]
         → responde al cliente con success/failure
```

`upload_to_drive()` (en `server/google_drive.py`) es sincrono: obtiene credenciales OAuth (posiblemente abriendo navegador), construye el cliente de Drive API, y ejecuta `files().create()`. Todo esto bloquea el event loop de asyncio.

El servidor ya tiene capacidad de escribir en disco (la funcion `_get_credentials()` ya maneja archivos). Agregar escritura de archivos PNG es trivial.

El frontend (CameraPanel.razor) tiene un timeout de 30 segundos esperando la respuesta del servidor. Con el nuevo flujo, la respuesta llega en <100ms, por lo que el timeout deja de ser un problema.

## Goals / Non-Goals

**Goals:**
- Guardar cada screenshot como PNG en `FotosAvatares/` en la raiz del proyecto inmediatamente al recibirlo
- Responder al cliente con exito apenas se complete el guardado local (sin esperar a Drive)
- Ejecutar el upload a Google Drive como tarea asincrona de fondo (non-blocking)
- Reintentar uploads fallidos a Drive: 3 intentos con backoff 2s/4s/8s
- Crear automaticamente el directorio `FotosAvatares/` si no existe
- Agregar `FotosAvatares/` a `.gitignore`

**Non-Goals:**
- Servir las fotos via HTTP (no se agrega file server)
- Limpieza automatica de fotos viejas (se acumulan, limpieza manual)
- Modificar el pipeline de captura de pantalla (html2canvas/getDisplayMedia)
- Notificar al cliente el resultado del upload a Drive (es background, solo logs)
- Galeria de fotos en el frontend
- Soportar upload desde multiples clientes simultaneos

## Decisions

### Decision 1: Guardado local en `FotosAvatares/`

**Ubicacion**: Raiz del proyecto (`/FotosAvatares/`), no dentro de `server/`. Desde el servidor la ruta se resuelve con `os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "FotosAvatares"))`.

**Formato de archivo**: PNG binario, mismo nombre que ya se genera: `avatar-YYYYMMDD-HHMMSS.png`.

**Creacion del directorio**: `os.makedirs(photos_dir, exist_ok=True)` al momento de guardar. No requiere inicializacion especial en el arranque.

**Permisos**: Hereda los permisos del directorio padre. En Linux, el umask del proceso define los permisos (tipicamente 644 para archivos).

### Decision 2: Drive upload asincrono con reintentos

El upload a Drive se mueve a una tarea de fondo usando `asyncio.create_task()`. Como `upload_to_drive()` es sincrono, se ejecuta via `loop.run_in_executor(None, ...)` para no bloquear el event loop.

Nueva funcion en `google_drive.py`:

```
async def upload_to_drive_with_retry(image_bytes, filename, max_retries=3):
    for attempt in range(max_retries):
        try:
            loop = asyncio.get_event_loop()
            result = await loop.run_in_executor(None, upload_to_drive, image_bytes, filename)
            if result["success"]:
                print(f"[Drive] Upload exitoso: {result['url']}")
                return True
        except Exception as e:
            if attempt < max_retries - 1:
                delay = 2 ** (attempt + 1)  # 2, 4, 8
                print(f"[Drive] Intento {attempt+1} fallo: {e}. Reintentando en {delay}s...")
                await asyncio.sleep(delay)
    print(f"[Drive] Upload fallo despues de {max_retries} intentos")
    return False
```

**Justificacion del backoff**: 2s/4s/8s es suficiente para manejar rate limits transitorios y micro-cortes de red. Tres intentos cubren la mayoria de fallos temporales sin esperar excesivamente (~14s total maximo).

**No se notifica al cliente**: El resultado del upload a Drive es solo para logs. El usuario ya recibio confirmacion de guardado local. Si en el futuro se quiere mostrar el estado de Drive, se puede agregar un mensaje WebSocket diferido.

### Decision 3: Formato de respuesta al cliente

El mensaje de respuesta cambia de:

```json
{"type": "screenshot_result", "success": true, "url": "https://drive.google.com/file/d/XXX/view"}
```

a:

```json
{"type": "screenshot_result", "success": true, "url": "local:FotosAvatares/avatar-20260606-143022.png"}
```

El prefijo `local:` permite al frontend distinguir entre URLs de Drive y paths locales en caso de que en el futuro se quiera mostrar o actuar de forma diferente.

**El frontend no necesita cambios**: `CameraPanel.razor` solo usa `url` para mostrar el texto "Foto subida" (no muestra la URL). El timeout de 30s sigue siendo valido como safety net, pero la respuesta ahora llega en <100ms.

### Decision 4: Manejo de errores

| Escenario | Comportamiento |
|-----------|---------------|
| Guardado local falla (disco lleno, permisos) | Responder al cliente con `success: false`. No intentar Drive. |
| Guardado local OK, Drive falla | Cliente ya recibio OK. Drive se reintenta en background. |
| Guardado local OK, Drive falla tras 3 reintentos | Log del error. Foto queda solo en local. |
| Base64 invalido | Responder al cliente con `success: false`. No guardar ni subir. |

## Risks / Trade-offs

- **[Riesgo bajo] Espacio en disco**: Sin politica de limpieza, `FotosAvatares/` crece indefinidamente. Un PNG de ~500KB x 1000 fotos = ~500MB. Aceptable para el hardware tipico de la expo. Se puede agregar limpieza en cambio futuro.
- **[Riesgo bajo] Fotos huerfanas en Drive**: Si el upload a Drive falla definitivamente, la foto queda solo en local. No hay mecanismo de reconciliacion. Para la expo esto es aceptable: local es la fuente primaria, Drive es backup.
- **[Riesgo bajo] Concurrencia**: `asyncio.create_task()` lanza la tarea y no espera. Si el usuario toma muchas fotos rapido, multiple tareas de Drive se ejecutaran concurrentemente. Esto es aceptable porque la API de Drive soporta cierto paralelismo y el backoff maneja rate limits.
- **[Trade-off] Sin feedback de Drive al usuario**: El usuario no sabe si la foto llego a Drive. Esto es intencional: la experiencia es mas fluida sin esperar. Si se necesita feedback, se puede agregar un evento WebSocket diferido en el futuro.
