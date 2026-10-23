from pathlib import Path

import cv2
import numpy as np


def video_info(path):
    cap = cv2.VideoCapture(str(path))

    if not cap.isOpened():
        raise ValueError(f"Видео не открылось: {path}")

    info = {
        "fps": round(cap.get(cv2.CAP_PROP_FPS), 2),
        "width": int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)),
        "height": int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT)),
        "frame_count": int(cap.get(cv2.CAP_PROP_FRAME_COUNT)),
    }
    info["duration_sec"] = round(info["frame_count"] / info["fps"], 2) if info["fps"] else 0

    cap.release()

    return info


def extract_frames(path, out_dir, every=30):
    out = Path(out_dir)
    out.mkdir(parents=True, exist_ok=True)

    cap = cv2.VideoCapture(str(path))
    if not cap.isOpened():
        raise ValueError(f"Видео не открылось: {path}")

    saved, number = [], 0
    while True:
        ok, frame = cap.read()
        if not ok:
            break

        if number % every == 0:
            name = out / f"frame_{len(saved):03d}.jpg"
            cv2.imwrite(str(name), frame)
            saved.append(str(name))

        number += 1

    cap.release()

    return saved


def process_edges(path, out_path):
    cap = cv2.VideoCapture(str(path))
    if not cap.isOpened():
        raise ValueError(f"Видео не открылось: {path}")

    fps = cap.get(cv2.CAP_PROP_FPS)
    size = (int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)), int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT)))
    writer = cv2.VideoWriter(str(out_path), cv2.VideoWriter_fourcc(*"mp4v"), fps, size)

    frames = 0
    while True:
        ok, frame = cap.read()
        if not ok:
            break

        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        edges = cv2.Canny(cv2.GaussianBlur(gray, (5, 5), 0), 60, 160)
        writer.write(cv2.cvtColor(edges, cv2.COLOR_GRAY2BGR))
        frames += 1

    cap.release()
    writer.release()

    return {"output": str(out_path), "frames": frames}


def detect_motion(path, min_area=600):
    cap = cv2.VideoCapture(str(path))
    if not cap.isOpened():
        raise ValueError(f"Видео не открылось: {path}")

    subtractor = cv2.createBackgroundSubtractorMOG2(detectShadows=False)
    kernel = np.ones((5, 5), np.uint8)

    events, number, preview = [], 0, None
    while True:
        ok, frame = cap.read()
        if not ok:
            break

        mask = subtractor.apply(frame)
        mask = cv2.morphologyEx(mask, cv2.MORPH_OPEN, kernel)
        mask = cv2.dilate(mask, kernel, iterations=2)

        contours, _ = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        boxes = [cv2.boundingRect(c) for c in contours if cv2.contourArea(c) >= min_area]

        if boxes:
            events.append({"frame": number, "objects": len(boxes),
                           "area": int(sum(w * h for _, _, w, h in boxes))})

            if preview is None and number > 30:
                preview = frame.copy()
                for x, y, w, h in boxes:
                    cv2.rectangle(preview, (x, y), (x + w, y + h), (0, 160, 255), 2)

        number += 1

    cap.release()

    return {"total_frames": number, "frames_with_motion": len(events),
            "events": events, "preview": preview}
