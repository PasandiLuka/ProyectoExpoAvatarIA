## 1. Dependencias y configuración base

- [x] 1.1 Agregar html2canvas vía CDN en `wwwroot/index.html` (`<script src="https://unpkg.com/html2canvas@1.4.1/dist/html2canvas.min.js">`)
- [x] 1.2 Agregar `google-api-python-client` y `google-auth` a `server/requirements.txt`
- [x] 1.3 Crear `server/credentials.json.example` como template para la Service Account (sin datos reales)
- [x] 1.4 Agregar `server/credentials.json` a `.gitignore`
- [x] 1.5 Agregar `DRIVE_FOLDER_ID` a la config del servidor (variable de entorno con fallback)

## 2. Captura del avatar en el frontend

- [x] 2.1 Agregar `id="avatar-wrapper"` al contenedor principal en `AvatarRenderer.razor`
- [x] 2.2 Agregar función `captureAvatarToBase64()` en `wwwroot/js/camera.js`: usa `html2canvas` sobre `#avatar-wrapper`, convierte a blob, retorna base64
- [x] 2.3 Agregar método C# `CaptureScreenshot()` en `CameraService.cs` que invoca la función JS y retorna el base64
- [x] 2.4 Agregar método `SendScreenshot(base64)` en `TrackingService.cs` que envía un mensaje JSON `{"type":"screenshot", ...}` por WebSocket

## 3. Botón y UI de captura

- [x] 3.1 Agregar botón "Sacar Foto" en `CameraPanel.razor` con ícono/emoji
- [x] 3.2 Agregar estado `_isCapturing` (bool) y `_captureStatus` (string: idle/processing/uploading/success/error)
- [x] 3.3 Agregar overlay/indicador visual de progreso (spinner + texto de estado)
- [x] 3.4 Agregar método `async Task TakeScreenshot()`: llama a `CaptureScreenshot()` → `SendScreenshot()` → espera respuesta del servidor
- [x] 3.5 Agregar handler para recibir mensaje `{"type":"screenshot_result"}` en el loop de mensajes WebSocket de `TrackingService.cs`

## 4. Módulo Google Drive en el servidor Python

- [x] 4.1 Crear `server/google_drive.py` con función `upload_to_drive(image_bytes: bytes, filename: str) -> str`
- [x] 4.2 Implementar autenticación con Service Account usando `google.oauth2.service_account`
- [x] 4.3 Implementar upload via `googleapiclient.discovery.build('drive', 'v3')`: `files().create()` con `parents=[DRIVE_FOLDER_ID]`
- [x] 4.4 Manejar errores de autenticación y upload (retornar dict con success/error/url)
- [x] 4.5 Cargar `DRIVE_FOLDER_ID` desde variable de entorno `DRIVE_FOLDER_ID` con fallback a valor hardcodeado para desarrollo

## 5. Endpoint de screenshot en el WebSocket server

- [x] 5.1 En `server/server.py`, agregar handler para mensajes JSON con `"type": "screenshot"`
- [x] 5.2 Decodificar base64 a bytes, generar filename con timestamp: `avatar-YYYYMMDD-HHMMSS.png`
- [x] 5.3 Llamar a `google_drive.upload_to_drive(bytes, filename)`
- [x] 5.4 Enviar respuesta al cliente: `{"type":"screenshot_result", "success": true/false, "url": "..."}`
- [x] 5.5 No bloquear el loop de tracking: el upload a Drive es async, no debe interferir con el stream de landmarks

## 6. Integración y flujo completo

- [x] 6.1 Conectar el `CameraPanel` → `CameraService.CaptureScreenshot()` → JS html2canvas → base64
- [x] 6.2 Conectar base64 → `TrackingService.SendScreenshot()` → WebSocket → Python handler
- [x] 6.3 Conectar Python handler → `google_drive.upload_to_drive()` → respuesta → WebSocket → `TrackingService` → UI feedback
- [x] 6.4 Probar flujo completo: botón → captura → upload → confirmación en UI

## 7. Verificación

- [x] 7.1 Compilar Blazor: `dotnet build` — 0 errores, 0 warnings
- [x] 7.2 Probar server: `python server.py` arranca sin errores, WebSocket funciona
- [x] 7.3 Probar captura: abrir app, customizar avatar, clickear "Sacar Foto" → se genera PNG correcto del estado actual
- [x] 7.4 Probar upload: verificar que la imagen aparece en la carpeta de Google Drive configurada
- [x] 7.5 Probar errores: desconectar internet → click "Sacar Foto" → se muestra mensaje de error
- [x] 7.6 Probar edge cases: avatar sin cabeza/brazos (demo mode), avatar con fondo transparente, avatar con items equipados
