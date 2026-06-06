## Context

El proyecto tiene dos componentes:
1. **Frontend Blazor WASM** (`AvatarExpo/`) — `dotnet run`, escucha en `http://0.0.0.0:5233`
2. **Servidor Python** (`server/`) — WebSocket + MediaPipe en `ws://0.0.0.0:8765`, upload a Google Drive

El cambio anterior (`fix-deploy-and-env-config`) agrego infraestructura `.env`/`python-dotenv` y un path de autenticacion con `API_KEY` + `DRIVE_FOLDER_URL` en `google_drive.py`. Al probar la funcionalidad, se detecto que:

- Google Drive API rechaza `drive.files().create()` cuando se autentica con `developerKey` (API_KEY). Las API Keys solo sirven para acceso read-only a datos publicos. La escritura requiere OAuth 2.0 o Service Account.
- El `DRIVE_FOLDER_URL` en el `.env` existente esta malformado (`https:https://...`), evidencia de que el formato es propenso a errores de tipeo.
- La coexistencia de dos metodos de autenticacion agrega branching innecesario en `google_drive.py` y confunde al usuario sobre cual usar.

**El unico metodo que funciona para upload a Drive es Service Account via `credentials.json`**, que ya existia en el proyecto antes del cambio `.env`.

## Goals / Non-Goals

**Goals:**
- Revertir `google_drive.py` a un solo path de autenticacion: Service Account via `credentials.json`
- Consolidar la configuracion de carpeta destino: leer `folder_id` desde `credentials.json` (campo adicional), con fallback a `DRIVE_FOLDER_ID` env var
- Eliminar toda la infraestructura `.env`: dependencia `python-dotenv`, archivos `.env`/`.env.example`, `load_dotenv()` en `server.py`, entrada en `.gitignore`
- Reescribir `DEPLOY.md` documentando exclusivamente Service Account + `credentials.json`
- Mantener la estructura mejorada del deploy (sin "Publicacion", flujo real, secciones claras)

**Non-Goals:**
- Agregar cualquier otro metodo de autenticacion Drive
- Modificar el frontend Blazor
- Cambiar el flujo de ejecucion del proyecto
- Dockerizar

## Decisions

### Decision 1: Eliminar infraestructura `.env` completamente

**Razon**: `python-dotenv` fue una dependencia agregada para un metodo de autenticacion (API_KEY) que no funciona para upload. No hay razon para mantenerlo.

**Archivos a eliminar/modificar**:
- `server/.env` — eliminar
- `server/.env.example` — eliminar
- `server/requirements.txt` — quitar `python-dotenv>=1.0.0`
- `server/server.py` — quitar `import os`, `from dotenv import load_dotenv`, y `load_dotenv(...)`
- `.gitignore` — quitar `server/.env`

### Decision 2: Un solo path de autenticacion en `google_drive.py`

El codigo actual tiene branching:

```
if API_KEY existe → build(developerKey=api_key)  [NO FUNCIONA para upload]
else             → build(credentials=creds)       [FUNCIONA]
```

Se elimina el branch `API_KEY` por completo. `google_drive.py` vuelve a tener una sola funcion `_get_credentials()` que:
1. Busca `credentials.json` en `server/`
2. Si no existe, lanza error claro con instrucciones
3. Retorna credenciales de Service Account

### Decision 3: folder_id desde `credentials.json`

En lugar de requerir `export DRIVE_FOLDER_ID=...` cada vez, el `credentials.json` puede incluir un campo adicional `folder_id`:

```json
{
  "type": "service_account",
  ...
  "folder_id": "1XVX85o7NKulCYaKY16k2CTETLK4L7Vip"
}
```

`google_drive.py` lo lee asi:
1. Cargar `credentials.json` completo con `json.load()`
2. Extraer `folder_id` del JSON (si existe)
3. Si no, fallback a `os.environ.get("DRIVE_FOLDER_ID")`

Esto consolida toda la configuracion Drive en un solo archivo.

### Decision 4: Estructura del nuevo DEPLOY.md

Secciones:
1. **Requisitos** (sin cambios)
2. **Configuracion inicial** — solo Service Account (crear proyecto GCP, generar key, compartir carpeta, crear `credentials.json`)
3. **Instalacion y Ejecucion** — 1) Servidor Python (`pip install` + `python3 server.py`), 2) Cliente Blazor (`dotnet run`)
4. **Acceso a la camara en red local** (sin cambios)
5. **Subida de fotos a Google Drive** — detalle paso a paso de Service Account

Secciones eliminadas respecto al DEPLOY.md actual:
- "Metodo A: API Key + .env" (completo)
- Referencia a `.env`/`.env.example`
- "Publicacion" (`dotnet publish`) — ya eliminada en el cambio anterior, se mantiene fuera

### Decision 5: credentials.json.example actualizado

El template `credentials.json.example` se actualiza para incluir `folder_id`:

```json
{
  "type": "service_account",
  "project_id": "your-project-id",
  "private_key_id": "your-private-key-id",
  "private_key": "-----BEGIN PRIVATE KEY-----\nYOUR_PRIVATE_KEY_HERE\n-----END PRIVATE KEY-----",
  "client_email": "your-service-account@your-project-id.iam.gserviceaccount.com",
  "client_id": "your-client-id",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "https://www.googleapis.com/robot/...",
  "folder_id": "your-google-drive-folder-id"
}
```

## Risks / Trade-offs

- **[Riesgo nulo] Eliminar soporte API_KEY**: No hay funcionalidad que romper — API_KEY nunca funciono para upload. Nadie depende de ese path.
- **[Riesgo bajo] folder_id en credentials.json**: Google ignora campos extra en el JSON de Service Account. El campo `folder_id` no interfiere con la autenticacion. Si el usuario usa un `credentials.json` sin `folder_id`, el fallback a `DRIVE_FOLDER_ID` mantiene compatibilidad.
- **[Riesgo nulo] DEPLOY.md**: Es documentacion, no afecta runtime.
