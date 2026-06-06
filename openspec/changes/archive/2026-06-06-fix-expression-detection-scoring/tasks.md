## 1. server.py — scoring system

- [x] 1.1 Función helper `clamp_score(value, min_val, max_val, invert=false)` que mapea un valor a [0,1]
- [x] 1.2 Calcular `lip_corner_up_score`: diferencia Y entre comisuras (61, 291) y centro del labio (13)
- [x] 1.3 Score de sorpresa: `mouth_open_score` (50%) + `brows_up_score` (50%)
- [x] 1.4 Score de sonrisa: `mouth_wide_score` (50%) + `lip_corner_up_score` (50%)
- [x] 1.5 Score de enojo: `brows_tight_score` (50%) + `mouth_tight_score` (50%)
- [x] 1.6 Selección: max(scores) ≥ 0.4 → expresión; sino "neutral"

## 2. Verificación

- [x] 2.1 Abrir bien la boca + levantar cejas → detecta "surprise"
- [x] 2.2 Sonreír ampliamente → detecta "smile"  
- [x] 2.3 Fruncir cejas + apretar boca → detecta "angry"
- [x] 2.4 Cara relajada sin expresión → detecta "neutral"
- [x] 2.5 Expresiones sutiles no disparan falsos positivos (score < 0.4)
