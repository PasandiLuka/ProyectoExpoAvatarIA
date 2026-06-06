## Why

Se necesita construir un sistema interactivo de avatar IA para una expo tecnológica, donde un personaje 2D renderizado en el navegador imite en tiempo real los movimientos y expresiones faciales del usuario capturados por webcam. El proyecto debe priorizar baja latencia, simulación 3D sutil del torso, rigidez anatómica de brazos, y un panel completo de personalización visual (ropa, accesorios, colores). Ya existe un prototipo HTML funcional y un diseño de arquitectura documentado; ahora se requiere la implementación completa en Blazor WebAssembly (.NET 10) con backend Python/MediaPipe.

## What Changes

- Crear proyecto Blazor WebAssembly (.NET 10) con estructura de componentes para el avatar y paneles de UI
- Implementar servidor Python con MediaPipe Heavy que recibe frames JPEG por WebSocket y devuelve landmarks procesados
- Integrar captura de webcam vía JS interop, compresión de frames y transmisión por WebSocket nativo
- Implementar cinemática inversa basada en ángulos (Math.Atan2) para brazos rígidos con tamaño fijo
- Implementar efecto espejo (inversión de coordenada X), rotación 3D sutil del torso vía CSS perspective + rotateY, y detección de 3 expresiones faciales (sonrisa, sorpresa, enojo)
- Sistema de calibración por usuario ("Punto Cero") para normalizar umbrales de expresiones y postura
- Suavizado de movimiento con interpolación Lerp/exponencial para eliminar jitter de MediaPipe
- Protocolo de reconexión WebSocket con heartbeat (ping/pong 5s), backoff exponencial, y throttling adaptativo de FPS
- Panel de personalización con AvatarState (Singleton Blazor): estilos de pelo (3) + color, remeras (4), gorros (3), anteojos (3), objetos en manos (3 por lado)
- Soporte para WSS/HTTPS en producción y fallback visual cuando no hay cámara disponible
- Layout grid 3 columnas (20%-60%-20%) con Tailwind CSS + CSS nativo para transformaciones 3D

## Capabilities

### New Capabilities
- `websocket-tracking`: Comunicación cliente-servidor vía WebSocket para transmisión de frames JPEG y recepción de landmarks de pose/rostro
- `avatar-rendering`: Renderizado 2D del avatar con cinemática de brazos rígida, rotación 3D del torso, efecto espejo, y expresiones faciales dinámicas
- `camera-capture`: Captura de webcam vía getUserMedia, extracción de frames a canvas, compresión JPEG y envío al servidor
- `movement-smoothing`: Suavizado de movimiento con interpolación Lerp/filtro exponencial para eliminar jitter de MediaPipe
- `user-calibration`: Sistema de calibración de postura neutra por usuario para normalizar umbrales de expresiones y rotaciones
- `avatar-customization`: Panel de personalización visual del avatar (pelo, remeras, gorros, anteojos, objetos en manos) con estado reactivo
- `connection-resilience`: Reconexión automática con heartbeat, backoff exponencial, throttling adaptativo de FPS, y fallback sin cámara

### Modified Capabilities
<!-- No existing capabilities to modify -->

## Impact

- **Nuevo proyecto Blazor WASM**: `AvatarExpo/` con .NET 10, componentes Razor, JS interop para webcam
- **Nuevo servidor Python**: `server/` con `websockets`, `mediapipe`, `opencv-python`, `numpy`
- **Configuración SSL**: Certificados (autofirmados para dev) para WSS y HTTPS
- **Tailwind CSS**: Integrado vía CDN o build step en el index.html del proyecto Blazor
- **Dependencias NuGet**: Ninguna adicional crítica; WebSocket se maneja con `System.Net.WebSockets` nativo de .NET
- **Dependencias Python**: `mediapipe`, `websockets`, `opencv-python`, `numpy`
