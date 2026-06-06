## 1. Timestamp en hora argentina

- [x] 1.1 En `TrackingService.SendScreenshot()`, reemplazar `DateTime.UtcNow` por conversión a `Argentina Standard Time`

## 2. Área de captura ampliada

- [x] 2.1 En `camera.js` `captureAvatarToBase64()`, cambiar selector de `#avatar-wrapper` a `.panel-central`

## 3. Verificación

- [x] 3.1 Probar "Sacar Foto" con tracking activo, brazos extendidos y un item equipado — captura muestra avatar completo
- [x] 3.2 Probar "Sacar Foto" en modo demo con gorro + anteojos + items — todos los accesorios visibles
- [x] 3.3 Verificar que el timestamp del archivo en Drive refleja hora argentina
