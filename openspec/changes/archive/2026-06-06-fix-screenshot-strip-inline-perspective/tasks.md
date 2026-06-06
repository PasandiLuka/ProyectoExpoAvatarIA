## 1. camera.js — strip de perspective() del transform inline

- [x] 1.1 Guardar `transform` inline actual de `#avatar-wrapper` antes de capturar
- [x] 1.2 Remover `perspective(Xpx)` del string con regex: `bw.style.transform.replace(/perspective\([^)]+\)\s*/, '')`
- [x] 1.3 Restaurar el transform original en el bloque `finally`

## 2. Verificación

- [x] 2.1 Captura con tracking: avatar 100% opaco, pose correcta, sin traslucidez
- [x] 2.2 Accesorios (pochoclos CSS, sable SVG) renderizan completos y correctos
- [x] 2.3 Después de capturar, el avatar en pantalla sigue viéndose con 3D normal
