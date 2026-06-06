## Why

La guia de despliegue actual (`DEPLOY.md`) tiene una seccion de "Publicacion" con `dotnet publish` que no forma parte del flujo real de desarrollo/ejecucion. El proyecto se ejecuta en modo desarrollo con `dotnet run` (Blazor) + `pip install -r requirements.txt && python3 server.py` (Python), y estos pasos no estan claramente reflejados como el flujo principal.

Ademas, el proyecto hoy solo soporta autenticacion de Google Drive via Service Account (`credentials.json` + `DRIVE_FOLDER_ID`), pero las credenciales provistas para la carpeta de Drive son `API_KEY` y `DRIVE_FOLDER_URL`. El servidor no tiene soporte para cargar estas credenciales.

## What Changes

- **DEPLOY.md reescrito**: Se elimina la seccion "Publicacion", se reorganiza el documento para reflejar el flujo real de instalacion y ejecucion, se agrega la configuracion via archivo `.env`.
- **Soporte para archivo `.env` en el servidor**: Se agrega `python-dotenv` como dependencia y se carga automaticamente al iniciar `server.py`.
- **Autenticacion Drive via API_KEY**: `google_drive.py` se extiende para soportar tanto Service Account (`credentials.json` + `DRIVE_FOLDER_ID`) como API Key + folder URL (`API_KEY` + `DRIVE_FOLDER_URL` desde `.env`), detectando automaticamente el metodo disponible.
- **Template `.env.example`**: Se crea un archivo de ejemplo para que el usuario sepa que variables configurar.
- **`.gitignore` actualizado**: Se agrega `.env` para evitar commitear credenciales.

## Capabilities

### Modified Capabilities

- `drive-upload`: Se extiende para soportar autenticacion alternativa via API_KEY + DRIVE_FOLDER_URL, ademas del metodo existente con Service Account.

### New Capabilities

- `env-config`: El servidor Python carga variables de entorno desde un archivo `.env` al inicio (via `python-dotenv`).

## Impact

- `DEPLOY.md` — reescritura completa de la guia de despliegue
- `server/requirements.txt` — agregar `python-dotenv`
- `server/server.py` — cargar `.env` al inicio con `load_dotenv()`
- `server/google_drive.py` — soportar API_KEY + DRIVE_FOLDER_URL como alternativa a Service Account
- `server/.env.example` — nuevo archivo template (no incluye valores reales)
- `.gitignore` — agregar `.env`
