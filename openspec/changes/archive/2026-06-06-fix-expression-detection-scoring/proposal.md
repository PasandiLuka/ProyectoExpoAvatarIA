## Why

La detección de expresiones usa `if/elif` secuencial con umbrales desbalanceados: sorpresa (0.04) se dispara 7x más fácil que sonrisa (0.28) y bloquea los demás checks. Solo la expresión de sorpresa funciona en la práctica. Se necesita un sistema de puntuación equitativo que evalúe todas las expresiones simultáneamente.

## What Changes

- **Sistema de scoring**: `detect_expression()` calcula un score [0,1] para cada expresión basado en múltiples señales faciales ponderadas, elige la de mayor puntaje, y solo la activa si supera un umbral de confianza (0.4). Si ninguna supera, retorna "neutral".
- **Señales por expresión**: sorpresa (boca abierta + cejas arriba), sonrisa (boca ancha + comisuras arriba), enojo (cejas juntas + boca tensa)
- **Normalización**: todos los umbrales se calibran para que una expresión "clara" produzca score ~0.6-0.8 en todas las categorías

## Capabilities

### Modified Capabilities
- `screenshot-upload`: la detección de expresiones faciales usa scoring ponderado multi-señal en vez de if/elif secuencial (no afecta screenshots, pero es el capability más cercano)

## Impact

- `server/server.py` — `detect_expression()`: reescritura completa del algoritmo de detección
