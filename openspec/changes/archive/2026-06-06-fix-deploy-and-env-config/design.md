## Context

El proyecto es un kiosko de avatar interactivo con dos componentes:
1. **Frontend Blazor WASM** (`AvatarExpo/`) — ejecutado con `dotnet run`, escucha en `http://localhost:5233` (development) o `http://0.0.0.0:5233`
2. **Servidor Python** (`server/`) — WebSocket server con MediaPipe, escucha en `ws://0.0.0.0:8765`, con upload a Google Drive integrado

El flujo actual de ejecucion es manual: dos terminales, una con `dotnet run` y otra con `python3 server.py`. No hay Docker, no hay publish a produccion.

El sistema de upload a Drive actualmente solo soporta Service Account (`credentials.json` + env var `DRIVE_FOLDER_ID`). Sin embargo, las credenciales provistas para el proyecto son:
- `API_KEY`: clave de API de Google (string)
- `DRIVE_FOLDER_URL`: URL completa de la carpeta de Drive (ej: `https://drive.google.com/drive/folders/1a2b3c4d5e...`)

El metodo de autenticacion con API_KEY es mas simple que Service Account: no requiere archivo JSON de credenciales, no requiere asignar permisos a una cuenta de servicio. La API_KEY se pasa directamente al construir el cliente de Drive. La carpeta destino se extrae del `DRIVE_FOLDER_URL` parseando el ID al final de la URL.

## Goals / Non-Goals

**Goals:**
- Reescribir `DEPLOY.md` para reflejar el flujo real de instalacion y ejecucion (sin publish)
- Agregar soporte para archivo `.env` en el servidor Python usando `python-dotenv`
- Extender `google_drive.py` para soportar autenticacion via `API_KEY` + `DRIVE_FOLDER_URL` como alternativa
- Mantener compatibilidad con el metodo existente de Service Account (no romper lo que ya funciona)
- Crear `server/.env.example` como template
- Agregar `.env` a `.gitignore`

**Non-Goals:**
- Dockerizar la aplicacion
- Agregar `dotnet publish` o despliegue a produccion
- Cambiar el puerto del servidor o del frontend
- Modificar el flujo de build del Blazor WASM
- Agregar autenticacion OAuth interactiva

## Decisions

### Decision 1: python-dotenv para cargar .env

**Se elige `python-dotenv`** porque:
- Es la libreria estandar de facto para cargar `.env` en Python (~8k stars)
- API minima: `load_dotenv()` al inicio de `server.py`
- No requiere cambios en el resto del codigo — `os.environ.get()` sigue funcionando igual
- Sin dependencias pesadas (puro Python, ~50KB)

### Decision 2: Deteccion automatica de metodo de autenticacion Drive

`google_drive.py` debe detectar automaticamente que metodo usar:

1. Si existe `API_KEY` en el entorno → usar API Key authentication
   - Extraer folder ID de `DRIVE_FOLDER_URL` (ultimo segmento despues de `/folders/`)
   - Construir cliente Drive con `build('drive', 'v3', developerKey=API_KEY)`

2. Sino, si existe `credentials.json` → usar Service Account (metodo actual, sin cambios)
   - Usar `DRIVE_FOLDER_ID` del entorno (variable legacy)
   - Autenticar con `service_account.Credentials.from_service_account_file()`

3. Si no hay ningun metodo configurado → error claro

**Nota**: La API_KEY de Google tipicamente es para acceso read-only. Para upload de archivos puede requerir configuracion adicional. Si no funciona, se documentara la limitacion y se recomendara el metodo de Service Account.

### Decision 3: DRIVE_FOLDER_URL parsing

El `DRIVE_FOLDER_URL` tiene formato: `https://drive.google.com/drive/folders/{FOLDER_ID}?usp=...`

Se extrae el `FOLDER_ID` con una regex simple: `r"/folders/([a-zA-Z0-9_-]+)"`. Si no se encuentra, se usa el valor completo como fallback (por si el usuario pasa solo el ID en vez de la URL).

### Decision 4: Estructura del nuevo DEPLOY.md

El nuevo `DEPLOY.md` tendra las siguientes secciones:

1. **Requisitos** (sin cambios)
2. **Configuracion inicial** — archivo `.env`, credenciales Drive (ambos metodos)
3. **Instalacion** — `pip install -r requirements.txt`
4. **Ejecucion** — pasos claros en orden: 1) servidor Python, 2) frontend Blazor
5. **Acceso desde red local** — flags de Chrome (sin cambios)
6. **Subida de fotos a Google Drive** — explicacion de ambos metodos (API_KEY via `.env`, o Service Account via `credentials.json`)

Secciones eliminadas:
- "Publicacion" (`dotnet publish`) — no forma parte del flujo actual

### Decision 5: .env.example contenido

```bash
# Google Drive API Key
API_KEY=tu_api_key_aqui

# URL de la carpeta de Drive donde se guardan las fotos
DRIVE_FOLDER_URL=https://drive.google.com/drive/folders/tu_folder_id
```

## Risks / Trade-offs

- **[Riesgo bajo] API_KEY puede no soportar escritura**: Las API Keys de Google tipicamente son para acceso read-only. **Mitigacion**: Se intentara el upload con API_KEY; si falla, se documentara y se recomendara Service Account.
- **[Riesgo bajo] Compatibilidad con codigo existente**: Los cambios agregan un nuevo camino de codigo sin modificar el existente. Si `API_KEY` no esta seteado, el comportamiento es identico al actual.
- **[Riesgo nulo] DEPLOY.md**: Es documentacion, no afecta funcionalidad.
