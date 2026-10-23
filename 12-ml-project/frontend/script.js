const form = document.getElementById("form");
const submit = document.getElementById("submit");
const result = document.getElementById("result");
const price = document.getElementById("price");
const meta = document.getElementById("meta");
const errorBox = document.getElementById("error");
const quality = document.getElementById("quality");

let limits = {};

const money = (value) =>
  new Intl.NumberFormat("ru-RU", { maximumFractionDigits: 0 }).format(value);

async function loadInfo() {
  try {
    const [health, features] = await Promise.all([
      fetch("/health").then((r) => r.json()),
      fetch("/features").then((r) => r.json()),
    ]);

    limits = features.limits;
    quality.textContent =
      `Модель загружена. Качество на тестовых данных: R² = ${health.test_r2}`;
  } catch {
    quality.textContent = "Не удалось получить информацию о модели";
  }
}

function showError(message) {
  errorBox.textContent = message;
  errorBox.hidden = false;
  result.hidden = true;
}

function collect() {
  const data = {};
  const problems = [];

  for (const input of form.querySelectorAll("input")) {
    input.classList.remove("invalid");
    const value = Number(input.value);

    if (input.value.trim() === "" || Number.isNaN(value)) {
      input.classList.add("invalid");
      problems.push(`${input.name}: введите число`);
      continue;
    }

    const limit = limits[input.name];
    if (limit && (value < limit.min || value > limit.max)) {
      input.classList.add("invalid");
      problems.push(`${input.name}: допустимо от ${limit.min} до ${limit.max}`);
      continue;
    }

    data[input.name] = value;
  }

  return { data, problems };
}

form.addEventListener("submit", async (event) => {
  event.preventDefault();
  errorBox.hidden = true;

  const { data, problems } = collect();

  if (problems.length) {
    showError(problems.join("\n"));
    return;
  }

  submit.disabled = true;
  submit.textContent = "Считаем…";

  try {
    const response = await fetch("/predict", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });

    const payload = await response.json();

    if (!response.ok) {
      showError(payload.detail || "Сервер вернул ошибку");
      return;
    }

    price.textContent = `$${money(payload.price_usd)}`;
    meta.textContent =
      `Средняя ошибка модели на тестовых данных — около $${money(payload.model_test_mae_usd)}`;
    result.hidden = false;
  } catch {
    showError("Не удалось связаться с сервером. Запущен ли backend?");
  } finally {
    submit.disabled = false;
    submit.textContent = "Предсказать";
  }
});

loadInfo();
