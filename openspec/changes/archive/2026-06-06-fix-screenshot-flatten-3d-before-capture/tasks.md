## 1. camera.js — desactivar 3D antes de capturar

- [x] 1.1 Antes de `html2canvas`, setear `.panel-central` `perspective: none` y `#avatar-wrapper` `transform-style: flat`
- [x] 1.2 Después de `html2canvas` (en finally), restaurar ambas propiedades (`perspective: ''`, `transform-style: ''`)

## 2. Verificación

- [x] 2.1 Captura con tracking: avatar 100% opaco, sin traslucidez
- [x] 2.2 Captura con pochoclos (CSS degradados): balde rayado + pochoclos visibles y correctos
- [x] 2.3 Captura con sable láser (SVG): color y glow correctos
- [x] 2.4 Sin cartel "Avatar Render" en la foto
- [x] 2.5 El avatar en pantalla recupera perspectiva 3D después de la captura
