import { useCallback, useEffect, useState } from "react";

import { LABELS, api, formatDate, percent } from "../api.js";

export default function History({ version, onChange }) {
  const [rows, setRows] = useState([]);
  const [label, setLabel] = useState("");
  const [search, setSearch] = useState("");
  const [sort, setSort] = useState("date");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const load = useCallback(async () => {
    setLoading(true);
    setError("");

    try {
      setRows(await api.history({ label, search, sort, desc: true }));
    } catch (e) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }, [label, search, sort]);

  useEffect(() => {
    load();
  }, [load, version]);

  const remove = async (id) => {
    try {
      await api.remove(id);
      onChange();
    } catch (e) {
      setError(e.message);
    }
  };

  const clearAll = async () => {
    if (!window.confirm("Удалить всю историю анализов?")) return;

    try {
      await api.clear();
      onChange();
    } catch (e) {
      setError(e.message);
    }
  };

  return (
    <section className="card">
      <div className="toolbar">
        <input
          type="search"
          placeholder="Поиск по тексту"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />

        <select value={label} onChange={(event) => setLabel(event.target.value)}>
          <option value="">Все тональности</option>
          <option value="positive">Только positive</option>
          <option value="negative">Только negative</option>
          <option value="neutral">Только neutral</option>
        </select>

        <select value={sort} onChange={(event) => setSort(event.target.value)}>
          <option value="date">По дате</option>
          <option value="score">По уверенности</option>
          <option value="id">По номеру</option>
        </select>

        <button className="ghost" onClick={load}>Обновить</button>
        <button className="danger" onClick={clearAll} disabled={!rows.length}>
          Очистить
        </button>
      </div>

      {error && <div className="error">{error}</div>}

      {loading && <p className="muted">Загрузка…</p>}

      {!loading && !rows.length && <p className="muted">История пуста</p>}

      {!!rows.length && (
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Текст</th>
              <th>Результат</th>
              <th>Уверенность</th>
              <th>Дата</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row.id}>
                <td className="muted">{row.id}</td>
                <td className="text-cell" title={row.text}>{row.text}</td>
                <td>
                  <span className={`badge ${LABELS[row.label].className}`}>
                    {row.label}
                  </span>
                </td>
                <td>{percent(row.score)}</td>
                <td className="muted">{formatDate(row.created_at)}</td>
                <td>
                  <button className="icon" onClick={() => remove(row.id)} title="Удалить">
                    ✕
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}
