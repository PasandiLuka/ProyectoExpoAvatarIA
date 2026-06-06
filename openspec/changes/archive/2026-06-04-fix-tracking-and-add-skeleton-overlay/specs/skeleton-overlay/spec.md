# Skeleton Overlay Specification

## ADDED Requirements

### Requirement: Canvas overlay para esqueleto en vista de cámara
El sistema SHALL mostrar un canvas superpuesto sobre el elemento `<video>` de la cámara que dibuja el esqueleto de pose detectado por MediaPipe. El canvas SHALL tener las mismas dimensiones que el video (640×480) y posicionarse con `position: absolute` sobre el mismo.

#### Scenario: Tracking activo con persona detectada
- **WHEN** MediaPipe detecta una pose y el servidor envía landmarks
- **THEN** el canvas muestra líneas verdes conectando los puntos del esqueleto (hombros, codos, muñecas, cadera, rodillas, tobillos) y puntos verdes en cada landmark

#### Scenario: Tracking activo sin persona detectada
- **WHEN** MediaPipe no detecta ninguna pose en el frame actual
- **THEN** el canvas se limpia (no muestra esqueleto)

### Requirement: Formato de datos del esqueleto
El servidor SHALL enviar los 33 landmarks de pose en el campo `skeleton.landmarks` del JSON de respuesta, como array de arrays `[x, y, z]` en coordenadas normalizadas. El campo `skeleton.connections` SHALL contener los pares de índices que definen las líneas del esqueleto. El campo `skeleton.width` y `skeleton.height` SHALL indicar las dimensiones del frame.

#### Scenario: Frame procesado correctamente
- **WHEN** el servidor procesa un frame JPEG con pose detectada
- **THEN** el JSON de respuesta contiene `skeleton.landmarks` con 33 entradas, `skeleton.connections` con los pares de conexión, y `skeleton.width`/`skeleton.height` con las dimensiones del frame

### Requirement: Dibujo del esqueleto en el cliente
El cliente SHALL dibujar el esqueleto en el canvas overlay convirtiendo las coordenadas normalizadas a píxeles usando las dimensiones del frame. Las líneas SHALL ser de color verde (`#00ff00`), grosor 2px, y los puntos SHALL ser círculos verdes de radio 3px.

#### Scenario: Recepción de datos de esqueleto
- **WHEN** el cliente recibe un mensaje JSON con `skeleton.landmarks` y `skeleton.connections`
- **THEN** la función `drawSkeleton()` limpia el canvas y dibuja las líneas de conexión y puntos en las posiciones convertidas
