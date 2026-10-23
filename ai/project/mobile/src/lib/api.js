import { Platform } from "react-native";

const HOST = Platform.OS === "android" ? "10.0.2.2" : "127.0.0.1";

export const API_URL = `http://${HOST}:8000`;

async function request(path, options = {}) {
  let response;

  try {
    response = await fetch(`${API_URL}${path}`, {
      headers: { "Content-Type": "application/json" },
      ...options,
    });
  } catch {
    throw new Error("Сервер недоступен. Запущен ли backend?");
  }

  const payload = await response.json().catch(() => null);

  if (!response.ok) {
    const detail = payload?.detail;
    throw new Error(
      typeof detail === "string" ? detail : `Ошибка сервера (${response.status})`
    );
  }

  return payload;
}

export const api = {
  health: () => request("/health"),
  features: () => request("/features"),
  metrics: () => request("/metrics"),
  predict: (day) => request("/predict", { method: "POST", body: JSON.stringify(day) }),
  insights: (days) =>
    request("/insights", { method: "POST", body: JSON.stringify({ days }) }),
};
