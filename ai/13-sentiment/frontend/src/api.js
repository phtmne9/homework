const BASE = "/api";

async function request(path, options = {}) {
  const response = await fetch(`${BASE}${path}`, {
    headers: { "Content-Type": "application/json" },
    ...options,
  });

  const payload = await response.json().catch(() => null);

  if (!response.ok) {
    throw new Error(payload?.detail || `Ошибка сервера (${response.status})`);
  }

  return payload;
}

export const api = {
  analyze: (text) =>
    request("/analyze", { method: "POST", body: JSON.stringify({ text }) }),

  history: ({ label, search, sort, desc } = {}) => {
    const params = new URLSearchParams();
    if (label) params.set("label", label);
    if (search) params.set("search", search);
    if (sort) params.set("sort", sort);
    if (desc !== undefined) params.set("desc", String(desc));

    const query = params.toString();
    return request(`/analyses${query ? `?${query}` : ""}`);
  },

  remove: (id) => request(`/analyses/${id}`, { method: "DELETE" }),

  clear: () => request("/analyses", { method: "DELETE" }),

  stats: () => request("/stats"),
};

export const LABELS = {
  positive: { title: "Позитивная", className: "positive" },
  negative: { title: "Негативная", className: "negative" },
  neutral: { title: "Нейтральная", className: "neutral" },
};

export const formatDate = (value) =>
  new Date(value).toLocaleString("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });

export const percent = (value) => `${(value * 100).toFixed(1)}%`;
