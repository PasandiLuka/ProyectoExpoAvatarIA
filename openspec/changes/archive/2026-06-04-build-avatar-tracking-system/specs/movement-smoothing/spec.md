# Movement Smoothing Specification

## ADDED Requirements

### Requirement: Interpolación Lerp para todos los ángulos
El sistema SHALL aplicar interpolación lineal (Lerp) a cada ángulo calculado (hombros, codos, torso, cabeza) antes de asignarlo a las propiedades CSS del avatar. La fórmula SHALL ser: `valorActual = valorActual + (valorObjetivo - valorActual) * t`, donde `t` es un factor de suavizado configurable (default: 0.3).

#### Scenario: Ángulo objetivo cambia bruscamente por jitter
- **WHEN** el ángulo del hombro recibido salta de 45° a 48° entre frames consecutivos (jitter de 3°)
- **THEN** el ángulo aplicado al CSS transiciona suavemente a ~45.9° en lugar de saltar directamente a 48°, reduciendo el temblor visible

#### Scenario: Ángulo se mantiene estable
- **WHEN** el ángulo objetivo se mantiene en 30° durante varios frames consecutivos
- **THEN** el ángulo aplicado converge asintóticamente a 30° sin oscilaciones

### Requirement: Factor de suavizado configurable
El factor `t` del Lerp SHALL ser configurable entre 0.1 (muy suave, alta inercia) y 0.8 (muy reactivo, poco filtrado), con valor por defecto de 0.3.

#### Scenario: Usuario prefiere respuesta más rápida
- **WHEN** el factor de suavizado se configura a 0.5
- **THEN** los movimientos del avatar siguen al usuario con menos inercia pero aún filtran el jitter fino

### Requirement: Aplicación independiente por articulación
El suavizado SHALL aplicarse de forma independiente a cada articulación (hombro izquierdo, hombro derecho, codo izquierdo, codo derecho, torso Y, cabeza Z) para evitar que una articulación ruidosa afecte a las demás.

#### Scenario: Solo el codo izquierdo tiene jitter
- **WHEN** el codo izquierdo presenta variaciones de ±5° pero el resto de articulaciones están estables
- **THEN** solo el codo izquierdo se suaviza; el resto de ángulos siguen respondiendo directamente sin retraso artificial
