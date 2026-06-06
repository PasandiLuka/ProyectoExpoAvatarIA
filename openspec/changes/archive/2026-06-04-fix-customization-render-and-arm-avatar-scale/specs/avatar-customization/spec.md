# Avatar Customization Specification

## MODIFIED Requirements

### Requirement: Servicio de estado AvatarState Singleton
El sistema SHALL proveer un servicio `AvatarState` registrado como Singleton en la inyección de dependencias de Blazor. Este servicio SHALL exponer propiedades para cada aspecto personalizable del avatar y un evento `OnChange` para notificar a los componentes suscriptores. **Los componentes que renderizan el avatar (AvatarRenderer y sus hijos) SHALL leer AvatarState exclusivamente desde AvatarRenderer, que pasa los valores como `[Parameter]` a sus hijos. Los componentes que escriben estado (CustomizationPanel, CameraPanel) continúan usando `@inject AvatarState`.**

#### Scenario: Usuario cambia el estilo de remera
- **WHEN** el usuario hace clic en "Remera de Boca" en el panel derecho
- **THEN** `AvatarState.ShirtStyle` se actualiza a `"shirt-boca"`, se dispara `NotifyStateChanged()`, `AvatarRenderer` re-renderiza y pasa el nuevo valor como parámetro a `AvatarTorso`, que aplica la clase CSS `torso shirt-boca`

#### Scenario: Múltiples componentes reaccionan al mismo cambio
- **WHEN** `AvatarState.NotifyStateChanged()` es invocado
- **THEN** `AvatarRenderer` se re-renderiza vía su suscripción a `OnChange`, y TODOS sus hijos reciben nuevos valores de parámetros, garantizando que cambios de pelo, remera, gorro, anteojos y objetos de mano se reflejen simultáneamente

### Requirement: Personalización de remeras
El sistema SHALL soportar 4 estilos de remera: `shirt-short` (manga corta gris), `shirt-long` (manga larga oscura), `shirt-boca` (Boca Juniors, azul y amarillo), `shirt-river` (River Plate, blanco y rojo). El color de las mangas SHALL definirse mediante reglas CSS explícitas usando el selector de hermano general `~`, sin depender de `background: inherit`.

#### Scenario: Usuario selecciona remera de River
- **WHEN** el usuario hace clic en "River Plate"
- **THEN** el torso muestra el gradiente blanco y rojo, y las mangas superiores e inferiores de los brazos cambian a blanco (`#ffffff`) mediante CSS sin depender de herencia de background

### Requirement: Modo demo funcional sin tracking
El panel de personalización SHALL permanecer completamente funcional incluso cuando no hay conexión de tracking activa o la cámara no está disponible. El avatar en modo demo SHALL mostrarse en posición neutra con los accesorios seleccionados. **Los cambios de personalización en modo demo SHALL reflejarse inmediatamente como resultado del flujo de datos vía parámetros, sin depender de parámetros cinemáticos (ángulos, expresión) que no cambian en modo demo.**

#### Scenario: Usuario sin cámara personaliza el avatar
- **WHEN** no hay stream de tracking activo y el usuario selecciona "Pelo Tipo 3" y "Gorra de Chef"
- **THEN** el avatar en posición neutra refleja inmediatamente ambos cambios visuales, sin requerir que el usuario también modifique sliders o botones de expresión
