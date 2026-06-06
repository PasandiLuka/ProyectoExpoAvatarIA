## Why

El cambio anterior (`fix-deploy-and-env-config`) agrego soporte para `.env` + `python-dotenv` con autenticacion Drive via `API_KEY` + `DRIVE_FOLDER_URL`. Al probarlo, el boton "Sacar Foto" quedo trabado en "SubiendoADrive" sin completar nunca.

**Causa raiz de los errores (analisis de `ErrorSubidaDrive.md`):**

1. **API Key no soporta escritura en Drive**: Google Drive API con `developerKey` (API_KEY) solo permite operaciones read-only. El `drive.files().create()` falla silenciosamente con un error 403/401 que no se propaga correctamente al frontend, dejando el boton en estado "SubiendoADrive" eternamente.

2. **URL de carpeta malformada**: El `DRIVE_FOLDER_URL` en `.env` tiene doble protocolo (`https:https://...`), por lo que la funcion `_extract_folder_id()` nunca matcheaba la regex y devolvia la URL rota completa como folder_id.

3. **Complejidad innecesaria**: Se agrego la dependencia `python-dotenv`, un archivo `.env` duplicado en el flujo de credenciales, y dos caminos de autenticacion que confunden. El proyecto ya tenia soporte para Service Account via `credentials.json` que es el metodo correcto para escritura en Drive.

La solucion es eliminar todo el soporte de `.env`/`API_KEY` y consolidar en un unico metodo: **Service Account via `credentials.json`**, que es el unico que soporta upload de archivos a Drive.

## What Changes

- **Eliminar soporte `.env`**: Remover `python-dotenv` de `requirements.txt`, quitar `load_dotenv()` de `server.py`, borrar `server/.env` y `server/.env.example`, quitar `server/.env` de `.gitignore`.
- **Simplificar `google_drive.py`**: Eliminar el path de autenticacion por API_KEY. Solo queda Service Account via `credentials.json`. La carpeta destino se lee de `credentials.json` (campo `folder_id`) con fallback a la variable de entorno `DRIVE_FOLDER_ID`.
- **Extender `credentials.json.example`**: Agregar campo `folder_id` para que el usuario ponga el ID de la carpeta Drive en el mismo archivo de credenciales.
- **Reescribir `DEPLOY.md`**: Eliminar toda referencia a `.env`/API_KEY. Documentar un solo metodo: Service Account + `credentials.json`. Mantener la estructura mejorada (sin seccion "Publicacion", flujo real de ejecucion).

## Capabilities

### Modified Capabilities

- `drive-upload`: Se simplifica para usar exclusivamente Service Account (`credentials.json`). Se elimina el path de API_KEY.

### Removed Capabilities

- `env-config`: Se elimina el soporte de `.env`/`python-dotenv`. Las credenciales se manejan unicamente via `credentials.json`.

## Impact

- `server/requirements.txt` — quitar `python-dotenv`
- `server/server.py` — quitar `load_dotenv()`, imports de `os`/`dotenv` sobrantes
- `server/google_drive.py` — eliminar path API_KEY, `_extract_folder_id()`, leer `folder_id` desde `credentials.json`
- `server/.env` — **eliminar archivo**
- `server/.env.example` — **eliminar archivo**
- `server/credentials.json.example` — agregar campo `folder_id`
- `.gitignore` — quitar entrada `server/.env`
- `DEPLOY.md` — reescribir documentando solo Service Account
