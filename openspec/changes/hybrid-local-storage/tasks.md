## 1. Guardado local inmediato en el servidor

- [x] 1.1 Agregar `FotosAvatares/` a `.gitignore`
- [x] 1.2 En `server/server.py`, modificar el handler de `type: "screenshot"`:
  - Decodificar base64 a bytes (ya existe)
  - Resolver ruta: `os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "FotosAvatares"))`
  - Crear directorio con `os.makedirs(photos_dir, exist_ok=True)`
  - Guardar archivo: `open(local_path, "wb").write(img_bytes)`
  - Responder al cliente inmediatamente con `success: true, url: "local:FotosAvatares/..." + filename`
- [x] 1.3 Si el guardado local falla (IOError, PermissionError), responder con `success: false`

## 2. Upload a Drive asincrono con reintentos

- [x] 2.1 En `server/google_drive.py`, agregar funcion `async def upload_to_drive_with_retry(image_bytes, filename, max_retries=3)`:
  - Loop de `max_retries` intentos
  - Ejecutar `upload_to_drive()` via `loop.run_in_executor(None, ...)`
  - Si `result["success"]`, loguear exito y retornar
  - Si falla, esperar `2 ** (attempt + 1)` segundos y reintentar
  - Si todos los intentos fallan, loguear error final
- [x] 2.2 En `server/server.py`, despues de guardar localmente y responder al cliente, lanzar tarea de fondo: `asyncio.create_task(upload_to_drive_with_retry(img_bytes, filename))`

## 3. Robustez del flujo

- [x] 3.1 Verificar que `upload_to_drive()` maneja correctamente `run_in_executor` (no usa objetos del event loop actual)
- [x] 3.2 Asegurar que el handler de screenshot no se ve afectado por frames binarios concurrentes (el `continue` despues del handler de screenshot ya existe)
- [x] 3.3 Agregar log en el servidor cuando se guarda localmente: `print(f"[Screenshot] Guardado local: {local_path}")`

## 4. Verificacion

- [ ] 4.1 Probar flujo completo con servidor local: tomar foto → verificar que el PNG aparece en `FotosAvatares/`
- [ ] 4.2 Probar flujo con Drive configurado: verificar que la foto tambien aparece en Drive (puede tardar unos segundos)
- [ ] 4.3 Probar fallo de Drive: desconectar internet → tomar foto → verificar que el PNG se guarda local y el servidor loguea los reintentos fallidos
- [ ] 4.4 Probar reintentos: el servidor loguea "Intento 1 fallo... Reintentando en 2s..." cuando Drive falla transitoriamente
- [ ] 4.5 Probar respuesta rapida: el frontend muestra "Foto subida" en <1s (sin esperar a Drive)
- [x] 4.6 Verificar que `dotnet build` compila sin errores (el frontend practicamente no cambia)
