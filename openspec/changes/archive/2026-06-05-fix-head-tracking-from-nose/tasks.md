## 1. Agregar nariz al modelo de datos

- [x] 1.1 Agregar `NoseX`, `NoseY` a `ProcessedLandmarks` en `Models.cs`
- [x] 1.2 En `LandmarkParser.Parse`, extraer `data.Skeleton.Landmarks[0]` → `result.NoseX`, `result.NoseY`
- [x] 2.1 En `HandleLandmarks`, reemplazar `targetHeadTiltZ = (data.RSy - data.LSy) * 30` por `targetHeadTiltZ = ((1.0 - data.NoseX) - midShoulderX) * 60`
- [x] 2.2 Ajustar clamp: de `[-15, 15]` a `[-25, 25]`
- [x] 3.1 Compilar `dotnet build` — 0 errores, 0 warnings
- [x] 3.2 Probar: inclinar cabeza a la derecha → avatar inclina cabeza a la derecha
- [x] 3.3 Probar: inclinar cabeza a la izquierda → avatar inclina cabeza a la izquierda
- [x] 3.4 Probar: mover solo hombros (no cabeza) → la cabeza del avatar no se mueve o se mueve mínimamente
- [x] 3.5 Probar: posición neutral → cabeza del avatar centrada
