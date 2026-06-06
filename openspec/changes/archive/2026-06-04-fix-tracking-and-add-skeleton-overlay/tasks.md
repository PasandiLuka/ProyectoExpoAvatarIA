## 1. Corregir indentación del `continue` en server.py

- [x] 1.1 Mover `continue` dentro del bloque `if isinstance(message, str)` (4 espacios más de indentación)

## 2. Agregar skeleton landmarks al servidor

- [x] 2.1 Definir constante `POSE_CONNECTIONS` con los pares de conexión de MediaPipe Pose
- [x] 2.2 Modificar `handle_connection` para extraer los 33 landmarks y construir el campo `skeleton`
- [x] 2.3 Incluir `skeleton` en el payload JSON (o `null` si no hay pose)

## 3. Agregar canvas overlay en CameraPanel

- [x] 3.1 Agregar `<canvas id="skeleton-canvas">` con `position: absolute` sobre el video en `CameraPanel.razor`
- [x] 3.2 El canvas debe tener `width="640"` y `height="480"`

## 4. Implementar drawSkeleton en camera.js

- [x] 4.1 Agregar función `window.avatarCamera.drawSkeleton(skeletonData)` que recibe `{landmarks, connections, width, height}`
- [x] 4.2 La función limpia el canvas, convierte coordenadas normalizadas a píxeles, dibuja líneas verdes y puntos

## 5. Wirear skeleton data del servidor al canvas

- [x] 5.1 Agregar campo `Skeleton` al modelo `LandmarkData` en `Models.cs`
- [x] 5.2 Agregar clase `SkeletonData` con `Landmarks`, `Connections`, `Width`, `Height`
- [x] 5.3 En `TrackingService.ReceiveLoop`, al recibir JSON con skeleton, llamar a JS `drawSkeleton`
- [x] 5.4 Inyectar `IJSRuntime` en `TrackingService` y actualizar `Program.cs`

## 6. Verificación

- [x] 6.1 Servidor Python: sintaxis corregida, lista para ejecutar
- [x] 6.2 Compilar `dotnet build` — 0 errores, 0 warnings
- [x] 6.3 Probar en navegador: al iniciar tracking, el avatar sigue los movimientos del usuario
- [x] 6.4 Probar en navegador: el esqueleto verde aparece sobre la vista de cámara, siguiendo la pose en tiempo real
