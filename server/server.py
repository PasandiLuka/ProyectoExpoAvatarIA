import asyncio
import json
import base64
from datetime import datetime, timezone
import cv2
import numpy as np
import mediapipe as mp
from websockets.asyncio.server import serve

from google_drive import upload_to_drive

mp_pose = mp.solutions.pose
mp_face = mp.solutions.face_mesh

pose = mp_pose.Pose(
    static_image_mode=False,
    model_complexity=2,
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5
)

face_mesh = mp_face.FaceMesh(
    static_image_mode=False,
    max_num_faces=1,
    refine_landmarks=True,
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5
)

POSE_LANDMARKS = {
    "ls": mp_pose.PoseLandmark.LEFT_SHOULDER,
    "rs": mp_pose.PoseLandmark.RIGHT_SHOULDER,
    "le": mp_pose.PoseLandmark.LEFT_ELBOW,
    "re": mp_pose.PoseLandmark.RIGHT_ELBOW,
    "lw": mp_pose.PoseLandmark.LEFT_WRIST,
    "rw": mp_pose.PoseLandmark.RIGHT_WRIST,
}

POSE_CONNECTIONS = [
    (11, 12),   # shoulders
    (11, 13), (13, 15),   # left arm
    (12, 14), (14, 16),   # right arm
    (11, 23), (12, 24),   # shoulders to hips
    (23, 24),   # hips
    (23, 25), (25, 27),   # left leg
    (24, 26), (26, 28),   # right leg
    (0, 11), (0, 12),     # nose to shoulders
]

def extract_skeleton(pose_landmarks, frame_width, frame_height):
    landmarks = []
    for lm in pose_landmarks.landmark:
        landmarks.append([round(lm.x, 4), round(lm.y, 4), round(lm.z, 4)])
    return {
        "landmarks": landmarks,
        "connections": POSE_CONNECTIONS,
        "width": frame_width,
        "height": frame_height
    }

def detect_expression(face_landmarks, frame_width, frame_height):
    if face_landmarks is None:
        return "neutral", "neutral"

    p13 = face_landmarks.landmark[13]
    p14 = face_landmarks.landmark[14]
    p61 = face_landmarks.landmark[61]
    p291 = face_landmarks.landmark[291]
    p107 = face_landmarks.landmark[107]
    p336 = face_landmarks.landmark[336]
    p70 = face_landmarks.landmark[70]
    p300 = face_landmarks.landmark[300]
    nose_tip = face_landmarks.landmark[1]

    mouth_width = abs(p61.x - p291.x)
    mouth_height = abs(p13.y - p14.y)

    dist_107_nose = abs(p107.x - nose_tip.x) + abs(p107.y - nose_tip.y)
    dist_336_nose = abs(p336.x - nose_tip.x) + abs(p336.y - nose_tip.y)
    brow_proximity = (dist_107_nose + dist_336_nose) / 2

    left_brow_y = p70.y
    right_brow_y = p300.y
    eye_center_y = (face_landmarks.landmark[159].y + face_landmarks.landmark[386].y) / 2
    brow_height = (left_brow_y + right_brow_y) / 2
    brow_state = "up" if (eye_center_y - brow_height) > 0.04 else "neutral"

    expression = "neutral"

    if mouth_height > 0.04:
        expression = "surprise"
    elif mouth_width > 0.28:
        expression = "smile"
    elif brow_proximity < 0.15:
        expression = "angry"

    return expression, brow_state


def extract_pose(pose_landmarks, frame_width, frame_height):
    p = {}
    for key, lm_id in POSE_LANDMARKS.items():
        lm = pose_landmarks.landmark[lm_id]
        if key in ("ls", "rs"):
            p[key] = [round(lm.x, 4), round(lm.y, 4), round(lm.z, 4)]
        else:
            p[key] = [round(lm.x, 4), round(lm.y, 4)]

    lh_vis = 0.0
    rh_vis = 0.0
    lw_lm = pose_landmarks.landmark[mp_pose.PoseLandmark.LEFT_WRIST]
    rw_lm = pose_landmarks.landmark[mp_pose.PoseLandmark.RIGHT_WRIST]
    lh_vis = round(max(0.0, min(1.0, lw_lm.visibility)), 2) if hasattr(lw_lm, 'visibility') else 1.0
    rh_vis = round(max(0.0, min(1.0, rw_lm.visibility)), 2) if hasattr(rw_lm, 'visibility') else 1.0

    return p, lh_vis, rh_vis


async def handle_connection(websocket):
    print("Client connected")
    try:
        while True:
            try:
                message = await websocket.recv()
            except Exception:
                break

            if isinstance(message, str):
                if message == "ping":
                    await websocket.send("pong")
                    continue

                try:
                    data = json.loads(message)
                    if data.get("type") == "screenshot":
                        img_bytes = base64.b64decode(data["image"].split(",")[-1])
                        filename = data.get("filename", f"avatar-{datetime.now(timezone.utc):%Y%m%d-%H%M%S}.png")
                        try:
                            result = upload_to_drive(img_bytes, filename)
                            await websocket.send(json.dumps({
                                "type": "screenshot_result",
                                "success": result["success"],
                                "url": result.get("url", "")
                            }))
                        except Exception as e:
                            await websocket.send(json.dumps({
                                "type": "screenshot_result",
                                "success": False,
                                "url": ""
                            }))
                            print(f"Screenshot upload error: {e}")
                        continue
                except (json.JSONDecodeError, KeyError):
                    pass
                continue

            while True:
                try:
                    next_msg = await asyncio.wait_for(websocket.recv(), timeout=0.001)
                    if isinstance(next_msg, str):
                        if next_msg == "ping":
                            await websocket.send("pong")
                    else:
                        message = next_msg
                except asyncio.TimeoutError:
                    break
                except Exception:
                    return

            nparr = np.frombuffer(message, np.uint8)
            frame = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

            if frame is None:
                continue

            frame_height, frame_width = frame.shape[:2]
            frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

            pose_result = pose.process(frame_rgb)
            face_result = face_mesh.process(frame_rgb)

            if pose_result.pose_landmarks is None:
                payload = {
                    "p": None,
                    "f": {"exp": "neutral", "brow": "neutral"},
                    "v": {"lh": 0.0, "rh": 0.0},
                    "skeleton": None
                }
                await websocket.send(json.dumps(payload, separators=(',', ':')))
                continue

            p, lh_vis, rh_vis = extract_pose(pose_result.pose_landmarks, frame_width, frame_height)
            expression, brow = detect_expression(
                face_result.multi_face_landmarks[0] if face_result.multi_face_landmarks else None,
                frame_width, frame_height
            )
            skeleton = extract_skeleton(pose_result.pose_landmarks, frame_width, frame_height)

            payload = {
                "p": p,
                "f": {"exp": expression, "brow": brow},
                "v": {"lh": lh_vis, "rh": rh_vis},
                "skeleton": skeleton
            }

            await websocket.send(json.dumps(payload, separators=(',', ':')))

    except Exception as e:
        print(f"Connection error: {e}")
    finally:
        print("Client disconnected")


async def main():
    print("Avatar Tracking Server starting on ws://0.0.0.0:8765")
    async with serve(handle_connection, "0.0.0.0", 8765) as server:
        await server.serve_forever()


if __name__ == "__main__":
    asyncio.run(main())
