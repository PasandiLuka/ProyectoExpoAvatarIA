## Why

Actualmente, la unica forma de persistir las fotos del avatar es mediante la subida a Google Drive. Este flujo tiene multiples puntos de falla: el upload a Drive es sincrono y bloquea la respuesta al cliente por 1-5 segundos, depende de conectividad a internet, de la validez del token OAuth (que puede expirar o requerir re-autenticacion via navegador), y de la disponibilidad de la API de Google. El resultado es una experiencia donde la subida de fotos es inconsistente: a veces funciona, a veces se cuelga, a veces falla silenciosamente.

El usuario necesita que las fotos se guarden de forma inmediata y confiable en el disco local del servidor, independientemente del estado de la red. El upload a Google Drive debe pasar a ser una operacion asincrona en segundo plano, sin bloquear la respuesta al cliente.

## What Changes

- **Guardado local inmediato**: Al recibir un screenshot, el servidor guarda la imagen PNG en `FotosAvatares/avatar-YYYYMMDD-HHMMSS.png` en la raiz del proyecto antes de cualquier otra operacion.
- **Respuesta instantanea al cliente**: El WebSocket responde con exito apenas se completa el guardado local (milisegundos). La URL devuelta referencia la ruta local: `local:FotosAvatares/...`.
- **Upload a Drive en background**: El upload a Google Drive se ejecuta de forma asincrona como tarea de fondo, sin bloquear el event loop del servidor ni la respuesta al cliente.
- **Reintentos con backoff**: Si el upload a Drive falla, se reintenta hasta 3 veces con backoff exponencial (2s, 4s, 8s). Los fallos se loguean pero no afectan al usuario.
- **Directorio versionado**: La carpeta `FotosAvatares/` se agrega a `.gitignore` y se crea automaticamente al iniciar el servidor si no existe.

## Capabilities

### Modified Capabilities

- `drive-upload`: El modulo `google_drive.py` gana un wrapper asincrono con reintentos. El upload deja de ser el unico destino de almacenamiento y pasa a ser secundario.
- `avatar-screenshot`: El flujo de screenshot en `server.py` se reestructura: guardado local → respuesta inmediata → upload a Drive en background.

## Impact

- `server/server.py` — reestructurar handler de mensajes `type: "screenshot"`: guardar local primero, responder inmediatamente, lanzar tarea asincrona de upload a Drive
- `server/google_drive.py` — nueva funcion `upload_to_drive_with_retry()` asincrona con reintentos exponenciales
- `.gitignore` — agregar `FotosAvatares/`
- `AvatarExpo/Components/CameraPanel.razor` — ajustar handler de respuesta para aceptar URL local (sin impacto funcional, el mensaje "Foto subida" sigue igual)
