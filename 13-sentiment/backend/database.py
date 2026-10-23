import sqlite3
from contextlib import contextmanager
from pathlib import Path

DB_PATH = Path(__file__).resolve().parent / "analyses.db"

SCHEMA = """
CREATE TABLE IF NOT EXISTS analyses (
    id         INTEGER PRIMARY KEY AUTOINCREMENT,
    text       TEXT    NOT NULL,
    label      TEXT    NOT NULL,
    score      REAL    NOT NULL,
    created_at TEXT    NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_analyses_created ON analyses (created_at DESC);
"""


@contextmanager
def connection():
    conn = sqlite3.connect(DB_PATH)
    conn.row_factory = sqlite3.Row
    try:
        yield conn
        conn.commit()
    finally:
        conn.close()


def init():
    with connection() as conn:
        conn.executescript(SCHEMA)


def add(text, label, score, created_at):
    with connection() as conn:
        cursor = conn.execute(
            "INSERT INTO analyses (text, label, score, created_at) VALUES (?, ?, ?, ?)",
            (text, label, score, created_at),
        )
        return cursor.lastrowid


def get_all(label=None, search=None, sort="date", desc=True):
    columns = {"date": "created_at", "score": "score", "id": "id"}
    order = columns.get(sort, "created_at")
    direction = "DESC" if desc else "ASC"

    query = "SELECT * FROM analyses"
    params, where = [], []

    if label:
        where.append("label = ?")
        params.append(label)

    if search:
        where.append("text LIKE ?")
        params.append(f"%{search}%")

    if where:
        query += " WHERE " + " AND ".join(where)

    query += f" ORDER BY {order} {direction}"

    with connection() as conn:
        return [dict(row) for row in conn.execute(query, params)]


def get_one(analysis_id):
    with connection() as conn:
        row = conn.execute("SELECT * FROM analyses WHERE id = ?", (analysis_id,)).fetchone()
        return dict(row) if row else None


def delete(analysis_id):
    with connection() as conn:
        return conn.execute("DELETE FROM analyses WHERE id = ?", (analysis_id,)).rowcount


def clear():
    with connection() as conn:
        return conn.execute("DELETE FROM analyses").rowcount


def stats():
    with connection() as conn:
        total = conn.execute("SELECT COUNT(*) AS n FROM analyses").fetchone()["n"]

        if not total:
            return {"total": 0, "average_score": 0.0,
                    "positive": 0, "negative": 0, "neutral": 0}

        average = conn.execute("SELECT AVG(score) AS a FROM analyses").fetchone()["a"]
        rows = conn.execute(
            "SELECT label, COUNT(*) AS n FROM analyses GROUP BY label").fetchall()

        counts = {row["label"]: row["n"] for row in rows}

        return {
            "total": total,
            "average_score": round(average, 4),
            "positive": counts.get("positive", 0),
            "negative": counts.get("negative", 0),
            "neutral": counts.get("neutral", 0),
        }
