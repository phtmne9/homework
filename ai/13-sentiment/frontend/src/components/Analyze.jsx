import { useState } from "react";

import { LABELS, api, percent } from "../api.js";

export default function Analyze({ onAnalyzed }) {
  const [text, setText] = useState("");
  const [result, setResult] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const submit = async (event) => {
    event.preventDefault();
    setError("");

    if (!text.trim()) {
      setError("Введите текст для анализа");
      return;
    }

    setLoading(true);

    try {
      const data = await api.analyze(text.trim());
      setResult(data);
      onAnalyzed();
    } catch (e) {
      setError(e.message);
      setResult(null);
    } finally {
      setLoading(false);
    }
  };

  const label = result ? LABELS[result.label] : null;
  const probabilities = result
    ? Object.entries(result.probabilities).sort((a, b) => b[1] - a[1])
    : [];

  return (
    <section className="card">
      <form onSubmit={submit}>
        <label className="field">
          <span>Текст для анализа</span>
          <textarea
            rows={5}
            value={text}
            placeholder="Например: Мне понравился урок, было понятно и интересно."
            onChange={(event) => setText(event.target.value)}
          />
        </label>

        <button type="submit" disabled={loading}>
          {loading ? "Анализируем…" : "Анализировать"}
        </button>
      </form>

      {error && <div className="error">{error}</div>}

      {result && (
        <div className={`result ${label.className}`}>
          <h2>Результат анализа</h2>

          <div className="verdict">
            <span className="badge">{result.label}</span>
            <span className="title">{label.title}</span>
          </div>

          <p className="confidence">Уверенность: {percent(result.score)}</p>

          <div className="bars">
            {probabilities.map(([name, value]) => (
              <div className="bar-row" key={name}>
                <span className="bar-name">{name}</span>
                <div className="bar">
                  <div
                    className={`bar-fill ${LABELS[name].className}`}
                    style={{ width: `${value * 100}%` }}
                  />
                </div>
                <span className="bar-value">{percent(value)}</span>
              </div>
            ))}
          </div>

          <div className="source">
            <strong>Текст:</strong>
            <p>{result.text}</p>
          </div>
        </div>
      )}
    </section>
  );
}
