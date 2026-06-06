## Context

El proyecto parte de un diseño documentado en 5 archivos dentro de `Informacion/` y un prototipo HTML funcional (`prototipo_avatar_y_ui_v2.html`). El sistema se divide en tres partes: un cliente Blazor WebAssembly (.NET 10), un servidor Python con MediaPipe Heavy, y un canal WebSocket que los comunica. El objetivo es lograr tracking de pose y rostro en tiempo real con latencia imperceptible, renderizando un avatar 2D con simulación 3D sutil.

## Goals / Non-Goals

**Goals:**
- Arquitectura cliente-servidor con WebSocket nativo, sin SignalR
- Servidor Python ejecutando MediaPipe Pose Landmarks + Face Landmarks (modelo Heavy)
- Cliente Blazor WASM con captura de webcam, renderizado CSS del avatar, y panel de personalización
- Cinemática de brazos basada exclusivamente en ángulos (sin escalado de elementos)
- Rotación 3D sutil del torso usando CSS perspective + rotateY con límites acotados
- Suavizado de movimiento vía interpolación lineal (Lerp)
- Calibración de postura neutra por usuario
- Reconexión automática con heartbeat, backoff exponencial, y throttling adaptativo
- Fallback visual funcional cuando no hay cámara (modo "demo" con sliders manuales)
- Comunicación segura vía WSS en producción

**Non-Goals:**
- Motor 3D real (Three.js, WebGL) — todo es CSS 2D con perspectiva
- Body tracking completo (piernas, pies) — solo upper body (torso, brazos, cabeza, rostro)
- Reconocimiento de voz o texto
- Multijugador o múltiples avatares simultáneos
- Mobile nativo — solo navegador desktop con webcam
- Streaming de video del avatar a otros clientes

## Decisions

### 1. WebSocket nativo en lugar de SignalR

**Decisión**: Usar `System.Net.WebSockets.ClientWebSocket` en C# del lado cliente y la librería `websockets` de Python en el servidor. No usar SignalR.

**Razón**: SignalR añade overhead de negociación HTTP, fallback a long-polling, y serialización JSON automática que no es necesaria. WebSocket nativo da control total sobre el ciclo de vida de la conexión, formato de payload binario/texto, y minimiza latencia. El doc de arquitectura explícitamente recomienda evitar SignalR.

**Alternativa considerada**: SignalR con MessagePack — descartado por complejidad innecesaria y capas de abstracción que dificultan el debugging de latencia.

### 2. Formato de payload: JPEG binario (subida) + JSON minificado (bajada)

**Decisión**: El cliente envía frames JPEG como ArrayBuffer binario por WebSocket. El servidor devuelve JSON minificado con solo las coordenadas necesarias.

**Formato de respuesta del servidor**:
```json
{
  "p": {"ls":[x,y],"le":[x,y],"lw":[x,y],"rs":[x,y],"re":[x,y],"rw":[x,y]},
  "f": {"exp":"smile","brow":"up"},
  "v": {"lh":0.9,"rh":0.1}
}
```

**Razón**: Enviar JPEG binario en lugar de Base64 reduce el payload ~33%. La respuesta JSON es mínima: solo 6 puntos de pose + expresión facial + visibilidad. Se eligió que el servidor devuelva coordenadas normalizadas (0-1) en lugar de ángulos precalculados para mantener el servidor stateless y permitir que el cliente aplique calibración y espejo.

### 3. Efecto espejo y cinemática en el cliente

**Decisión**: Toda la lógica de transformación (espejo, ángulos, calibración, Lerp) corre en C# en Blazor. El servidor solo hace inferencia de MediaPipe.

**Pipeline en cliente**:
```
Landmarks crudos → Calibración (restar offset) → Espejo (X = 1.0 - X) → Ángulos (Atan2) → Lerp → CSS transform
```

**Razón**: Mantiene el servidor Python simple y stateless. La lógica de presentación pertenece al cliente. Si se cambia el motor de renderizado (ej. pasar a Three.js en el futuro), el servidor no necesita cambios.

### 4. Estructura de componentes Blazor

**Decisión**: Layout grid 3 columnas con componentes Razor independientes:

```
Pages/Index.razor          → Grid container
├── Components/CameraPanel.razor      → Panel izquierdo (20%): webcam feed + sliders debug
├── Components/AvatarRenderer.razor   → Panel central (60%): avatar con perspective: 800px
│   ├── Components/AvatarTorso.razor  → Torso + rotación 3D
│   ├── Components/AvatarHead.razor   → Cabeza, ojos, cejas, boca, pelo, gorros, anteojos
│   └── Components/AvatarArm.razor    → Brazo (reutilizado para izq y der) con cinemática
└── Components/CustomizationPanel.razor → Panel derecho (20%): botones de personalización
```

**Servicios**:
- `AvatarState.cs` — Singleton con propiedades reactivas (ropa, accesorios, colores) + evento `OnChange`
- `TrackingService.cs` — Scoped, maneja WebSocket, cola de frames, buffer de landmarks
- `CameraService.cs` — JS interop para getUserMedia + canvas + envío de frames

### 5. CSS 3D del torso: perspective + rotateY con clamping

**Decisión**: El contenedor `.body-wrapper` recibe `transform: rotateY(angulo)` calculado a partir de la diferencia de profundidad (Z) entre hombros. El ángulo se limita a ±30° con un factor de sensibilidad bajo para evitar que el avatar "le dé la espalda" al usuario.

```
diferenciaZ = hombroDer.Z - hombroIzq.Z
anguloY = clamp(diferenciaZ * factorSensibilidad, -30, 30)
```

El contenedor central tiene `perspective: 800px` y el wrapper tiene `transform-style: preserve-3d`.

**Razón**: Sin clamping, diferencias grandes de profundidad producirían rotaciones no naturales. El factor de sensibilidad bajo (~0.5) asegura un efecto sutil e inmersivo.

### 6. Suavizado Lerp

**Decisión**: Interpolación lineal entre el valor actual del ángulo y el valor objetivo recibido del tracking. Factor de suavizado `t = 0.3` (30% hacia el target por frame).

```csharp
anguloActual = anguloActual + (anguloObjetivo - anguloActual) * t;
```

**Razón**: `t = 0.3` ofrece buen balance entre respuesta rápida y eliminación de jitter. Valores menores (0.1) se sienten "pesados", valores mayores (0.8) no filtran suficiente. Se aplica a todos los ángulos: hombros, codos, torso y cabeza.

### 7. Calibración de postura neutra

**Decisión**: Botón "Calibrar" captura 2 segundos de landmarks, calcula promedios, y los guarda como `CalibrationOffset` en el cliente. Los offsets se restan de cada landmark entrante antes de calcular ángulos.

**Flujo**:
1. Usuario presiona "Calibrar"
2. Se recolectan 60 frames (~2s a 30 FPS)
3. Se promedian posiciones de hombros y distancias faciales clave
4. Se almacenan en `TrackingService.CalibrationOffset`
5. En cada frame subsiguiente: `landmarkCorregido = landmarkCrudo - offset`

**Razón**: Hacer la calibración en el cliente mantiene el servidor stateless. Cada usuario tiene su propio offset sin afectar a otros (aunque en la práctica es single-user).

### 8. Heartbeat, reconexión y throttling

**Decisión**:
- **Heartbeat**: Ping/pong cada 5 segundos. Si no hay respuesta en 3 segundos, se considera desconectado.
- **Reconexión**: Backoff exponencial: 1s → 2s → 4s → 8s → 16s (max). Después de 5 intentos fallidos, mostrar toast "Conexión perdida".
- **Throttling**: Si el round-trip time supera 100ms, reducir envío a 20 FPS. Si supera 200ms, a 15 FPS. Si <50ms, mantener 30 FPS.

**Razón**: El heartbeat es estándar para WebSocket. El backoff exponencial evita flooding. El throttling adaptativo es crítico en WiFi de expo donde la red puede estar congestionada.

### 9. WSS y HTTPS

**Decisión**: En desarrollo, usar `ws://localhost:8765` y `http://localhost:5000`. En producción/expo, configurar certificado autofirmado y usar `wss://` y `https://`.

**Razón**: `getUserMedia` solo funciona en contextos seguros. Para una demo en red local, un certificado autofirmado es suficiente (el equipo de la expo instala el certificado en los navegadores de las máquinas cliente). Let's Encrypt no es viable sin dominio público.

### 10. Fallback sin cámara

**Decisión**: Si `getUserMedia` falla o el usuario rechaza permisos, el sistema entra en "modo demo": el avatar adopta posición neutra, y los sliders manuales del panel izquierdo (ya existentes en el prototipo) permiten simular movimiento. El panel de personalización sigue 100% funcional.

**Razón**: No se debe mostrar una pantalla rota o vacía. El prototipo HTML ya implementa este modo demo con sliders. La experiencia de personalización es valiosa incluso sin tracking.

### 11. AvatarState como Singleton

**Decisión**: `AvatarState.cs` registrado como Singleton en DI de Blazor WASM. Expone propiedades para cada aspecto personalizable y un evento `OnChange`. Los componentes se suscriben y re-renderizan al cambiar el estado.

```csharp
public class AvatarState {
    public string ShirtStyle { get; set; } = "shirt-short";
    public string HatStyle { get; set; } = "none";
    public string GlassesStyle { get; set; } = "none";
    public string LeftHandItem { get; set; } = "none";
    public string RightHandItem { get; set; } = "none";
    public string HairStyle { get; set; } = "hair-style-1";
    public string HairColor { get; set; } = "#3e2723";
    public event Action? OnChange;
    public void NotifyStateChanged() => OnChange?.Invoke();
}
```

**Razón**: Singleton asegura que todos los componentes compartan el mismo estado. El patrón event-based evita acoplar componentes entre sí.

### 12. Tailwind CSS + CSS nativo

**Decisión**: Tailwind CDN en `index.html` para layout, utilidades, colores de la paleta. CSS nativo en archivos `.css` por componente para transformaciones 3D y reglas complejas (sombras, keyframes, pseudo-elementos de accesorios).

**Paleta**:
- `principal`: `#990066` (violetita, fondos de paneles)
- `textoBeige`: `rgba(245, 245, 220, 0.9)` (textos, bordes)
- `fondoApp`: `#1a1a1a`
- `fondoCentral`: `#2a2a35` (canvas del avatar)

**Razón**: Tailwind acelera el layout base y mantiene consistencia. CSS nativo es necesario para `transform`, `perspective`, `transform-origin`, `transform-style: preserve-3d` y pseudo-elementos que Tailwind no cubre bien.

## Risks / Trade-offs

- **[Riesgo] Latencia real > 33ms (2 frames a 30 FPS)** → Mitigación: El Lerp oculta parte de la latencia; el throttling adaptativo evita saturar la red. Si no alcanza, se puede reducir resolución JPEG o bajar a modelo MediaPipe Lite.
- **[Riesgo] MediaPipe Heavy requiere GPU para rendimiento óptimo** → Mitigación: En máquinas sin GPU, usar MediaPipe Lite como fallback configurable. El doc de arquitectura pide Heavy explícitamente, así que se prioriza.
- **[Riesgo] Certificado autofirmado genera warning en navegadores** → Mitigación: Documentar el proceso de aceptación del certificado para el equipo de la expo. Es un one-time click por máquina.
- **[Riesgo] WebSocket binario (ArrayBuffer) requiere manejo manual de buffers** → Mitigación: Encapsular en `TrackingService` con buffer circular y semáforo para evitar memory leaks.
- **[Trade-off] Servidor Python vs C# para MediaPipe** → Se eligió Python por ser el runtime nativo de MediaPipe. La alternativa (MediaPipe para .NET via ONNX) no tiene soporte oficial para el modelo Heavy. El costo es mantener dos codebases (C# + Python).
- **[Trade-off] CSS 3D vs WebGL/Three.js** → CSS 3D es más simple, no requiere librerías adicionales, y el prototipo ya lo valida. La limitación es que no se pueden hacer rotaciones compuestas complejas ni iluminación dinámica. Para una expo, el efecto es suficiente.

## Open Questions

- ¿Se usará un solo servidor Python para todas las máquinas de la expo o uno por máquina? Afecta la dirección IP/hostname en la configuración del WebSocket.
- ¿El certificado SSL lo genera el equipo de infraestructura de la expo o se incluye uno autofirmado en el repo?
- ¿MediaPipe Heavy se corre en CPU o GPU del servidor? Determina si se necesita `mediapipe` con soporte CUDA.
- ¿El servidor Python corre en la misma máquina que el cliente Blazor o en una separada? Afecta la configuración de CORS y la IP del WebSocket.
