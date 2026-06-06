# WebSocket Tracking Specification

## ADDED Requirements

### Requirement: Servidor recibe frames JPEG por WebSocket
El servidor Python SHALL aceptar conexiones WebSocket en `ws://0.0.0.0:8765` y recibir frames JPEG como mensajes binarios (ArrayBuffer). Cada frame SHALL ser decodificado y procesado por MediaPipe para extraer Pose Landmarks y Face Landmarks.

#### Scenario: Frame JPEG válido recibido
- **WHEN** el servidor recibe un mensaje binario que contiene un frame JPEG válido
- **THEN** decodifica el frame con OpenCV, ejecuta inferencia de MediaPipe, y devuelve un JSON con landmarks de pose y rostro

#### Scenario: Frame corrupto o inválido
- **WHEN** el servidor recibe un mensaje binario que no es un JPEG decodificable
- **THEN** ignora el frame y NO cierra la conexión, continúa esperando el siguiente mensaje

### Requirement: Respuesta JSON minificada con landmarks
El servidor SHALL devolver un JSON con la siguiente estructura exacta tras cada inferencia exitosa:

```json
{
  "p": {"ls":[x,y],"le":[x,y],"lw":[x,y],"rs":[x,y],"re":[x,y],"rw":[x,y]},
  "f": {"exp":"<expresion>","brow":"<cejas>"},
  "v": {"lh":<float>,"rh":<float>}
}
```

Donde `p` contiene coordenadas normalizadas (0.0-1.0) de hombros, codos y muñecas; `f` contiene la expresión facial detectada y estado de cejas; `v` contiene visibilidad de manos (0.0-1.0).

#### Scenario: Usuario frente a cámara con brazos visibles
- **WHEN** MediaPipe detecta pose completa con ambas manos visibles
- **THEN** el servidor devuelve todas las coordenadas con valores entre 0.0 y 1.0, `exp` con la expresión detectada, y `v.lh`/`v.rh` con valores > 0.5

#### Scenario: Usuario con una mano fuera de cuadro
- **WHEN** MediaPipe detecta pose pero una mano no es visible
- **THEN** el servidor devuelve coordenadas para la mano no visible con valor -1, y `v.lh` o `v.rh` < 0.5 para esa mano

### Requirement: Servidor soporta múltiples conexiones secuenciales
El servidor SHALL aceptar una nueva conexión WebSocket tras el cierre de la anterior sin necesidad de reiniciar el proceso.

#### Scenario: Cliente se desconecta y reconecta
- **WHEN** un cliente cierra su conexión WebSocket y luego un nuevo cliente (o el mismo) se conecta al mismo puerto
- **THEN** el servidor acepta la nueva conexión y comienza a procesar frames normalmente

### Requirement: Detección de expresiones faciales
El servidor SHALL clasificar la expresión facial en uno de: `"neutral"`, `"smile"`, `"surprise"`, `"angry"` basándose en distancias relativas entre puntos del Face Mesh de MediaPipe.

#### Scenario: Usuario sonríe
- **WHEN** la distancia horizontal entre puntos 61 y 291 del Face Mesh supera en 20% el umbral base calibrado
- **THEN** el campo `f.exp` devuelve `"smile"`

#### Scenario: Usuario abre la boca (sorpresa)
- **WHEN** la distancia vertical entre puntos 13 y 14 del Face Mesh supera el umbral base
- **THEN** el campo `f.exp` devuelve `"surprise"`

#### Scenario: Usuario frunce el ceño (enojo)
- **WHEN** la distancia entre puntos 107 y 336 a la raíz nasal es menor al umbral base
- **THEN** el campo `f.exp` devuelve `"angry"`
