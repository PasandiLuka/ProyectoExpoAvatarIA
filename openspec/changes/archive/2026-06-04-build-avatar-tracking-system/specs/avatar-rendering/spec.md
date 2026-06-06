# Avatar Rendering Specification

## ADDED Requirements

### Requirement: Renderizado del torso con rotación 3D sutil
El componente de avatar SHALL renderizar un torso con dimensiones fijas (width/height CSS constantes) que rota en el eje Y basándose en la diferencia de profundidad (Z) entre hombros del usuario. La rotación SHALL estar limitada a ±30° y aplicarse mediante `transform: rotateY(angulo)` en un contenedor con `perspective: 800px`.

#### Scenario: Usuario gira hombros levemente
- **WHEN** la diferencia de profundidad entre hombro derecho e izquierdo es de 0.1 unidades normalizadas
- **THEN** el torso rota proporcionalmente en el eje Y, dentro del rango ±30°, visible como un sutil giro 3D

#### Scenario: Usuario en postura completamente frontal
- **WHEN** ambos hombros tienen la misma profundidad (diferencia ≈ 0)
- **THEN** el torso permanece en 0° de rotación (posición frontal)

### Requirement: Cinemática de brazos rígidos
Los brazos del avatar SHALL mantener dimensiones fijas en píxeles (upper: 120px, lower: 110px) y SHALL posicionarse exclusivamente mediante rotación angular (`transform: rotate()`), nunca mediante escalado. Los ángulos se calcularán con `Math.Atan2` a partir de las coordenadas de hombro, codo y muñeca.

#### Scenario: Usuario levanta el brazo derecho
- **WHEN** las coordenadas del codo derecho están por encima del hombro derecho
- **THEN** el brazo superior derecho rota hacia arriba manteniendo su tamaño fijo de 120px, y el antebrazo rota relativamente al codo

#### Scenario: Usuario extiende el brazo horizontalmente
- **WHEN** la muñeca está alineada horizontalmente con el codo y el hombro
- **THEN** el brazo superior rota ~90° y el antebrazo mantiene ~0° relativo, sin estirarse ni deformarse

### Requirement: Efecto espejo
El sistema SHALL invertir la coordenada X de todos los landmarks recibidos del servidor aplicando `X_espejo = 1.0 - X_original`. El hombro izquierdo de MediaPipe (landmark 11) SHALL mapearse al brazo derecho del avatar en pantalla.

#### Scenario: Usuario levanta la mano derecha (desde su perspectiva)
- **WHEN** el landmark correspondiente a la mano derecha del usuario (derecha de MediaPipe) tiene coordenada X cercana a 1.0
- **THEN** en pantalla, el brazo izquierdo del avatar (el que ve el usuario como su reflejo) se levanta

### Requirement: Posición de reposo cuando manos no detectadas
Cuando la visibilidad de una muñeca cae por debajo de 0.5, el brazo correspondiente SHALL retornar suavemente a la posición neutra (`rotate(0deg)`), cayendo verticalmente pegado al costado del torso.

#### Scenario: Usuario saca una mano del cuadro de la cámara
- **WHEN** la visibilidad de la muñeca izquierda (`v.lh`) cae por debajo de 0.5
- **THEN** el brazo izquierdo del avatar transiciona suavemente a `rotate(0deg)` (caído al costado)

### Requirement: Expresiones faciales del avatar
El avatar SHALL reflejar la expresión facial recibida del servidor modificando dinámicamente los elementos CSS de la boca y cejas. Las expresiones soportadas son: `"neutral"`, `"smile"`, `"surprise"`, `"angry"`.

#### Scenario: Servidor reporta expresión "smile"
- **WHEN** el campo `f.exp` tiene valor `"smile"`
- **THEN** la boca del avatar se ensancha horizontalmente (border-radius modificado) y las comisuras se elevan

#### Scenario: Servidor reporta expresión "surprise"
- **WHEN** el campo `f.exp` tiene valor `"surprise"`
- **THEN** la boca del avatar se vuelve circular (border-radius: 50%) y se agranda

#### Scenario: Servidor reporta expresión "angry"
- **WHEN** el campo `f.exp` tiene valor `"angry"`
- **THEN** la boca del avatar se invierte (curvatura hacia abajo) y las cejas rotan hacia adentro (~15°)

### Requirement: Layout de 3 columnas
La UI SHALL distribuirse en un grid CSS de 3 columnas con proporciones 20% (panel izquierdo), 60% (panel central del avatar), 20% (panel derecho de personalización), ocupando el 100% del viewport height.

#### Scenario: Carga inicial de la aplicación
- **WHEN** la aplicación se carga en el navegador
- **THEN** se muestra el layout de 3 columnas con el panel central conteniendo el avatar en su posición neutra y los paneles laterales con sus controles
