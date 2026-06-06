import io
import json
import os

from google.oauth2.credentials import Credentials
from google_auth_oauthlib.flow import InstalledAppFlow
from google.auth.transport.requests import Request
from googleapiclient.discovery import build
from googleapiclient.http import MediaIoBaseUpload

SCOPES = ["https://www.googleapis.com/auth/drive.file"]

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
CREDS_PATH = os.path.join(SCRIPT_DIR, "credentials.json")
TOKEN_PATH = os.path.join(SCRIPT_DIR, "token.json")


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


def upload_to_drive(image_bytes: bytes, filename: str) -> dict:
    creds = _get_credentials()
    drive = build("drive", "v3", credentials=creds)

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
