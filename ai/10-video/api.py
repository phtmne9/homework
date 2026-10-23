import shutil
import tempfile
from pathlib import Path

from fastapi import FastAPI, File, HTTPException, UploadFile
from fastapi.responses import FileResponse

import video_tools as vt

BASE = Path(__file__).resolve().parent
ALLOWED = {".mp4", ".avi", ".mov", ".mkv"}
MAX_BYTES = 50 * 1024 * 1024

app = FastAPI(
    title="Video Processing API",
    description="Чтение видео, извлечение кадров, выделение границ и детекция движения",
    version="1.0",
)


def save_upload(file: UploadFile) -> Path:
    suffix = Path(file.filename or "").suffix.lower()

    if suffix not in ALLOWED:
        raise HTTPException(400, f"Поддерживаются форматы: {', '.join(sorted(ALLOWED))}")

    tmp = Path(tempfile.mkdtemp()) / f"input{suffix}"

    with tmp.open("wb") as out:
        copied = shutil.copyfileobj(file.file, out, length=1024 * 1024)

    if tmp.stat().st_size > MAX_BYTES:
        tmp.unlink()
        raise HTTPException(400, "Файл больше 50 МБ")

    return tmp


@app.get("/")
def root():
    return {"status": "ok", "endpoints": ["/info", "/frames", "/edges", "/motion"]}


@app.get("/health")
def health():
    return {"status": "OK", "opencv": True}


@app.post("/info")
async def info(file: UploadFile = File(...)):
    path = save_upload(file)
    try:
        return vt.video_info(path)
    except ValueError as e:
        raise HTTPException(400, str(e))


@app.post("/frames")
async def frames(file: UploadFile = File(...), every: int = 30):
    if every < 1:
        raise HTTPException(400, "Параметр every должен быть больше нуля")

    path = save_upload(file)
    try:
        saved = vt.extract_frames(path, path.parent / "frames", every=every)
    except ValueError as e:
        raise HTTPException(400, str(e))

    return {"saved": len(saved), "every": every,
            "files": [Path(f).name for f in saved]}


@app.post("/edges")
async def edges(file: UploadFile = File(...)):
    path = save_upload(file)
    out = path.parent / "edges.mp4"

    try:
        result = vt.process_edges(path, out)
    except ValueError as e:
        raise HTTPException(400, str(e))

    return FileResponse(result["output"], media_type="video/mp4", filename="edges.mp4")


@app.post("/motion")
async def motion(file: UploadFile = File(...), min_area: int = 600):
    path = save_upload(file)

    try:
        result = vt.detect_motion(path, min_area=min_area)
    except ValueError as e:
        raise HTTPException(400, str(e))

    return {
        "total_frames": result["total_frames"],
        "frames_with_motion": result["frames_with_motion"],
        "motion_ratio": round(result["frames_with_motion"] / max(result["total_frames"], 1), 3),
        "events": result["events"][:50],
    }
