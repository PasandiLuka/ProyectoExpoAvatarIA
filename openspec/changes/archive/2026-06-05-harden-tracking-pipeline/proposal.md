## Why

El pipeline de tracking tiene tres errores de lógica corregidos (offset -90°, head tracking, binding de sliders) pero carece de tolerancia a fallos. Si cualquier excepción no manejada ocurre en `HandleLandmarks` o `ReceiveLoop`, el pipeline muere en silencio: el esqueleto verde desaparece, el avatar se congela, y no hay diagnóstico visible. El `try-catch` de `ReceiveLoop` solo atrapa `OperationCanceledException` y `WebSocketException` — cualquier otra excepción escapa, el `Task.Run` falla sin que nadie lo observe (`_receiveTask` nunca es await-eado), y la UI nunca se entera.

Además hay un doble suavizado: el código C# aplica `Lerp(0.3)` Y el CSS aplica `transition: transform 0.1s`. Esto acumula ~100ms extra de latencia en cada frame, haciendo que el avatar "persiga" al usuario en vez de seguirlo.

No existe ningún mecanismo de logging o diagnóstico en runtime. Si el pipeline falla, solo se puede diagnosticar abriendo DevTools y buscando errores en consola.

## What Changes

- **Blindaje de `ReceiveLoop`**: Agregar `catch (Exception)` que loguee el error y fuerce reconexión en vez de morir silenciosamente.
- **Protección de `HandleLandmarks`**: Envolver en `try/catch` para que una excepción en el handler no reviente el loop completo.
- **Eliminar doble suavizado CSS**: Remover `transition: transform` de `.arm-container`, `.arm-lower`, y `.body-wrapper` — el `Lerp` en C# ya suaviza.
- **Logging de diagnóstico**: Agregar `Console.WriteLine` en puntos clave del pipeline (conexión, frames recibidos, landmarks procesados, ángulos finales) visible en DevTools.
- **Indicador visual de pipeline health**: Mostrar en el panel de cámara el estado real del pipeline (FPS, último error, si hay landmarks fluyendo) como texto pequeño debajo del feed.

## Impact

- **TrackingService.cs**: `ReceiveLoop` (líneas 92-154) — agregar catch(Exception) + logging; `HandleLandmarks` wrapper
- **AvatarRenderer.razor**: `HandleLandmarks` (línea 120) — try/catch protector
- **avatar.css**: Eliminar `transition: transform` de `.arm-container` (línea 110), `.arm-lower` (línea 135), `.body-wrapper` (línea 26)
- **CameraPanel.razor**: Agregar indicador de health debajo del canvas
