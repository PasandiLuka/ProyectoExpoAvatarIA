# User Calibration Specification

## ADDED Requirements

### Requirement: Botón de calibración en UI
La UI del panel izquierdo SHALL incluir un botón "Calibrar" que inicie el proceso de calibración de postura neutra. Durante la calibración, se SHALL mostrar una cuenta regresiva de 2 segundos con instrucciones visuales ("Mirá al frente en posición relajada").

#### Scenario: Usuario inicia calibración
- **WHEN** el usuario hace clic en "Calibrar"
- **THEN** se muestra un overlay o texto con cuenta regresiva "Mantené la posición... 2... 1..." y se recolectan landmarks durante 2 segundos

#### Scenario: Calibración completada exitosamente
- **WHEN** transcurren 2 segundos de recolección de landmarks
- **THEN** se muestra confirmación "Calibración completada", se calculan los promedios, y se almacena el offset

### Requirement: Cálculo de promedios de landmarks
El sistema SHALL recolectar 60 frames (~2 segundos a 30 FPS) durante la calibración y calcular el promedio de: posición de hombros (X, Y, Z), distancias faciales clave (puntos 61-291 para sonrisa, 13-14 para sorpresa, 107-336 a raíz nasal para enojo). Estos promedios SHALL almacenarse como `CalibrationOffset` en memoria del cliente.

#### Scenario: Cálculo de offset de hombros
- **WHEN** se promedian 60 muestras de posición de hombros
- **THEN** el offset resultante representa la postura neutra del usuario específico y se resta de cada landmark entrante en frames subsiguientes

### Requirement: Aplicación de offset a landmarks entrantes
Cada landmark recibido del servidor SHALL ser corregido restando el `CalibrationOffset` correspondiente ANTES de calcular ángulos. `landmarkCorregido = landmarkCrudo - offset`.

#### Scenario: Frame post-calibración con usuario en postura neutra
- **WHEN** el usuario mantiene la misma postura que durante la calibración
- **THEN** los landmarks corregidos están centrados en ~0, produciendo ángulos cercanos a 0° en todas las articulaciones

#### Scenario: Frame post-calibración con usuario levantando un brazo
- **WHEN** el usuario levanta el brazo derecho desde su postura neutra calibrada
- **THEN** el ángulo del hombro calculado refleja solo la desviación respecto a su neutra, no valores absolutos

### Requirement: Umbrales de expresión basados en calibración
Los umbrales para detectar expresiones faciales (sonrisa, sorpresa, enojo) SHALL recalcularse basándose en las distancias promedio obtenidas durante la calibración. El umbral de sonrisa SHALL ser `distanciaPromedio * 1.2`, el de sorpresa `distanciaPromedioVertical * 1.5`.

#### Scenario: Usuario con rostro naturalmente expresivo
- **WHEN** un usuario tiene distancias faciales naturalmente grandes en reposo
- **THEN** tras calibrar, los umbrales se ajustan proporcionalmente y no se detectan falsas sonrisas en reposo

### Requirement: Persistencia de calibración en sesión
El `CalibrationOffset` SHALL mantenerse en memoria durante toda la sesión del navegador. No se requiere persistencia entre sesiones. Si el usuario cambia de posición significativamente, puede re-calibrar.

#### Scenario: Usuario recalibra tras cambiar de silla
- **WHEN** el usuario presiona "Calibrar" nuevamente
- **THEN** los offsets anteriores se descartan y se calculan nuevos promedios con los 60 frames frescos
