## Context

El sistema actual evalúa expresiones en orden fijo con umbrales dispares. Se propone un scoring system que calcula probabilidad para cada expresión usando múltiples landmarks de MediaPipe Face Mesh.

## Goals / Non-Goals

**Goals:**
- Las 4 expresiones (neutral, smile, surprise, angry) son detectables con similar facilidad
- Cada expresión usa al menos 2 señales faciales independientes
- Si ninguna expresión es clara, el sistema retorna "neutral"
- Umbrales calibrados para que una expresión "evidente" dé score ≥ 0.5

**Non-Goals:**
- No se agregan nuevas expresiones
- No se modifica el frontend

## Decisions

### 1. Sistema de scoring por señal

Cada expresión recibe un score [0,1] basado en la suma ponderada de sus señales:

```
Score = w1 * signal1_normalized + w2 * signal2_normalized
```

Donde cada `signal_normalized` es el valor crudo del landmark mapeado a [0,1] mediante una función de saturación:

```
signal_normalized = clamp((raw_value - threshold_min) / (threshold_max - threshold_min), 0, 1)
```

Esto hace que todas las señales compitan en igualdad de condiciones.

### 2. Señales por expresión

**Sorpresa (surprise):**
- `mouth_open_score`: `mouth_height` mapeado de 0.01→0 a 0.06→1
- `brows_up_score`: `brow_height` mapeado de 0.02→0 a 0.06→1  
- Peso: 50% boca, 50% cejas

**Sonrisa (smile):**
- `mouth_wide_score`: `mouth_width` mapeado de 0.20→0 a 0.35→1
- `lip_corner_up_score`: diferencia Y de comisuras (61, 291) vs centro del labio (13, 14) — comisuras más altas que el centro = sonrisa
- Peso: 50% ancho, 50% comisuras arriba

**Enojo (angry):**
- `brows_tight_score`: `brow_proximity` mapeado de 0.30→0 a 0.10→1 (invertido: más juntas = más score)
- `mouth_tight_score`: `mouth_height` mapeado de 0.03→1 a 0.005→0 (invertido: boca más cerrada = más score, pero no 0 absoluto)
- Peso: 50% cejas, 50% boca tensa

### 3. Selección final

```python
scores = {
    "surprise": surprise_score,
    "smile": smile_score, 
    "angry": angry_score
}
best = max(scores, key=scores.get)
if scores[best] >= 0.4:
    return best, brow_state
return "neutral", brow_state
```

Esto elige la expresión con mayor score siempre que supere 0.4. Si hay empate virtual (todas < 0.4), retorna "neutral".

## Risks / Trade-offs

- **Calibración inicial** → los umbrales y pesos pueden requerir ajuste fino una vez probados con usuarios reales. Los valores propuestos son estimaciones basadas en la escala de landmarks de MediaPipe (0-1 normalizados).
- **Comisuras de labios** → la señal `lip_corner_up` es novedosa y no está en el código actual. Requiere calcular la diferencia entre las comisuras (61, 291) y el centro del labio (13, 14).
