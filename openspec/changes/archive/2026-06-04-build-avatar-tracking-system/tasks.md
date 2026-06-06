## 1. Setup del proyecto

- [x] 1.1 Crear proyecto Blazor WebAssembly (.NET 10) en `AvatarExpo/` con estructura de carpetas para componentes, servicios y wwwroot
- [x] 1.2 Configurar Tailwind CSS en `wwwroot/index.html` con paleta de colores (principal, textoBeige, fondoApp, fondoCentral)
- [x] 1.3 Crear `server/` con script Python, `requirements.txt` (mediapipe, websockets, opencv-python, numpy)
- [x] 1.4 Configurar `AvatarState.cs` como Singleton en `Program.cs` con propiedades iniciales y evento `OnChange`
- [x] 1.5 Crear estructura de carpetas para specs: `openspec/specs/` con subdirectorios por capacidad

## 2. Servidor Python - WebSocket + MediaPipe

- [x] 2.1 Implementar servidor WebSocket en `server/server.py` que escuche en `ws://0.0.0.0:8765`
- [x] 2.2 Integrar MediaPipe Pose Landmarks (modelo Heavy) para extraer hombros, codos, muñecas
- [x] 2.3 Integrar MediaPipe Face Landmarks para detectar expresión facial (sonrisa, sorpresa, enojo, neutral)
- [x] 2.4 Implementar decodificación de frames JPEG binarios recibidos por WebSocket con OpenCV
- [x] 2.5 Implementar respuesta JSON minificada con estructura `{p, f, v}` según especificación
- [x] 2.6 Agregar soporte para heartbeat: responder "pong" a mensajes "ping"
- [x] 2.7 Manejar desconexión limpia y aceptar nuevas conexiones sin reiniciar el proceso

## 3. Cliente Blazor - Layout y Componentes Base

- [x] 3.1 Crear `Pages/Index.razor` con layout grid 3 columnas (20%-60%-20%) y colores de fondo
- [x] 3.2 Crear `Components/AvatarRenderer.razor` con contenedor `perspective: 800px` y `body-wrapper` con `transform-style: preserve-3d`
- [x] 3.3 Crear `Components/AvatarTorso.razor` con dimensiones fijas, border-radius, y soporte para clases de remera dinámicas
- [x] 3.4 Crear `Components/AvatarHead.razor` con ojos, cejas, boca, pelo, y placeholders para gorros/anteojos
- [x] 3.5 Crear `Components/AvatarArm.razor` reutilizable con parámetros (lado, punto de anclaje) y estructura upper/lower/hand
- [x] 3.6 Crear `Components/CameraPanel.razor` con placeholder de webcam feed, sliders de simulación, y botón calibrar
- [x] 3.7 Crear `Components/CustomizationPanel.razor` con secciones de pelo, remeras, gorros, anteojos, objetos en manos

## 4. Captura de Cámara y JS Interop

- [x] 4.1 Crear `wwwroot/js/camera.js` con funciones para `getUserMedia`, dibujar en canvas oculto, y convertir a JPEG ArrayBuffer
- [x] 4.2 Crear `Services/CameraService.cs` con JS interop para iniciar/detener cámara y recibir frames
- [x] 4.3 Implementar compresión JPEG con calidad configurable (default 0.6) en el canvas
- [x] 4.4 Implementar control de frame rate (30 FPS default) con requestAnimationFrame o setInterval

## 5. WebSocket Tracking Service

- [x] 5.1 Crear `Services/TrackingService.cs` con `ClientWebSocket`, conexión, y cola de frames
- [x] 5.2 Implementar envío de frames JPEG como ArrayBuffer binario por WebSocket
- [x] 5.3 Implementar recepción y deserialización de JSON de landmarks
- [x] 5.4 Implementar heartbeat: enviar "ping" cada 5s, esperar "pong" en 3s, marcar desconexión si timeout
- [x] 5.5 Implementar reconexión con backoff exponencial (1s, 2s, 4s, 8s, 16s, ... 16s max)
- [x] 5.6 Implementar medición de RTT y throttling adaptativo de FPS según umbrales (50ms/100ms/200ms)
- [x] 5.7 Implementar modo fallback: si no hay cámara o servidor, avatar en posición neutra con sliders habilitados

## 6. Renderizado del Avatar - Cinemática y CSS

- [x] 6.1 Implementar función de efecto espejo: `X = 1.0 - X` aplicada a todos los landmarks entrantes
- [x] 6.2 Implementar cálculo de ángulo de hombro: `Atan2(codo.Y - hombro.Y, codo.X - hombro.X)` → rotación de `.arm-container`
- [x] 6.3 Implementar cálculo de ángulo de codo (relativo): `anguloGlobalCodo - anguloHombro` → rotación de `.arm-lower`
- [x] 6.4 Implementar rotación 3D del torso con diferencia de profundidad Z entre hombros y clamping a ±30°
- [x] 6.5 Aplicar CSS `transform: perspective(600px) rotateY(anguloY) rotateZ(anguloZ)` al body-wrapper
- [x] 6.6 Implementar regla de reposo: si `v.lh < 0.5` o `v.rh < 0.5`, brazo correspondiente vuelve a `rotate(0deg)`
- [x] 6.7 Implementar expresiones faciales: mapear `exp` del servidor a clases CSS `.exp-smile`, `.exp-surprise`, `.exp-angry`

## 7. Suavizado de Movimiento (Lerp)

- [x] 7.1 Implementar función `Lerp(actual, objetivo, t)` genérica en C#
- [x] 7.2 Aplicar Lerp a cada ángulo calculado antes de asignarlo a CSS (hombros, codos, torso, cabeza)
- [x] 7.3 Hacer configurable el factor `t` con valor por defecto 0.3

## 8. Sistema de Calibración

- [x] 8.1 Implementar botón "Calibrar" en CameraPanel que active recolección de 60 frames (~2s)
- [x] 8.2 Calcular promedios de posiciones de hombros (X, Y, Z) y distancias faciales clave durante la calibración
- [x] 8.3 Almacenar `CalibrationOffset` en TrackingService y aplicarlo a cada landmark entrante
- [x] 8.4 Recalcular umbrales de expresiones basados en promedios de calibración
- [x] 8.5 Mostrar feedback visual durante calibración (cuenta regresiva) y confirmación al completar

## 9. Panel de Personalización

- [x] 9.1 Implementar selector de estilo de pelo (3 estilos) y color (6 swatches) en CustomizationPanel
- [x] 9.2 Implementar selector de remeras (4 estilos) que actualiza `ShirtStyle` y hereda color a mangas
- [x] 9.3 Implementar selector de gorros (3 + ninguno) con toggle de visibilidad CSS
- [x] 9.4 Implementar selector de anteojos (3 + ninguno) con toggle de visibilidad CSS
- [x] 9.5 Implementar selector de objetos en mano izquierda y derecha (3 + vacío) con toggle de visibilidad CSS
- [x] 9.6 Conectar todos los selectores a `AvatarState` y verificar que `NotifyStateChanged()` actualiza el avatar

## 10. Estilos CSS del Avatar

- [x] 10.1 Crear `wwwroot/css/avatar.css` con todos los estilos del prototipo: torso, cabeza, brazos, manos, accesorios
- [x] 10.2 Implementar estilos de remeras: `.shirt-short`, `.shirt-long`, `.shirt-boca`, `.shirt-river`
- [x] 10.3 Implementar estilos de pelo: `.hair-style-1`, `.hair-style-2`, `.hair-style-3` + variable CSS `--hair-color`
- [x] 10.4 Implementar estilos de gorros: `.hat-cowboy`, `.hat-chef`, `.hat-cap` con pseudo-elementos
- [x] 10.5 Implementar estilos de anteojos: `.glass-sun`, `.glass-googly`, `.glass-2026`
- [x] 10.6 Implementar estilos de objetos en mano: `.item-fernet`, `.item-water`, `.item-cup` con SVG de la copa
- [x] 10.7 Estilizar sliders de simulación y panel de cámara con la paleta de colores del proyecto

## 11. Seguridad y Despliegue

- [x] 11.1 Hacer configurable la URL del WebSocket (appsettings.json) para desarrollo local (`ws://`) y producción (`wss://`)
- [x] 11.2 Generar certificado SSL autofirmado para desarrollo con WSS
- [x] 11.3 Configurar Kestrel para servir Blazor sobre HTTPS en producción
- [x] 11.4 Documentar pasos de despliegue: instalar dependencias Python, iniciar servidor, publicar Blazor, configurar certificados

## 12. Integración y Pruebas

- [x] 12.1 Integrar flujo completo: cámara → canvas → WebSocket → servidor → JSON → avatar rendering
- [x] 12.2 Probar ciclo completo con sliders manuales del prototipo (modo demo) verificando que todas las articulaciones responden
- [x] 12.3 Probar expresiones faciales con sliders del simulador facial (normal, sonrisa, sorpresa, enojo)
- [x] 12.4 Probar personalización: verificar que cambios en panel derecho se reflejan instantáneamente en el avatar
- [x] 12.5 Probar reconexión: desconectar servidor, verificar backoff, reconectar servidor, verificar reanudación
- [x] 12.6 Probar modo fallback: denegar permiso de cámara, verificar que app funciona con sliders manuales
- [x] 12.7 Probar calibración: ejecutar calibrar, verificar que postura neutra produce ángulos ~0°
