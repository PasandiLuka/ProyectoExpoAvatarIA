## 1. Soporte .env en el servidor Python

- [x] 1.1 Agregar `python-dotenv` a `server/requirements.txt`
- [x] 1.2 Agregar `from dotenv import load_dotenv` y `load_dotenv()` al inicio de `server/server.py` (antes de cualquier `os.environ.get`)
- [x] 1.3 Crear `server/.env.example` con template de variables: `API_KEY`, `DRIVE_FOLDER_URL`
- [x] 1.4 Agregar `.env` a `.gitignore` del repositorio

## 2. Autenticacion Drive via API_KEY + DRIVE_FOLDER_URL

- [x] 2.1 Extender `server/google_drive.py` para detectar metodo de autenticacion:
  - Si `API_KEY` existe en entorno → usar API Key + extraer folder ID de `DRIVE_FOLDER_URL`
  - Sino → usar Service Account como antes (mantener compatibilidad)
- [x] 2.2 Implementar funcion `_extract_folder_id(url)` que parsea el ID de una URL de Drive (`/folders/{ID}`)
- [x] 2.3 Ajustar `upload_to_drive()` para soportar el nuevo flujo con API Key

## 3. Reescritura de DEPLOY.md

- [x] 3.1 Eliminar seccion "Publicacion" (`dotnet publish`)
- [x] 3.2 Agregar seccion "Configuracion inicial" con instrucciones para archivo `.env` (copiar `.env.example`, completar valores, mover a `server/`)
- [x] 3.3 Reorganizar "Instalacion y Ejecucion" en dos subsecciones claras:
  - "1. Servidor Python" (`pip install -r requirements.txt` + `python3 server.py`)
  - "2. Cliente Blazor" (`dotnet run`)
- [x] 3.4 Actualizar seccion "Subida de fotos" documentando ambos metodos de autenticacion:
  - Metodo A: API_KEY + DRIVE_FOLDER_URL (via `.env`)
  - Metodo B: Service Account (via `credentials.json` + `DRIVE_FOLDER_ID`)
- [x] 3.5 Agregar nota sobre `.env.example` como referencia

## 4. Verificacion

- [x] 4.1 Probar que `pip install -r requirements.txt` instala `python-dotenv` sin errores
- [x] 4.2 Probar que `python3 server.py` arranca sin errores con `.env` presente en `server/`
- [x] 4.3 Probar que `python3 server.py` arranca sin errores sin `.env` (fallback a Service Account)
- [x] 4.4 Probar upload a Drive con API_KEY + DRIVE_FOLDER_URL
- [x] 4.5 Probar upload a Drive con Service Account (regresion)
- [x] 4.6 Verificar que `.env` esta en `.gitignore` y no se commitea
