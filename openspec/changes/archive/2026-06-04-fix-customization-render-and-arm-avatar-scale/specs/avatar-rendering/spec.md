# Avatar Rendering Specification

## MODIFIED Requirements

### Requirement: Renderizado del torso con rotación 3D sutil
El componente de torso (`AvatarTorso`) SHALL recibir el estilo de remera como parámetro explícito `[Parameter] public string? ShirtStyle` desde su padre (`AvatarRenderer`), en lugar de leerlo de `@inject AvatarState`. El componente SHALL aplicar la clase CSS `torso @ShirtStyle` al elemento raíz. La rotación 3D continúa funcionando como antes mediante `transform: rotateY(angulo)` con `perspective: 800px`.

#### Scenario: Usuario cambia remera a Boca Juniors
- **WHEN** el usuario hace clic en "Boca Jrs" en el panel de personalización
- **THEN** `AvatarRenderer` pasa `ShirtStyle="shirt-boca"` a `AvatarTorso`, y el torso muestra el gradiente azul y amarillo sin requerir interacción con sliders o botones de expresión

#### Scenario: Usuario gira hombros levemente con remera personalizada
- **WHEN** la diferencia de profundidad entre hombros genera una rotación de 15° en el eje Y mientras el usuario tiene seleccionada "Manga Larga"
- **THEN** el torso rota 15° manteniendo el color oscuro `#2b2d42` de la remera manga larga

### Requirement: Personalización del avatar vía parámetros explícitos
Los componentes `AvatarTorso`, `AvatarHead` y `AvatarArm` SHALL recibir TODOS los datos de personalización como `[Parameter]` desde `AvatarRenderer`, eliminando la dependencia de `@inject AvatarState` para datos de renderizado. `AvatarRenderer` SHALL leer `AvatarState` al momento de renderizar y pasar los valores como parámetros a sus hijos.

Los parámetros requeridos son:
- **AvatarTorso**: `ShirtStyle` (string)
- **AvatarHead**: `HairStyle` (string), `HairColor` (string), `HatStyle` (string), `GlassesStyle` (string)
- **AvatarArm**: `HandItem` (string, uno por lado: `LeftHandItem`, `RightHandItem`)

#### Scenario: Cambio de pelo sin tocar otros controles
- **WHEN** el usuario hace clic en "Tipo 3" en personalización
- **THEN** `AvatarRenderer` re-renderiza y pasa `HairStyle="hair-style-3"` a `AvatarHead`, que aplica la clase CSS correspondiente inmediatamente

#### Scenario: Cambio de anteojos visible al instante
- **WHEN** el usuario hace clic en "Saltones" en personalización
- **THEN** `AvatarRenderer` pasa `GlassesStyle="glasses-googly"` a `AvatarHead`, y los anteojos saltones aparecen sobre los ojos del avatar sin necesidad de otra interacción

### Requirement: Color explícito del brazo superior
El segmento superior del brazo (`arm-upper`) SHALL tener un color de fondo explícito definido por CSS en lugar de `background: inherit`. El color por defecto SHALL ser `var(--skin-color)` (color de piel). Los selectores de hermano (`~`) existentes SHALL extender la regla para cambiar el color cuando se usan remeras de manga larga o de equipos de fútbol.

#### Scenario: Avatar con remera manga corta
- **WHEN** el avatar usa `shirt-short` (manga corta, default)
- **THEN** el brazo superior (`arm-upper`) se muestra con color de piel `#fcd5ce`, idéntico al antebrazo (`arm-lower`)

#### Scenario: Avatar con remera de River Plate
- **WHEN** el avatar usa `shirt-river`
- **THEN** el brazo superior se muestra en blanco (`#ffffff`), consistente con las mangas de la camiseta de River

### Requirement: Escala del avatar
El contenedor del avatar (`body-wrapper`) SHALL aplicar `transform-origin: center bottom` y una escala de `scale(2)` dentro de la cadena de transformación existente (`perspective → rotateY → rotateZ → scale`). Esto SHALL hacer que el avatar ocupe visualmente el doble de su tamaño base sin modificar las dimensiones CSS en píxeles de los elementos internos.

#### Scenario: Carga inicial con avatar escalado
- **WHEN** la aplicación se carga con el avatar en posición neutra
- **THEN** el avatar se muestra al doble de su tamaño base (360×500px visuales desde 180×250px lógicos), ocupando una porción significativa del panel central

## ADDED Requirements

### Requirement: Flujo de datos de personalización unidireccional
`AvatarRenderer` SHALL ser el único componente que lee `AvatarState` para datos de personalización. Los componentes hijos (`AvatarTorso`, `AvatarHead`, `AvatarArm`) SHALL recibir estos datos exclusivamente como `[Parameter]`, estableciendo un flujo de datos unidireccional típico de Blazor.

#### Scenario: Componente hijo no depende de @inject para renderizado
- **WHEN** se inspecciona el código de `AvatarHead.razor`
- **THEN** no contiene `@inject AvatarState` para leer `HairStyle`, `HairColor`, `HatStyle`, o `GlassesStyle` — todas son propiedades `[Parameter]`
