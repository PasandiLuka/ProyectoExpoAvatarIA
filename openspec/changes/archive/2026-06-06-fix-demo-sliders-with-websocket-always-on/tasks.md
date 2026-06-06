## 1. AvatarRenderer — desacoplar demo del WebSocket

- [x] 1.1 Agregar flag `_landmarksActive` (default `false`) y `System.Threading.Timer` `_landmarkTimeout` de 2s
- [x] 1.2 En `HandleLandmarks()`, setear `_landmarksActive = true` y reiniciar el timer a 2s
- [x] 1.3 En el callback del timer, setear `_landmarksActive = false` y llamar `InvokeAsync(StateHasChanged)`
- [x] 1.4 Cambiar condición en `HandleAvatarStateChange()` de `!TrackingService.IsConnected` a `!_landmarksActive`
- [x] 1.5 Asegurar que `HandleTrackingState(false)` resetea `_landmarksActive = false` y detiene el timer
- [x] 1.6 Disponer el timer en `Dispose()`

## 2. Verificación

- [x] 2.1 Iniciar app sin tracking → mover sliders demo → avatar se mueve en tiempo real
- [x] 2.2 Iniciar tracking → sliders demo ignorados, avatar sigue landmarks
- [x] 2.3 Detener tracking → esperar 2s → sliders demo vuelven a funcionar
- [x] 2.4 Botones de expresión siguen funcionando en ambos modos
- [x] 2.5 "Sacar Foto" funciona en ambos modos (tracking y demo)
