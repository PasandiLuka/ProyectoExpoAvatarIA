## Context

El cambio `fix-screenshot-upload-flow` hizo que el WebSocket se conecte al iniciar la app. Esto rompió los sliders demo porque `AvatarRenderer` usaba `TrackingService.IsConnected` como proxy de "estamos en modo demo".

```
Antes:  IsConnected=false  → aplica valores demo  ✓
Ahora:  IsConnected=true   → nunca aplica valores demo  ✗
```

## Goals / Non-Goals

**Goals:**
- Valores demo se aplican cuando no hay tracking activo, sin depender del WebSocket
- Valores demo se aplican inmediatamente al mover sliders
- Los landmarks reales tienen prioridad sobre los valores demo
- Al detener la cámara (sin desconectar WebSocket), los sliders demo vuelven a funcionar

**Non-Goals:**
- No se modifica `TrackingService`, `CameraPanel`, ni ningún otro componente
- No se cambia la arquitectura de cómo fluyen los landmarks

## Decisions

### 1. Flag local `_landmarksActive` + timer de inactividad

**Alternativa considerada:** Usar `TrackingService.IsDemoMode` (ya existe pero refleja WebSocket, no tracking).
**Alternativa considerada:** Escuchar `OnTrackingStateChanged` (solo se dispara en cambios de WebSocket, no de cámara).
**Decisión:** Flag local `_landmarksActive` que se pone `true` en cada `HandleLandmarks()` y se resetea a `false` tras 2 segundos sin landmarks vía `System.Threading.Timer`.

```
HandleLandmarks() → _landmarksActive = true, reiniciar timer de 2s
Timer expira      → _landmarksActive = false (ya no llegan landmarks)
Mover slider demo → HandleAvatarStateChange() → if (!_landmarksActive) aplicar valores
```

Esto cubre todos los casos:
- Tracking activo → landmarks llegan cada ~33ms → `_landmarksActive = true` → sliders ignorados
- Tracking detenido (cámara apagada) → dejan de llegar landmarks → timer expira → `_landmarksActive = false` → sliders funcionan
- App recién iniciada → `_landmarksActive = false` (inicial) → sliders funcionan
- WebSocket cae → `HandleTrackingState(false)` → `ResetToNeutral()` + `_landmarksActive = false`

### 2. Mínima invasión

Solo se cambia una línea en `HandleAvatarStateChange()` y se agregan ~10 líneas para el flag + timer. Sin nuevos eventos, sin acoplamiento adicional.

## Risks / Trade-offs

- **Timer de 2s: latencia al cambiar de tracking a demo** → el usuario nota 2s de delay entre apagar tracking y que los sliders respondan. Aceptable: es el tiempo de "drenaje" del buffer de frames.
- **Timer no se dispara en el thread de Blazor** → el callback del timer usa `InvokeAsync(StateHasChanged)` para actualizar la UI correctamente.
