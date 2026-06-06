## Context

El flujo actual de screenshot tiene tres problemas raíz:

1. `TrackingService.SendScreenshot()` retorna `void` y hace `return` silencioso cuando `_ws` no está abierto. La UI queda congelada.
2. El WebSocket solo se conecta al presionar "Iniciar Tracking", bloqueando screenshots en modo demo.
3. No hay countdown pre-captura, forzando al usuario a correr a posicionarse.

El servidor Python (`server.py`) y `google_drive.py` ya fueron verificados funcionalmente. El cambio es exclusivamente cliente Blazor WASM.

## Goals / Non-Goals

**Goals:**
- WebSocket conectado desde el inicio de la app (independiente del tracking de cámara)
- `SendScreenshot` retorna `Task<bool>` con feedback al caller
- Countdown configurable (default 5s) antes de la captura para que el usuario pose
- Slider de delay (0-10s) en el menú hamburguesa inferior, deshabilitado en modo demo
- UI nunca congelada: timeout de seguridad + feedback de error en todos los paths

**Non-Goals:**
- No se modifica el servidor Python ni `google_drive.py`
- No se agregan nuevos endpoints o protocolos de red
- El tracking de cámara sigue iniciándose/deteniéndose con el botón existente

## Decisions

### 1. WebSocket siempre conectado

**Alternativa considerada:** Conectar solo al iniciar tracking, y usar un fallback de descarga local en demo.
**Decisión:** Conectar al iniciar la app. El servidor ya soporta conexiones idle (solo reacciona cuando recibe mensajes). Sin tracking activo, simplemente no se envían frames binarios y el WebSocket queda en espera para screenshots. Esto elimina la necesidad de cualquier fallback local.

La conexión se dispara desde `CameraPanel.OnInitializedAsync()` llamando a `TrackingService.Connect()`. El botón "Iniciar/Detener Tracking" solo controla el envío de frames de cámara, no el WebSocket.

### 2. Countdown pre-captura

**Alternativa considerada:** Captura inmediata al click.
**Decisión:** Countdown visual de N segundos (configurable) antes de disparar html2canvas. El botón muestra la cuenta regresiva: "Sacar Foto (3)", "Sacar Foto (2)", "Sacar Foto (1)", y al llegar a 0 ejecuta la captura. Un segundo click durante el countdown lo cancela, restaurando el botón a "Sacar Foto" y liberando la UI.

**Alternativa considerada:** Overlay full-screen con cuenta regresiva grande.
**Decisión:** Solo el texto del botón cambia. Más simple, no requiere overlay ni bloquear toda la UI.

### 3. Control de delay en menú hamburguesa

**Alternativa considerada:** Input numérico directo.
**Decisión:** Slider de 0 a 10 segundos (step 1) dentro del menú desplegable `≡ Tamaño Avatar`. Se agrega una sección debajo del control de escala:

```
≡ Tamaño Avatar
  ┌─────────────────────────┐
  │ Escala: 0.5 [===] 3.0   │
  │        2.0x             │
  │ [Auto activado]         │
  ├─────────────────────────┤
  │ Delay foto: 0 [====] 10 │  ← NUEVO
  │            5s           │
  └─────────────────────────┘
```

El slider se deshabilita en modo demo (`disabled="@(!IsTracking)"`). El valor por defecto es 5s.

### 4. Timeout de seguridad (mantenido del diseño anterior)

Un `CancellationTokenSource` con `Task.Delay(30000)` (30s, holgado por el countdown + subida) asegura que si el servidor nunca responde, la UI se recupera.

### 5. SendScreenshot retorna `Task<bool>`

`true` = mensaje enviado al socket. `false` = no se pudo enviar. El caller muestra error y libera el botón.

### 6. Desacoplar estado de WebSocket del estado de tracking

Actualmente `CameraPanel` deriva `IsDemoMode` del evento `OnTrackingStateChanged`, que refleja el estado del WebSocket, no del tracking. Esto causa un bug si el WebSocket se reconecta mientras la cámara está apagada: los sliders demo desaparecen.

**Decisión:** `CameraPanel` deja de suscribirse a `OnTrackingStateChanged`. Maneja `IsTracking`/`IsDemoMode` exclusivamente en `ToggleTracking()`. El WebSocket pasa a ser transparente para la UI de tracking.

**Impacto en TrackingService:** `IsDemoMode` e `OnTrackingStateChanged` se mantienen (otras partes pueden consumirlos), pero `CameraPanel` los ignora. `TryConnect()` y `ReceiveLoop()` ya no disparan cambios de UI.

### 7. Renombrar menú hamburguesa

El toggle actual dice `≡ Tamaño Avatar`. Como ahora también contiene el control de delay, se renombra a `≡ Ajustes`.

### 8. Botón deshabilitado sin conexión WebSocket

**Alternativa considerada:** Permitir el click y mostrar error después.
**Decisión:** El botón "Sacar Foto" se deshabilita cuando `_ws` no está conectado. Esto evita intentos fallidos y deja claro que la funcionalidad no está disponible. El botón se rehabilita automáticamente cuando el reconnect loop restablece la conexión.

Para que `CameraPanel` conozca el estado del WebSocket sin acoplarse al tracking, `TrackingService` expone `IsConnected` como propiedad pública (ya existe) y dispara `OnConnectionChanged` (nuevo evento separado de `OnTrackingStateChanged`). `CameraPanel` se suscribe para habilitar/deshabilitar el botón.

## Risks / Trade-offs

- **WebSocket idle puede desconectarse por timeout de red** → el reconnect loop de `TrackingService` ya maneja reconexión automática con backoff exponencial, cubriendo este caso
- **Countdown de 0s (instantáneo)** → el usuario puede setear delay=0 si no necesita posicionarse. Comportamiento equivalente al actual pero sin bugs
- **html2canvas falla** → el `catch` en `TakeScreenshot` muestra error y libera el botón
