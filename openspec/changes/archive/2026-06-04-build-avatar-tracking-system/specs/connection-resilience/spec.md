# Connection Resilience Specification

## ADDED Requirements

### Requirement: Heartbeat ping/pong cada 5 segundos
El cliente Blazor SHALL enviar un mensaje de ping (texto "ping") por el WebSocket cada 5 segundos. Si no recibe respuesta "pong" en 3 segundos, SHALL considerar la conexión como perdida e iniciar el proceso de reconexión.

#### Scenario: Conexión saludable
- **WHEN** el cliente envía "ping" y recibe "pong" en menos de 3 segundos
- **THEN** la conexión se considera activa y el tracking continúa normalmente

#### Scenario: Timeout del heartbeat
- **WHEN** el cliente envía "ping" y no recibe "pong" en 3 segundos
- **THEN** el cliente marca la conexión como perdida, detiene el envío de frames, y comienza el backoff de reconexión

### Requirement: Reconexión con backoff exponencial
Al perder la conexión, el cliente SHALL intentar reconectarse automáticamente con intervalos de 1s, 2s, 4s, 8s, 16s, y luego cada 16s hasta un máximo de 10 intentos. Tras 5 intentos fallidos, SHALL mostrar un indicador visual "Reconectando..." en la UI. Tras 10 intentos, SHALL mostrar "Conexión perdida - Verificá el servidor".

#### Scenario: Micro-corte de red de 3 segundos
- **WHEN** la red se interrumpe brevemente y se restablece en 3 segundos
- **THEN** el cliente reconecta en el segundo intento (a los ~2s), el tracking se reanuda, y el usuario apenas percibe la interrupción

#### Scenario: Servidor caído por tiempo prolongado
- **WHEN** el servidor está inactivo por más de 30 segundos
- **THEN** tras 5 intentos fallidos se muestra "Reconectando..." y tras 10 intentos se muestra "Conexión perdida - Verificá el servidor"

### Requirement: Throttling adaptativo de FPS
El cliente SHALL monitorear el round-trip time (RTT) de los mensajes WebSocket y ajustar la tasa de envío de frames según: RTT < 50ms → 30 FPS, RTT 50-100ms → 24 FPS, RTT 100-200ms → 20 FPS, RTT > 200ms → 15 FPS.

#### Scenario: Red congestionada durante la expo
- **WHEN** el RTT supera 100ms de forma sostenida
- **THEN** el cliente reduce el envío a 20 FPS para no saturar aún más la red

#### Scenario: Red se normaliza
- **WHEN** el RTT vuelve a bajar de 50ms tras un período de congestión
- **THEN** el cliente restablece el envío a 30 FPS

### Requirement: WSS en producción
En entornos que no sean localhost, el cliente SHALL conectarse usando `wss://` (WebSocket Secure) en lugar de `ws://`. La URL del WebSocket SHALL ser configurable (appsettings.json o variable de entorno).

#### Scenario: Aplicación desplegada en red local de la expo
- **WHEN** la URL configurada usa `wss://` y el servidor tiene un certificado SSL (autofirmado)
- **THEN** la conexión WebSocket se establece de forma segura y la cámara web funciona sin errores de Mixed Content

#### Scenario: Desarrollo local
- **WHEN** la URL configurada es `ws://localhost:8765`
- **THEN** la conexión WebSocket funciona sin SSL (localhost es considerado contexto seguro por los navegadores)

### Requirement: Fallback visual cuando no hay tracking
Si la cámara no está disponible o la conexión WebSocket no puede establecerse, la aplicación SHALL funcionar en modo offline mostrando el avatar en posición neutra con los sliders de simulación manual habilitados y el panel de personalización completamente operativo.

#### Scenario: Usuario abre la app sin cámara conectada
- **WHEN** `getUserMedia` falla porque no hay dispositivo de video
- **THEN** el avatar se muestra en posición neutra, los sliders de "Simulador" en el panel izquierdo están habilitados, y el panel de personalización funciona normalmente

#### Scenario: Cámara disponible pero servidor inalcanzable
- **WHEN** la cámara se activa correctamente pero el WebSocket no logra conectarse tras 3 intentos
- **THEN** se muestra un aviso "Servidor no disponible - Usando modo demo", el avatar queda en posición neutra, y los sliders manuales permiten mover el avatar
