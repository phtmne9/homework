export const theme = {
  bg: "#0f1420",
  card: "#171d2b",
  cardSoft: "#1d2435",
  line: "#2a3346",
  ink: "#eef2fa",
  muted: "#8b95ab",
  accent: "#5b8cff",
  high: "#3ecf8e",
  medium: "#e0b341",
  low: "#e5654f",
};

export const classColor = (label) =>
  ({ high: theme.high, medium: theme.medium, low: theme.low }[label] || theme.muted);

export const classTitle = (label) =>
  ({ high: "Хорошее", medium: "Среднее", low: "Низкое" }[label] || label);
