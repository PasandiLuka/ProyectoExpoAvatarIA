## 1. Modificar avatarAnim.js

- [x] 1.1 En `update()`, después de aplicar los transforms existentes, agregar loop que busca `.item.stay-upright.active` y aplica `rotate(±90° - shoulder - elbow)`
- [x] 2.1 En `avatar.css`, eliminar `.arm-container.left .item.active { transform: rotate(90deg); }` y `.arm-container.right .item.active { transform: rotate(-90deg); }`
- [x] 3.1 En `AvatarArm.razor`, agregar `stay-upright` a las clases de fernet, water y cup (ambas manos)
- [x] 4.1 Compilar `dotnet build` — 0 errores, 0 warnings
- [ ] 4.2 Probar: con tracking activo, mover brazos → fernet/agua/copa siempre hacia arriba
- [ ] 4.3 Probar: items sin stay-upright (si existen) siguen el brazo normalmente
- [ ] 4.4 Probar: mano izquierda y derecha apuntan hacia afuera del cuerpo
