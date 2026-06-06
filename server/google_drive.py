import asyncio
import io
import json
import os
import random

from google.oauth2.credentials import Credentials
from google_auth_oauthlib.flow import InstalledAppFlow
from google.auth.transport.requests import Request
from googleapiclient.discovery import build
from googleapiclient.http import build_http, MediaIoBaseUpload

SCOPES = ["https://www.googleapis.com/auth/drive.file"]

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
CREDS_PATH = os.path.join(SCRIPT_DIR, "credentials.json")
TOKEN_PATH = os.path.join(SCRIPT_DIR, "token.json")

PHOTOS_DIR = os.path.abspath(os.path.join(SCRIPT_DIR, "..", "FotosAvatares"))


def _get_credentials():
    if not os.path.isfile(CREDS_PATH):
        raise FileNotFoundError(
            "credentials.json not found in server/. "
            "Download OAuth client secret from Google Cloud Console "
            "and add a 'folder_id' field with your Drive folder ID."
        )

    creds = None
    if os.path.isfile(TOKEN_PATH):
        creds = Credentials.from_authorized_user_file(TOKEN_PATH, SCOPES)

    if not creds or not creds.valid:
        if creds and creds.expired and creds.refresh_token:
            creds.refresh(Request())
        else:
            config = json.load(open(CREDS_PATH, "r"))
            client_config = {
                "installed": config["installed"]
                if "installed" in config
                else config.get("web", {})
            }
            flow = InstalledAppFlow.from_client_config(client_config, SCOPES)
            creds = flow.run_local_server(port=0)

        with open(TOKEN_PATH, "w") as token_file:
            token_file.write(creds.to_json())

    return creds


def _get_folder_id():
    if os.path.isfile(CREDS_PATH):
        try:
            with open(CREDS_PATH, "r") as f:
                config = json.load(f)
            folder_id = config.get("folder_id", "").strip()
            if folder_id:
                return folder_id
        except (json.JSONDecodeError, IOError):
            pass
    return os.environ.get("DRIVE_FOLDER_ID", "")


def _build_drive_service(creds):
    http = build_http()
    http.timeout = 30
    return build("drive", "v3", credentials=creds, http=http)


def mark_pending(png_path: str):
    with open(png_path + ".pending", "w") as f:
        pass


def mark_uploaded(png_path: str):
    pending_path = png_path + ".pending"
    uploaded_path = png_path + ".uploaded"
    if os.path.isfile(pending_path):
        os.rename(pending_path, uploaded_path)


def clear_pending(png_path: str):
    pending_path = png_path + ".pending"
    if os.path.isfile(pending_path):
        os.remove(pending_path)


def _has_uploaded(png_path: str) -> bool:
    return os.path.isfile(png_path + ".uploaded")


def _has_pending(png_path: str) -> bool:
    return os.path.isfile(png_path + ".pending")


def upload_to_drive(image_bytes: bytes, filename: str) -> dict:
    creds = _get_credentials()
    drive = _build_drive_service(creds)

    folder_id = _get_folder_id()

    fh = io.BytesIO(image_bytes)
    media = MediaIoBaseUpload(fh, mimetype="image/png", resumable=True)

    file_metadata = {"name": filename}
    if folder_id:
        file_metadata["parents"] = [folder_id]

    uploaded = drive.files().create(body=file_metadata, media_body=media, fields="id").execute()

    return {
        "success": True,
        "url": f"https://drive.google.com/file/d/{uploaded['id']}/view",
    }


async def upload_to_drive_with_retry(image_bytes: bytes, filename: str, max_retries: int = 5):
    for attempt in range(max_retries):
        try:
            loop = asyncio.get_event_loop()
            result = await loop.run_in_executor(None, upload_to_drive, image_bytes, filename)
            if result["success"]:
                print(f"[Drive] Upload exitoso: {result['url']}")
                return True, result["url"]
        except Exception as e:
            if attempt < max_retries - 1:
                base_delay = min(120, 2 ** (attempt + 2))
                jitter = random.uniform(0, base_delay * 0.5)
                delay = base_delay + jitter
                print(f"[Drive] Intento {attempt + 1} fallo: {e}. Reintentando en {delay:.1f}s...")
                await asyncio.sleep(delay)
    print(f"[Drive] Upload fallo despues de {max_retries} intentos")
    return False, ""


async def resume_pending_uploads():
    if not os.path.isdir(PHOTOS_DIR):
        return

    pending_files = []
    for f in sorted(os.listdir(PHOTOS_DIR)):
        if f.endswith(".png.pending"):
            png_name = f[:-8]
            png_path = os.path.join(PHOTOS_DIR, png_name)
            pending_path = os.path.join(PHOTOS_DIR, f)
            if _has_uploaded(png_path):
                os.remove(pending_path)
            elif os.path.isfile(png_path):
                pending_files.append((png_path, png_name))

    if not pending_files:
        return

    print(f"[Resume] {len(pending_files)} uploads pendientes encontrados")

    semaphore = asyncio.Semaphore(2)

    async def upload_one(png_path, png_name):
        async with semaphore:
            with open(png_path, "rb") as fh:
                img_bytes = fh.read()
            success, drive_url = await upload_to_drive_with_retry(img_bytes, png_name)
            if success:
                mark_uploaded(png_path)
                print(f"[Resume] Upload reanudado exitoso: {png_name} -> {drive_url}")
            else:
                print(f"[Resume] Upload reanudado fallo: {png_name}")

    tasks = [asyncio.create_task(upload_one(p, n)) for p, n in pending_files]
    if tasks:
        print(f"[Resume] {len(tasks)} tareas de reanudacion lanzadas en background")
