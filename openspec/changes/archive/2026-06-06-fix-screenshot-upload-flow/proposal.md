## Why

El botón "Sacar Foto" tiene dos bugs críticos: (1) cuando el WebSocket no está conectado (modo demo o pérdida de conexión), la UI se congela permanentemente en "Subiendo a Drive..." sin posibilidad de recuperación, y (2) no existe feedback de error para el usuario ante fallas. Además, el WebSocket solo se conecta al iniciar tracking, lo que impide subir fotos en modo demo. Se requiere también un countdown configurable antes de la captura para que el usuario pueda posicionarse.

## What Changes

- **WebSocket siempre conectado**: `TrackingService.Connect()` se llama al iniciar la app, no solo al presionar "Iniciar Tracking". El tracking de cámara sigue controlado por el botón independiente.
- **SendScreenshot** retorna `Task<bool>` para que el caller pueda reaccionar ante fallas de conexión
- **TakeScreenshot** maneja timeout de seguridad y feedback de error (nunca se congela la UI)
- **Countdown configurable**: delay de 5s por defecto antes de capturar, ajustable con slider (0-10s) en el menú hamburguesa inferior
- **Control de delay deshabilitado en modo demo**: solo tiene sentido cuando el usuario se está moviendo frente a la cámara

## Capabilities

### New Capabilities
- `screenshot-upload`: Captura de avatar vía html2canvas con countdown configurable, subida a Google Drive vía WebSocket (siempre conectado), y manejo robusto de errores con feedback visual.

### Modified Capabilities

## Impact

- `AvatarExpo/Services/TrackingService.cs` — `SendScreenshot` cambia a `Task<bool>`, `Connect()` se llama en inicialización de la app
- `AvatarExpo/Components/CameraPanel.razor` — `TakeScreenshot` con countdown, timeout, manejo de errores; slider de delay en menú hamburguesa
- `AvatarExpo/Program.cs` — inicialización temprana de la conexión WebSocket (si aplica)
