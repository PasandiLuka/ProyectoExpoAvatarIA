# Avatar Expo IA - Guia de Despliegue

## Requisitos
- .NET 10 SDK
- Python 3.10+ con pip
- Navegador moderno con soporte WebSocket

## Configuracion inicial

### Credenciales de Google Drive (OAuth)

Para que el boton "Sacar Foto" funcione, se necesita un cliente OAuth de Google con acceso a una carpeta de Drive.

> El servidor usa el flujo **OAuth Installed App**. La primera vez que inicies el servidor se abrira el navegador para que autorices el acceso a Drive. Las credenciales se guardan en `server/token.json` para sesiones futuras.

#### 1. Crear credenciales OAuth en Google Cloud

1. Ir a [Google Cloud Console](https://console.cloud.google.com)
2. Crear un proyecto (o usar uno existente)
3. Habilitar **Google Drive API**
4. Ir a **API y Servicios → Pantalla de consentimiento OAuth**
5. Elegir **Externo** y completar los datos basicos (nombre, email)
6. Ir a **API y Servicios → Credenciales → Crear credenciales → ID de cliente OAuth**
7. Tipo de aplicacion: **Aplicacion de escritorio**
8. Darle un nombre (ej: "Avatar Expo") y crear
9. Descargar el JSON y renombrarlo como `credentials.json`

#### 2. Carpeta de Drive

1. En Google Drive, crear una carpeta donde se guardaran las fotos (ej: "Fotos Avatar Expo")
2. Copiar el ID de la carpeta desde la URL: `https://drive.google.com/drive/folders/`**`1a2b3c4d5e...`**

> No hace falta compartir manualmente la carpeta. Al autorizar con OAuth, la app usa tu propia cuenta de Google y tiene acceso a tus carpetas.

#### 3. Configurar `credentials.json`

Copiar el archivo a la carpeta `server/` y agregar el `folder_id`:

```
Copiar credentials.json a server/
```

Editar `server/credentials.json` para que tenga este formato:

```json
{
  "installed": {
    "client_id": "xxx.apps.googleusercontent.com",
    "project_id": "mi-proyecto-expo",
    "auth_uri": "https://accounts.google.com/o/oauth2/auth",
    "token_uri": "https://oauth2.googleapis.com/token",
    "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
    "client_secret": "XXX",
    "redirect_uris": ["http://localhost"]
  },
  "folder_id": "1a2b3c4d5e..."
}
```

> Usa `server/credentials.json.example` como referencia. El archivo real `credentials.json` y `token.json` estan en `.gitignore` y no se commitean.

> Como alternativa al campo `folder_id`, podes setear la variable de entorno `DRIVE_FOLDER_ID`:
> ```bash
> # Linux/macOS
> export DRIVE_FOLDER_ID=1a2b3c4d5e...
> 
> # Windows (PowerShell)
> $env:DRIVE_FOLDER_ID="1a2b3c4d5e..."
> ```

## Instalacion y Ejecucion

### 1. Servidor Python (MediaPipe)

```bash
cd server
pip install -r requirements.txt
python3 server.py
```

Escucha en `ws://0.0.0.0:8765`.

> Archivos del servidor: `server.py` (WebSocket + MediaPipe), `google_drive.py` (subida a Drive), `credentials.json.example` (plantilla).

La primera vez que se suba una foto a Drive, el navegador se abrira para autorizar. El token se guarda en `server/token.json`.

### 2. Cliente Blazor WASM

En otra terminal:

```bash
cd AvatarExpo
dotnet run
```

Abre en `http://localhost:5233`. La URL del WebSocket se configura automaticamente para apuntar al mismo host en puerto `8765`.

Si necesitas apuntar a otro servidor WebSocket, edita `wwwroot/appsettings.json`:
```json
{
  "WebSocket": {
    "Url": "ws://192.168.1.100:8765"
  }
}
```

> Si no configuras `WebSocket:Url`, se usa `ws://<host-actual>:8765` automaticamente.

## Acceso a la camara en red local

Los navegadores bloquean la camara (`getUserMedia`) en sitios HTTP que no sean `localhost`.
Para usar la app desde otra maquina en la red (ej: pizarra digital), segui estos pasos:

1. Abri Chrome o Edge
2. En la barra de direcciones, escribi: `chrome://flags/#unsafely-treat-insecure-origin-as-secure`
3. En el campo de texto, ingresa la URL del servidor: `http://192.168.1.100:5233`
4. Cambia el dropdown de **Default** a **Enabled**
5. Clic en **Relaunch** para reiniciar el navegador

> La IP y puerto deben coincidir con los de la maquina que hostea el servicio.

## Subida de fotos a Google Drive

El servidor usa **OAuth** (`server/google_drive.py`) para autenticar y subir las imagenes a Drive.

### Flujo

1. El boton "Sacar Foto" captura el avatar via `html2canvas`
2. La imagen PNG se envia por WebSocket al servidor Python
3. El servidor autentica con `credentials.json` + `token.json` y sube la imagen a la carpeta configurada
4. El frontend recibe confirmacion con la URL de la imagen en Drive

### Verificar que funciona

1. Inicia el servidor: `python3 server.py`
2. Si `credentials.json` no existe o no tiene credenciales validas, el servidor mostrara un error al iniciar
3. La primera subida abrira el navegador para autorizar el acceso a Drive
4. Si `folder_id` no esta configurado, la imagen se subira a la raiz de Drive (sin carpeta destino)
5. Con todo configurado, al presionar "Sacar Foto" la imagen debe aparecer en la carpeta de Drive
