## Context

El pipeline de tracking (`camera.js` → `CameraService` → `TrackingService` → `AvatarRenderer`) fue corregido en su lógica de ángulos en el change `fix-avatar-tracking-and-add-outline`. Sin embargo, carece de tolerancia a fallos en runtime. El `try-catch` de `ReceiveLoop` es incompleto (solo `OperationCanceledException` y `WebSocketException`) y `_receiveTask` nunca es observado — si una excepción no atrapada escapa del loop, el `Task.Run` falla silenciosamente y el pipeline muere sin diagnóstico.

Además existe un doble suavizado: `Lerp(0.3)` en C# más `transition: transform 0.1s ease-out` en CSS. Para 30 FPS (~33ms por frame), el CSS agrega 100ms de latencia que el C# ya maneja con su propio interpolador.

## Goals / Non-Goals

**Goals:**
- Prevenir que excepciones en `HandleLandmarks` maten el pipeline de tracking completo
- Prevenir que excepciones no-WebSocket en `ReceiveLoop` maten el loop silenciosamente
- Eliminar el doble suavizado que causa latencia innecesaria
- Proveer logging mínimo en consola para diagnóstico en runtime
- Agregar indicador visual de pipeline health en la UI

**Non-Goals:**
- Refactorizar la arquitectura del pipeline (eso es un change separado)
- Agregar tests automatizados (change futuro)
- Modificar el servidor Python
- Agregar un sistema de logging estructurado (solo `Console.WriteLine`)

## Decisions

### Decision 1: Catch-all en ReceiveLoop con reconexion forzada

El `catch` actual solo cubre `OperationCanceledException` y `WebSocketException`. Se agrega un `catch (Exception)` al final que:
1. Escribe el error en `Console.WriteLine`
2. Dispara `OnStatusChanged` con el mensaje de error
3. Cierra el WebSocket actual
4. Llama a `ReconnectLoop()` para restablecer la conexion

Esto asegura que cualquier excepcion (incluyendo `NullReferenceException` por un handler buggy) sea contenida y el pipeline se recupere automaticamente.

```csharp
catch (OperationCanceledException) { break; }
catch (WebSocketException) { break; }
catch (Exception ex)
{
    Console.WriteLine($"[TrackingService] ReceiveLoop error: {ex.Message}");
    OnStatusChanged?.Invoke($"Error: {ex.Message}");
    break; // sale del while, cae en el bloque de reconexion
}
```

### Decision 2: Try/catch protector en HandleLandmarks

Actualmente `HandleLandmarks` se invoca directamente desde `OnLandmarksReceived?.Invoke(processed)`. Si lanza una excepcion, esta se propaga al caller (`ReceiveLoop`). Con la Decision 1 el loop se recupera, pero perdemos el frame actual.

Se envuelve el invoke en un try/catch dentro de `ReceiveLoop` para que un handler que lance no impida procesar frames futuros:

```csharp
if (data?.P != null)
{
    try
    {
        var processed = LandmarkParser.Parse(data);
        if (_calibration != null)
            processed = ApplyCalibration(processed);
        OnLandmarksReceived?.Invoke(processed);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[TrackingService] Handler error: {ex.Message}");
    }
}
```

### Decision 3: Eliminar CSS transitions, mantener solo C# Lerp

El `Lerp(current, target, 0.3)` en C# converge en ~3 frames (~100ms a 30fps). El CSS `transition: transform 0.1s ease-out` agrega otros 100ms. Resultado: ~200ms de latencia percibida, el avatar "persigue" al usuario.

Se eliminan las propiedades `transition: transform` de:
- `.body-wrapper` (linea 26)
- `.arm-container` (linea 110)
- `.arm-lower` (linea 135)

Se mantiene `transition: background 0.3s` en `.torso` (cambio de remera, no relacionado con tracking).

**Alternativa considerada**: Eliminar el Lerp de C# y usar solo CSS. Descartada porque el Lerp permite ajustar el factor de suavizado dinamicamente (ej: reducirlo cuando el RTT es bajo) y es mas predecible que las animaciones CSS en Blazor WASM.

### Decision 4: Console.WriteLine en puntos clave

Se agregan logs minimos en:
- `TrackingService.TryConnect()`: "Conectando a {url}..."
- `TrackingService.ReceiveLoop()`: "Tracking iniciado, esperando landmarks..."
- `TrackingService.ReceiveLoop()`: cada 60 frames (2 segundos a 30fps), loguear "Landmarks OK, FPS={fps}"
- `TrackingService.ReceiveLoop()`: en catch(Exception), loguear el error
- `AvatarRenderer.HandleLandmarks()`: si los angulos calculados son NaN o Infinity, loguear warning

Estos logs son visibles en DevTools → Console y no requieren infraestructura adicional.

### Decision 5: Indicador de pipeline health en CameraPanel

Debajo del canvas del feed de camara, donde ya se muestra "Tracking: Activo/Inactivo", se agrega una segunda linea con el estado real del pipeline:

```
Tracking: Activo   |   FPS: 28   |   Landmarks: OK
Tracking: Activo   |   FPS: 0    |   Landmarks: Sin datos
Tracking: Inactivo  |   Error: Connection refused
```

Esto se implementa con eventos existentes (`OnFpsChanged`, `OnStatusChanged`, `OnLandmarksReceived`) mas un flag booleano `_hasRecentLandmarks` que se resetea cada 3 segundos.

## Risks / Trade-offs

- **[Riesgo bajo] Console.WriteLine en produccion**: Los logs en consola son visibles para cualquier usuario que abra DevTools pero no afectan el rendimiento. En produccion se puede condicionar con `#if DEBUG`.
- **[Riesgo bajo] Eliminar transition CSS**: Sin la transicion CSS, los cambios de angulo son inmediatos frame a frame. El Lerp de C# (factor 0.3) ya provee suavizado. Si se percibe "stuttering", se puede aumentar el Lerp factor o reintroducir una transicion mas corta (0.05s).
- **[Riesgo bajo] Reconexion en catch(Exception)**: Si la excepcion es un error permanente (ej: bug en `LandmarkParser`), el loop entrara en ciclo reconectar-fallar-reconectar. El `ReconnectLoop` tiene backoff exponencial y maximo 10 intentos, por lo que eventualmente se detiene y muestra "Modo demo".
