## Context

El `#avatar-wrapper` mide 180x250px pero el avatar completo (con brazos extendidos e items) ocupa ~400x450px. html2canvas clipea al tamaño del elemento raíz, resultando en una captura parcial. Además, el timestamp del archivo usa UTC.

## Goals / Non-Goals

**Goals:**
- Screenshot captura el avatar completo incluyendo brazos, items, cabeza
- Nombre de archivo refleja hora argentina (UTC-3)

**Non-Goals:**
- No se modifica el servidor Python
- No se modifica el watermark ET12

## Decisions

### 1. Capturar `.panel-central` en vez de `#avatar-wrapper`

**Alternativa considerada:** Pasar `width`/`height` explícitos a html2canvas.
**Decisión:** Cambiar el selector. `.panel-central` es el contenedor de la vista completa del avatar (`width: 100%`, `height: 100%`), ya tiene `perspective: 800px` y `overflow: hidden`. html2canvas lo renderiza completo. El resultado es una imagen que muestra el avatar entero, centrado.

### 2. Timezone Argentina Standard Time

**Alternativa considerada:** Hardcodear UTC-3 manualmente.
**Decisión:** Usar `TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time")`. Maneja automáticamente cambios de zona horaria si los hubiera.

```csharp
var argentinaTz = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentinaTz);
```

## Risks / Trade-offs

- **`.panel-central` captura la UI de tracking también** → la barra "Avatar Render" y el skeleton canvas entran en la captura. Si molesta, se puede ocultar con CSS antes de capturar. Para v1, el avatar es lo dominante en la imagen.
- **html2canvas + `perspective` 3D** → los elementos con `transform-style: preserve-3d` pueden renderizarse sin la profundidad 3D en el canvas. Es una limitación conocida de html2canvas. El resultado puede verse "aplanado" pero completo.
