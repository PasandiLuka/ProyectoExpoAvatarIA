## Context

La cabeza del avatar no responde al movimiento real de la cabeza porque `headTiltZ` se deriva de la diferencia de altura entre hombros (`RSy - LSy`). Los 33 landmarks del pose de MediaPipe (incluyendo nariz, ojos, orejas) ya se envían en `skeleton.landmarks` y se dibujan en el canvas, pero nunca se usan para la animación del avatar.

## Goals / Non-Goals

**Goals:**
- Hacer que la cabeza del avatar rote cuando el usuario inclina su cabeza
- Usar datos ya existentes (nariz de `skeleton.landmarks[0]`) — sin tocar el servidor
- Mantener el mismo sistema de transform (JS `rotate()` via `avatarAnim.update`)

**Non-Goals:**
- Agregar head yaw (rotateY) — requiere 3D perspective en el CSS de la cabeza
- Agregar head pitch — requiere transform adicional
- Modificar el servidor Python
- Usar face_mesh landmarks (los de pose son suficientes para tilt)

## Decisions

### Decision 1: Usar nariz (landmark 0) del skeleton, no face_mesh

MediaPipe Pose ya incluye 11 landmarks de la cabeza (0-10): nariz, ojos, orejas, boca. Estos vienen en `skeleton.landmarks` y no requieren modificar el servidor.

Se usa `landmarks[0]` (nariz) porque es el landmark más estable y representativo de la posición de la cabeza.

### Decision 2: Cálculo de head tilt como offset lateral de la nariz

```
midShoulderX = (LSx + RSx) / 2
headTiltZ = (noseX - midShoulderX) * factor
```

Cuando la cabeza se inclina hacia la derecha, la nariz se desplaza a la derecha del punto medio de los hombros. Este offset lateral es proporcional al ángulo de inclinación.

**Factor**: 60.0, clamped a [-25, 25] grados. Determinado empíricamente para que una inclinación natural de cabeza produzca ~10-15° de rotación en el avatar.

**Alternativa considerada**: Usar el ángulo de la línea entre ojos (`atan2(rightEyeY - leftEyeY, rightEyeX - leftEyeX)`). Más preciso pero requiere dos landmarks extra. La nariz es suficiente para un tilt 2D y es más simple.

### Decision 3: Agregar NoseX, NoseY a ProcessedLandmarks

Se agregan dos campos nuevos al modelo existente:

```csharp
public double NoseX { get; set; }
public double NoseY { get; set; }
```

Y en `LandmarkParser.Parse`:

```csharp
if (data.Skeleton?.Landmarks != null && data.Skeleton.Landmarks.Count > 0)
{
    var nose = data.Skeleton.Landmarks[0];
    if (nose.Count >= 2)
    {
        result.NoseX = nose[0];
        result.NoseY = nose[1];
    }
}
```

## Risks / Trade-offs

- **[Riesgo bajo] Landmark 0 ausente**: Si `skeleton.landmarks` es null o vacío, NoseX/NoseY quedan en 0 (default de double). El `headTiltZ` dará 0, que es el comportamiento actual (sin cambios).
- **[Riesgo bajo] Precisión del tilt**: El offset nariz→hombros es una aproximación. No distingue entre tilt de cabeza y desplazamiento lateral del torso. Para un avatar 2D simplificado es aceptable.
- **[Riesgo bajo] Factor 60 hardcodeado**: Si se percibe demasiado sensible o poco, se puede ajustar. No requiere cambios estructurales.
