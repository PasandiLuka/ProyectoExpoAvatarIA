## 1. Revertir cambios de .env en el servidor

- [x] 1.1 Quitar `python-dotenv>=1.0.0` de `server/requirements.txt`
- [x] 1.2 Quitar `import os`, `from dotenv import load_dotenv`, y `load_dotenv(...)` de `server/server.py`
- [x] 1.3 Eliminar archivo `server/.env`
- [x] 1.4 Eliminar archivo `server/.env.example`
- [x] 1.5 Quitar entrada `server/.env` de `.gitignore`

## 2. Simplificar google_drive.py a solo Service Account

- [x] 2.1 Eliminar funcion `_extract_folder_id()` (ya no se usa)
- [x] 2.2 Eliminar path de API_KEY en `_get_credentials()` — solo retornar credenciales de Service Account
- [x] 2.3 Leer `credentials.json` via `json.load()` para extraer `folder_id` como campo opcional
- [x] 2.4 En `upload_to_drive()`, usar `folder_id` del JSON con fallback a `DRIVE_FOLDER_ID` env var
- [x] 2.5 Eliminar `import re` (solo se usaba en `_extract_folder_id`)

## 3. Actualizar credentials.json.example

- [x] 3.1 Agregar campo `folder_id` al template con valor placeholder

## 4. Reescribir DEPLOY.md

- [x] 4.1 Eliminar seccion "Metodo A: API Key + .env" de "Configuracion inicial"
- [x] 4.2 Eliminar seccion "Metodo A: API Key + .env" de "Subida de fotos"
- [x] 4.3 Eliminar toda referencia a `.env`, `.env.example`, `API_KEY`, `DRIVE_FOLDER_URL`
- [x] 4.4 Reescribir "Configuracion inicial" con solo el flujo Service Account: crear `credentials.json`, poner `folder_id`
- [x] 4.5 Reescribir "Subida de fotos" con paso a paso de Service Account (GCP, clave JSON, compartir carpeta, `credentials.json` con `folder_id`)
- [x] 4.6 Mantener estructura sin "Publicacion", ejecucion clara en 2 pasos

## 5. Verificacion

- [x] 5.1 Sintaxis de `server/server.py` sin errores (`python3 -c "import py_compile; ..."`)
- [x] 5.2 Sintaxis de `server/google_drive.py` sin errores
- [x] 5.3 `python3 server.py` arranca sin errores sin `.env` presente
- [ ] 5.4 `python3 server.py` arranca con `credentials.json` de ejemplo (sin credenciales reales, verifica mensaje de error claro)
- [ ] 5.5 Probar upload a Drive con `credentials.json` real + `folder_id` (si hay credenciales disponibles)
- [x] 5.6 Verificar que `server/.env` no existe y no esta en `.gitignore`
