## Why

La cabeza del avatar no rota cuando el usuario mueve solo la cabeza. El cálculo actual usa `RSy - LSy` (diferencia de altura entre hombros), que mide la inclinación del torso, no de la cabeza. Si el usuario inclina la cabeza sin mover los hombros, la cabeza del avatar no se mueve.

El dato de la nariz YA existe: `skeleton.landmarks[0]` viene en cada frame del servidor, se deserializa en `SkeletonData`, y se dibuja correctamente en el canvas del esqueleto (el triángulo nariz→hombros). Pero `ProcessedLandmarks` y `HandleLandmarks` nunca lo leen. Solo hay que cablearlo.

## What Changes

- **`Models.cs`**: `ProcessedLandmarks` + campos `NoseX`, `NoseY`
- **`Models.cs`**: `LandmarkParser.Parse` extrae nariz de `data.Skeleton.Landmarks[0]`
- **`AvatarRenderer.razor`**: `HandleLandmarks` calcula `headTiltZ` a partir de la posición de la nariz relativa al punto medio de los hombros, en vez de `RSy - LSy`

## Impact

- `AvatarExpo/Services/Models.cs` — agregar NoseX, NoseY a ProcessedLandmarks + parse en LandmarkParser
- `AvatarExpo/Components/AvatarRenderer.razor` — cambiar fórmula de headTiltZ

El servidor NO se modifica. El dato ya viene en el payload.
