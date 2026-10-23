from datetime import datetime, timezone

from fastapi import FastAPI, HTTPException, Query
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from transformers import pipeline

import database

MODEL_NAME = "cointegrated/rubert-tiny-sentiment-balanced"
MAX_LENGTH = 2000

app = FastAPI(
    title="AI Sentiment Analyzer",
    description="Анализ тональности текста готовой моделью, история в SQLite",
    version="1.0",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:5173", "http://127.0.0.1:5173"],
    allow_methods=["GET", "POST", "DELETE"],
    allow_headers=["*"],
)

classifier = pipeline("sentiment-analysis", model=MODEL_NAME, top_k=None)
database.init()


class AnalyzeRequest(BaseModel):
    text: str = Field(..., min_length=1, max_length=MAX_LENGTH)


class AnalyzeResponse(BaseModel):
    id: int
    text: str
    label: str
    score: float
    created_at: str
    probabilities: dict[str, float]


@app.get("/")
def root():
    return {"status": "ok", "model": MODEL_NAME, "labels": ["positive", "negative", "neutral"]}


@app.post("/analyze", response_model=AnalyzeResponse)
def analyze(request: AnalyzeRequest):
    text = request.text.strip()

    if not text:
        raise HTTPException(400, "Текст не может быть пустым")

    scores = classifier(text[:MAX_LENGTH])[0]
    best = max(scores, key=lambda item: item["score"])
    created_at = datetime.now(timezone.utc).isoformat(timespec="seconds")

    analysis_id = database.add(text, best["label"], float(best["score"]), created_at)

    return AnalyzeResponse(
        id=analysis_id,
        text=text,
        label=best["label"],
        score=round(float(best["score"]), 4),
        created_at=created_at,
        probabilities={item["label"]: round(float(item["score"]), 4) for item in scores},
    )


@app.get("/analyses")
def analyses(
    label: str | None = Query(None, pattern="^(positive|negative|neutral)$"),
    search: str | None = Query(None, max_length=200),
    sort: str = Query("date", pattern="^(date|score|id)$"),
    desc: bool = True,
):
    return database.get_all(label=label, search=search, sort=sort, desc=desc)


@app.get("/analyses/{analysis_id}")
def analysis(analysis_id: int):
    found = database.get_one(analysis_id)

    if not found:
        raise HTTPException(404, f"Анализ {analysis_id} не найден")

    return found


@app.delete("/analyses/{analysis_id}")
def remove(analysis_id: int):
    if not database.delete(analysis_id):
        raise HTTPException(404, f"Анализ {analysis_id} не найден")

    return {"deleted": analysis_id}


@app.delete("/analyses")
def clear():
    return {"deleted": database.clear()}


@app.get("/stats")
def stats():
    return database.stats()
