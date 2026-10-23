const loading = document.getElementById("loading");
const errorBox = document.getElementById("error");
const resultBox = document.getElementById("result");
const panelOutput = document.getElementById("panelOutput");

const FIELDS = [
  "overall_qual", "overall_cond", "gr_liv_area", "garage_cars", "total_bsmt_sf",
  "first_flr_sf", "year_built", "full_bath", "bedroom_abvgr", "totrms_abvgrd",
];

const METRIC_TITLES = {
  mae: "MAE",
  mse: "MSE",
  rmse: "RMSE",
  r2: "R²",
};

const money = (value) => `$${Number(value).toLocaleString("en-US", {
  maximumFractionDigits: 0,
})}`;

function showLoading(on) {
  loading.hidden = !on;
  document.querySelectorAll("button").forEach((b) => (b.disabled = on));
}

function showError(message) {
  errorBox.textContent = message;
  errorBox.hidden = false;
}

function clearError() {
  errorBox.hidden = true;
}

async function request(path, options = {}) {
  showLoading(true);
  clearError();

  try {
    const response = await fetch(path, options);

    if (!response.ok) {
      let detail = `Server returned ${response.status}`;
      try {
        const payload = await response.json();
        if (payload.detail) {
          detail = typeof payload.detail === "string"
            ? payload.detail
            : JSON.stringify(payload.detail);
        }
      } catch {
        // тело не json, оставляем код ответа
      }
      throw new Error(detail);
    }

    return await response.json();
  } catch (e) {
    showError(
      e instanceof TypeError
        ? "Unable to connect to the server."
        : `Request failed: ${e.message}`
    );
    return null;
  } finally {
    showLoading(false);
  }
}

async function checkApi() {
  const data = await request("/health");
  if (!data) return;

  panelOutput.innerHTML = `
    <div class="status-list">
      <div><span class="key">Status:</span> <span class="ok">${data.status.toUpperCase()}</span></div>
      <div><span class="key">Model loaded:</span> ${data.model_loaded}</div>
      <div><span class="key">Features:</span> ${data.features_count}</div>
    </div>`;
}

async function showFeatures() {
  const data = await request("/features");
  if (!data) return;

  const items = data.features.map((name) => `<li><code>${name}</code></li>`).join("");
  panelOutput.innerHTML = `
    <p class="panel-title">Признаки модели (${data.features.length}):</p>
    <ul class="features">${items}</ul>`;
}

async function modelMetrics() {
  const data = await request("/metrics");
  if (!data) return;

  const cards = Object.entries(data)
    .map(([key, value]) => {
      const title = METRIC_TITLES[key] || key.toUpperCase();
      const shown = key === "r2"
        ? Number(value).toFixed(4)
        : Number(value).toLocaleString("en-US", { maximumFractionDigits: 0 });

      return `<div class="metric"><span class="metric-value">${shown}</span>
              <span class="metric-title">${title}</span></div>`;
    })
    .join("");

  panelOutput.innerHTML = `
    <p class="panel-title">Качество модели на тестовых данных:</p>
    <div class="metrics">${cards}</div>
    <p class="note">MAE и RMSE указаны в долларах, R² — доля объяснённого разброса.</p>`;
}

function collect() {
  const body = {};
  const problems = [];

  for (const id of FIELDS) {
    const input = document.getElementById(id);
    input.classList.remove("invalid");

    const value = Number(input.value);

    if (input.value.trim() === "" || Number.isNaN(value)) {
      input.classList.add("invalid");
      problems.push(id);
      continue;
    }

    body[id] = value;
  }

  return { body, problems };
}

async function predictPrice() {
  resultBox.hidden = true;

  const { body, problems } = collect();

  if (problems.length) {
    showError(`Заполните корректно поля: ${problems.join(", ")}`);
    return;
  }

  const data = await request("/predict", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  if (!data) return;

  resultBox.innerHTML = `
    <h2>Estimated House Price</h2>
    <div class="price">${money(data.predicted_price)}</div>
    <div class="note">Prediction generated using the AI model.</div>`;
  resultBox.hidden = false;
}

document.getElementById("checkApiBtn").addEventListener("click", checkApi);
document.getElementById("showFeaturesBtn").addEventListener("click", showFeatures);
document.getElementById("modelMetricsBtn").addEventListener("click", modelMetrics);
document.getElementById("predictBtn").addEventListener("click", predictPrice);
