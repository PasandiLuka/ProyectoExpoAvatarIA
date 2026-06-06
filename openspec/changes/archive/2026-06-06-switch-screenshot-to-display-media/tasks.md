## 1. camera.js — inicializar stream display media

- [x] 1.1 Agregar método `initDisplayCapture()` que llame a `getDisplayMedia({ preferCurrentTab: true })` y guarde el stream
- [x] 1.2 Crear `<video>` oculto que reproduzca el stream para poder extraer frames
- [x] 1.3 Agregar método `stopDisplayCapture()` que detenga el stream y limpie el video

## 2. camera.js — nueva captura con recorte

- [x] 2.1 Reemplazar `captureAvatarToBase64()`: extraer frame del video, recortar al `getBoundingClientRect()` del wrapper
- [x] 2.2 Aplicar watermark ET12 centrado sobre el canvas recortado
- [x] 2.3 Retornar `toDataURL('image/png')`

## 3. camera.js — fallback a html2canvas

- [x] 3.1 Si `getDisplayMedia` falla, usar `captureWithHtml2canvas()` con `width`/`height` calculados del bounding box del wrapper
- [x] 3.2 Limpiar código de desactivación 3D que ya no es necesario en el path principal

## 4. Verificación

- [x] 4.1 Captura en Chrome/Edge: imagen pixel-perfect, sin traslucidez, accesorios correctos
- [x] 4.2 Captura en Firefox: fallback html2canvas funciona
- [x] 4.3 Diálogo de permiso aparece solo 1 vez, no en cada foto
- [x] 4.4 Recorte: solo el avatar, sin UI
- [x] 4.5 Watermark centrado correctamente
