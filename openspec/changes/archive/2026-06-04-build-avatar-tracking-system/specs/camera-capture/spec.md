# Camera Capture Specification

## ADDED Requirements

### Requirement: Acceso a cámara web via getUserMedia
La aplicación Blazor SHALL solicitar acceso a la cámara web del usuario mediante `navigator.mediaDevices.getUserMedia` con resolución mínima 640x480 y 30 FPS, invocado desde JavaScript interop.

#### Scenario: Usuario otorga permiso de cámara
- **WHEN** el usuario hace clic en "Iniciar Tracking" y el navegador muestra el diálogo de permisos
- **THEN** tras aceptar, se activa el stream de video y comienza la captura de frames

#### Scenario: Usuario deniega permiso de cámara
- **WHEN** el usuario rechaza el permiso de cámara en el diálogo del navegador
- **THEN** el sistema entra en modo demo/fallback, mostrando el avatar en posición neutra con sliders manuales habilitados

### Requirement: Extracción de frames a canvas oculto
Un script JavaScript SHALL extraer cada frame del stream de video a un elemento `<canvas>` oculto y convertirlo a JPEG comprimido (calidad 60-70%) antes de enviarlo al servidor.

#### Scenario: Frame capturado y comprimido
- **WHEN** el stream de video está activo a 30 FPS
- **THEN** cada ~33ms se dibuja el frame actual en un canvas oculto, se extrae como JPEG con calidad 0.6, y se coloca en la cola de envío

### Requirement: Conversión a ArrayBuffer para envío binario
El frame JPEG comprimido SHALL convertirse a `ArrayBuffer` (no Base64) para minimizar el tamaño del payload antes de enviarlo por WebSocket.

#### Scenario: Frame listo para enviar
- **WHEN** un frame JPEG ha sido extraído del canvas
- **THEN** el blob JPEG se convierte a ArrayBuffer y se envía como mensaje binario por el WebSocket

### Requirement: Control de frame rate configurable
El sistema SHALL permitir configurar la tasa de envío de frames (15, 20, 24, 30 FPS) y SHALL respetar el throttling adaptativo indicado por el estado de la conexión.

#### Scenario: Conexión con buena latencia
- **WHEN** el round-trip time es menor a 50ms
- **THEN** los frames se envían a 30 FPS

#### Scenario: Conexión congestionada
- **WHEN** el round-trip time supera 100ms
- **THEN** el sistema reduce automáticamente el envío a 20 FPS
