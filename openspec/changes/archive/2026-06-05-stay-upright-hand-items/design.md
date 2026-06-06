## Context

Los items en la mano rotan con el brazo porque son hijos del `body-wrapper` que recibe los transforms. Para que un item apunte siempre hacia arriba, necesita una rotación compensatoria que cancele la rotación acumulada del brazo + antebrazo.

La solución usa una clase CSS `stay-upright` como opt-in: los items que la tengan se compensan, los que no (reloj, pulsera) siguen el brazo normalmente.

## Goals / Non-Goals

**Goals:**
- Items con `stay-upright` (fernet, agua, copa) siempre apuntan hacia arriba
- Items sin `stay-upright` (futuros: reloj, pulsera) siguen la rotación del brazo
- Funciona dinámicamente: se actualiza cada frame con los ángulos del tracking
- La implementación actual con rotate(±90°) se reemplaza

**Non-Goals:**
- Rotación 3D de los items (solo rotate 2D)
- Animación de transición al compensar

## Decisions

### Decision 1: Compensar rotación en JS, no en CSS

CSS no puede leer los ángulos del brazo (que vienen de JS). La compensación debe hacerse en `avatarAnim.update()` que ya tiene acceso a los ángulos.

```javascript
// en update(), después de aplicar transforms
var uprightItems = document.querySelectorAll('.item.stay-upright.active');
for (var i = 0; i < uprightItems.length; i++) {
    var item = uprightItems[i];
    var isLeft = item.closest('.arm-container.left');
    var baseRotation = isLeft ? 90 : -90;  // apuntar hacia afuera
    var shoulderAngle = isLeft ? angles.leftShoulder : angles.rightShoulder;
    var elbowAngle = isLeft ? angles.leftElbow : angles.rightElbow;
    var compensation = baseRotation - shoulderAngle - elbowAngle;
    item.style.transform = 'rotate(' + compensation + 'deg)';
}
```

**Ajuste de signos**: Los ángulos de hombro y codo ya están en el sistema de coordenadas CSS (con el offset -90° aplicado en C#). La compensación resta los ángulos acumulados porque los items rotan CON el brazo; al restar, se cancelan.

### Decision 2: `stay-upright` como clase CSS, no como parámetro Blazor

Es más simple y directo. El `AvatarArm` ya recibe `LeftHandItem`/`RightHandItem` como parámetros. En vez de agregar un parámetro booleano `Upright` por item, se usa una clase CSS fija en el template.

Los items que necesitan `stay-upright` la tienen hardcodeada en el HTML. Si en el futuro se necesita control dinámico (ej: un toggle en la UI), se puede migrar a un parámetro.

### Decision 3: Mantener rotate(±90°) como base

Después de cancelar la rotación del brazo, el item quedaría en su orientación natural (vertical hacia abajo). Se aplica un `rotate(±90°)` adicional para que apunte horizontalmente hacia afuera del cuerpo, que es la orientación natural de agarrar una botella/copa.

## Risks / Trade-offs

- **[Riesgo bajo] Signo de compensación**: Si el signo está invertido, el item rotará el doble en vez de compensar. Fácil de detectar y corregir probando con tracking activo.
- **[Riesgo bajo] Perf**: `querySelectorAll` cada frame es ~0.1ms para 3 elementos. Sin impacto.
- **[Riesgo bajo] Items futuros**: Si se agregan items sin `stay-upright`, simplemente no se compensan — comportamiento correcto por defecto.
