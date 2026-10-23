import { useEffect, useState } from "react";

import { api, percent } from "../api.js";

export default function Stats({ version }) {
  const [data, setData] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    api.stats().then(setData).catch((e) => setError(e.message));
  }, [version]);

  if (error) return <section className="card"><div className="error">{error}</div></section>;

  if (!data) return <section className="card"><p className="muted">Загрузка…</p></section>;

  const parts = [
    { key: "positive", title: "Positive", value: data.positive },
    { key: "negative", title: "Negative", value: data.negative },
    { key: "neutral", title: "Neutral", value: data.neutral },
  ];

  return (
    <section className="card">
      <div className="totals">
        <div className="total">
          <span className="total-value">{data.total}</span>
          <span className="total-title">Всего анализов</span>
        </div>
        <div className="total">
          <span className="total-value">
            {data.total ? percent(data.average_score) : "—"}
          </span>
          <span className="total-title">Средняя уверенность</span>
        </div>
      </div>

      <div className="counts">
        {parts.map((part) => (
          <div className={`count ${part.key}`} key={part.key}>
            <span className="count-value">{part.value}</span>
            <span className="count-title">{part.title}</span>
            <div className="count-bar">
              <div
                className={`count-fill ${part.key}`}
                style={{ width: data.total ? `${(part.value / data.total) * 100}%` : "0%" }}
              />
            </div>
          </div>
        ))}
      </div>
    </section>
  );
}
