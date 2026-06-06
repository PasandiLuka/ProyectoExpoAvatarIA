# WebSocket Tracking Specification

## MODIFIED Requirements

### Requirement: Procesamiento de frames binarios JPEG
El servidor SHALL procesar correctamente los mensajes binarios WebSocket que contienen frames JPEG. El loop de recepción SHALL distinguir entre mensajes de texto (ping/pong) y mensajes binarios (frames), aplicando `continue` solo a mensajes de texto.

#### Scenario: Recepción de frame JPEG binario
- **WHEN** el cliente envía un mensaje binario WebSocket conteniendo un frame JPEG
- **THEN** el servidor decodifica el JPEG con OpenCV, lo procesa con MediaPipe Pose y Face Mesh, y responde con un JSON conteniendo landmarks de pose y expresión facial

#### Scenario: Recepción de ping
- **WHEN** el cliente envía el mensaje de texto "ping"
- **THEN** el servidor responde con "pong" y continúa esperando el siguiente mensaje sin intentar decodificarlo como JPEG

### Requirement: Payload JSON extendido con datos de esqueleto
El servidor SHALL incluir en el JSON de respuesta un campo `"skeleton"` que contenga:
- `"landmarks"`: array de 33 arrays `[x, y, z]` con las coordenadas normalizadas de todos los landmarks de pose
- `"connections"`: array de pares `[from_idx, to_idx]` que definen las líneas del esqueleto según la topología de MediaPipe Pose
- `"width"`: ancho del frame original en píxeles
- `"height"`: alto del frame original en píxeles

El campo `"skeleton"` SHALL ser `null` cuando no se detecta pose.

#### Scenario: Pose detectada
- **WHEN** `pose_result.pose_landmarks` no es `None`
- **THEN** el JSON incluye `"skeleton"` con los 33 landmarks, las conexiones, y las dimensiones del frame

#### Scenario: Sin pose detectada
- **WHEN** `pose_result.pose_landmarks` es `None`
- **THEN** el JSON incluye `"skeleton": null`
