# Avatar Customization Specification

## ADDED Requirements

### Requirement: Servicio de estado AvatarState Singleton
El sistema SHALL proveer un servicio `AvatarState` registrado como Singleton en la inyección de dependencias de Blazor. Este servicio SHALL exponer propiedades para cada aspecto personalizable del avatar y un evento `OnChange` para notificar a los componentes suscriptores.

#### Scenario: Usuario cambia el estilo de remera
- **WHEN** el usuario hace clic en "Remera de Boca" en el panel derecho
- **THEN** `AvatarState.ShirtStyle` se actualiza a `"shirt-boca"`, se dispara `NotifyStateChanged()`, y el componente AvatarTorso actualiza su clase CSS a `torso shirt-boca`

#### Scenario: Múltiples componentes reaccionan al mismo cambio
- **WHEN** `AvatarState.NotifyStateChanged()` es invocado
- **THEN** todos los componentes suscritos (AvatarTorso, AvatarArm para colores de manga, etc.) se re-renderizan con el nuevo estado sin comunicación directa entre ellos

### Requirement: Personalización de pelo
El sistema SHALL soportar 3 estilos de pelo (`hair-style-1`, `hair-style-2`, `hair-style-3`) y 6 colores (negro, castaño, rubio, pelirrojo, gris, verdoso). El estilo y color SHALL aplicarse dinámicamente al avatar.

#### Scenario: Usuario selecciona "Tipo 2" y color "Rubio"
- **WHEN** el usuario hace clic en "Tipo 2" y luego en el swatch amarillo
- **THEN** el avatar muestra el pelo con forma `hair-style-2` y color `#e6c229`

### Requirement: Personalización de remeras
El sistema SHALL soportar 4 estilos de remera: `shirt-short` (manga corta gris), `shirt-long` (manga larga oscura), `shirt-boca` (Boca Juniors, azul y amarillo), `shirt-river` (River Plate, blanco y rojo). El color de las mangas SHALL heredarse del estilo de remera seleccionado.

#### Scenario: Usuario selecciona remera de River
- **WHEN** el usuario hace clic en "River Plate"
- **THEN** el torso muestra el gradiente blanco y rojo, y las mangas de los brazos cambian a blanco para mantener consistencia visual

### Requirement: Personalización de accesorios
El sistema SHALL soportar 3 tipos de gorros (vaquero, chef, gorra), 3 tipos de anteojos (de sol, saltones, "2026"), y 3 objetos por mano (Fernet, botella de agua, Copa Mundial). Los accesorios SHALL poder combinarse libremente.

#### Scenario: Usuario equipa gorra, anteojos de sol, y Fernet en mano derecha
- **WHEN** el usuario selecciona "Gorra", "De Sol", y "Fernet" en mano derecha
- **THEN** el avatar muestra simultáneamente la gorra roja, los anteojos oscuros, y la botella de Fernet en su mano derecha

#### Scenario: Usuario quita todos los accesorios
- **WHEN** el usuario hace clic en "Ninguno" para gorros, anteojos, y ambas manos
- **THEN** todos los accesorios desaparecen y el avatar queda con su apariencia base (pelo, remera, sin accesorios)

### Requirement: Modo demo funcional sin tracking
El panel de personalización SHALL permanecer completamente funcional incluso cuando no hay conexión de tracking activa o la cámara no está disponible. El avatar en modo demo SHALL mostrarse en posición neutra con los accesorios seleccionados.

#### Scenario: Usuario sin cámara personaliza el avatar
- **WHEN** no hay stream de tracking activo y el usuario selecciona "Pelo Tipo 3" y "Gorra de Chef"
- **THEN** el avatar en posición neutra refleja inmediatamente los cambios visuales seleccionados
