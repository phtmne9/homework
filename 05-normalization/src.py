# %% [markdown]
# # Урок 5. Нормализация, корреляция, линейная регрессия, метрики и predict
#
# Задача: по таблице недвижимости предсказать `price`.
#
# > **Про датасет.** Файл `properties_data.csv` к заданию приложен не был — в
# > методичке есть только его паспорт. Поэтому таблица сгенерирована скриптом
# > `make_dataset.py` строго по этому паспорту: 1905 строк, 38 колонок,
# > 8 числовых, 2 категориальных (`neighborhood`, `quality`), 28 булевых,
# > без пропусков, целевая переменная `price`.

# %%
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from sklearn.linear_model import LinearRegression
from sklearn.metrics import mean_absolute_error, mean_squared_error, r2_score
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler

pd.set_option("display.width", 140)
pd.set_option("display.max_columns", 40)

df = pd.read_csv("properties_data.csv")
print("Размер таблицы:", df.shape)

# %% [markdown]
# ## 1-2. Первичный анализ

# %%
df.head()

# %%
df.info()

# %%
df.describe()

# %%
print("Пропуски по колонкам:", int(df.isnull().sum().sum()))
df.isnull().sum().head(10)

# %% [markdown]
# ## 3-4. Целевая переменная и отбор признаков
#
# Целевая переменная — `price`.
#
# Удаляем `id`, потому что это технический номер: он не описывает объект, но модель
# может найти в нём случайную закономерность.
#
# Удаляем `price_per_sqft`, потому что он считается как `price / size_in_sqft` и уже
# содержит в себе ответ. Оставить его — значит получить красивые метрики обманом.
# Ниже обе модели сравниваются явно.

# %%
y = df["price"]
X = df.drop(columns=["price", "id", "price_per_sqft"])

X = pd.get_dummies(X, columns=["neighborhood", "quality"], drop_first=True)
X = X.astype({c: int for c in X.select_dtypes("bool").columns})

print("Признаков после кодирования:", X.shape[1])
X.head()

# %% [markdown]
# ## 5. Разделение на train и test

# %%
X_train, X_test, y_train, y_test = train_test_split(
    X, y, test_size=0.2, random_state=42)

print("train:", X_train.shape)
print("test: ", X_test.shape)

# %% [markdown]
# ## 6-7. Корреляция признаков с ценой
#
# Булевы колонки уже переведены в 0/1, поэтому корреляция считается по всей таблице.

# %%
corr = X.join(y).corr(numeric_only=True)["price"].drop("price")
top10 = corr.abs().sort_values(ascending=False).head(10)

pd.DataFrame({
    "feature": top10.index,
    "corr": corr[top10.index].round(3),
    "abs_corr": top10.round(3),
}).reset_index(drop=True)

# %%
plt.figure(figsize=(8, 4.5))
colors = ["#2d6cdf" if corr[f] > 0 else "#d9534f" for f in top10.index]
plt.barh(range(len(top10)), corr[top10.index], color=colors)
plt.yticks(range(len(top10)), top10.index)
plt.gca().invert_yaxis()
plt.xlabel("Корреляция с price")
plt.title("Топ-10 признаков по модулю корреляции")
plt.grid(axis="x", alpha=0.3)
plt.tight_layout()
plt.show()

# %% [markdown]
# ## 8. Нормализация через StandardScaler
#
# Scaler обучается **только на train** и затем применяется к test. Если обучить его
# на всей таблице, в обучение просочится информация о тестовой выборке — это и есть
# data leakage.

# %%
scaler = StandardScaler()
X_train_scaled = scaler.fit_transform(X_train)
X_test_scaled = scaler.transform(X_test)

print("Среднее по train после нормализации:", round(float(X_train_scaled.mean()), 6))
print("Стандартное отклонение:            ", round(float(X_train_scaled.std()), 6))

# %% [markdown]
# ## 9-10. Обучение модели и метрики

# %%
model = LinearRegression()
model.fit(X_train_scaled, y_train)
y_pred = model.predict(X_test_scaled)

mae = mean_absolute_error(y_test, y_pred)
mse = mean_squared_error(y_test, y_pred)
rmse = np.sqrt(mse)
r2 = r2_score(y_test, y_pred)

print(f"MAE:  {mae:,.0f}")
print(f"MSE:  {mse:,.0f}")
print(f"RMSE: {rmse:,.0f}")
print(f"R2:   {r2:.4f}")
print(f"\nСредняя цена в тесте: {y_test.mean():,.0f}")
print(f"Ошибка в процентах от средней цены: {mae / y_test.mean() * 100:.1f}%")

# %% [markdown]
# ## 11. Таблица: реальная цена против предсказанной

# %%
result = pd.DataFrame({
    "real_price": y_test.values,
    "predicted_price": y_pred.round(0),
})
result["error"] = (result["real_price"] - result["predicted_price"]).round(0)
result["abs_error"] = result["error"].abs()
result.head(15)

# %%
print("Статистика ошибки:")
result["abs_error"].describe().round(0)

# %% [markdown]
# ## 12. График: real vs predicted

# %%
plt.figure(figsize=(6.5, 6.5))
plt.scatter(y_test, y_pred, alpha=0.45, s=18, color="#2d6cdf")
limits = [y_test.min(), y_test.max()]
plt.plot(limits, limits, color="#d9534f", linewidth=2, label="идеальное предсказание")
plt.xlabel("Реальная цена")
plt.ylabel("Предсказанная цена")
plt.title("Реальная и предсказанная цена")
plt.legend()
plt.grid(alpha=0.3)
plt.tight_layout()
plt.show()

# %% [markdown]
# ## 13. Predict для одного объекта из теста

# %%
one = X_test.iloc[[0]]
one_scaled = scaler.transform(one)
one_pred = model.predict(one_scaled)[0]

print("Площадь:    ", int(one["size_in_sqft"].iloc[0]))
print("Спален:     ", int(one["no_of_bedrooms"].iloc[0]))
print("Ванных:     ", int(one["no_of_bathrooms"].iloc[0]))
print(f"\nРеальная цена:      {y_test.iloc[0]:,.0f}")
print(f"Предсказанная цена: {one_pred:,.0f}")
print(f"Ошибка:             {abs(y_test.iloc[0] - one_pred):,.0f}")

# %% [markdown]
# ## 14. Predict для нового объекта, созданного вручную

# %%
new_flat = X.iloc[[0]].copy()
new_flat["size_in_sqft"] = 1800
new_flat["no_of_bedrooms"] = 3
new_flat["no_of_bathrooms"] = 3
new_flat["balcony"] = 1
new_flat["private_pool"] = 1
new_flat["view_of_water"] = 1

new_pred = model.predict(scaler.transform(new_flat))[0]
print(f"Новая квартира: 1800 кв. футов, 3 спальни, 3 ванные, бассейн и вид на воду")
print(f"Предсказанная цена: {new_pred:,.0f}")

# %% [markdown]
# ## Проверка: что будет, если оставить price_per_sqft

# %%
X_leak = df.drop(columns=["price", "id"])
X_leak = pd.get_dummies(X_leak, columns=["neighborhood", "quality"], drop_first=True)
X_leak = X_leak.astype({c: int for c in X_leak.select_dtypes("bool").columns})

Xl_train, Xl_test, yl_train, yl_test = train_test_split(
    X_leak, y, test_size=0.2, random_state=42)

leak_scaler = StandardScaler()
leak_model = LinearRegression().fit(leak_scaler.fit_transform(Xl_train), yl_train)
yl_pred = leak_model.predict(leak_scaler.transform(Xl_test))

print(f"Честная модель:          R2 = {r2:.4f}, MAE = {mae:,.0f}")
print(f"С price_per_sqft:        R2 = {r2_score(yl_test, yl_pred):.4f}, "
      f"MAE = {mean_absolute_error(yl_test, yl_pred):,.0f}")

# %% [markdown]
# ## 15. Вывод
#
# Модель получилась рабочей: она объясняет большую часть разброса цен, а средняя
# ошибка составляет около десяти процентов от средней стоимости объекта. Для
# линейной модели на табличных данных это нормальный результат — в тех же единицах
# это означает, что предсказание по типичной квартире промахивается на несколько
# сотен тысяч при цене около двух миллионов.
#
# Самым сильным признаком ожидаемо оказалась площадь: корреляция с ценой у неё
# заметно выше, чем у остальных. Следом идут количество ванных комнат и спален,
# которые по сути тоже отражают размер жилья. Среди районов и уровней качества
# выделяются премиальные категории — они дают устойчивую прибавку к цене. Удобства
# вроде вида на воду и собственного бассейна влияют слабее поодиночке, но вместе
# заметно поднимают стоимость.
#
# Отдельно стоит отметить эксперимент с `price_per_sqft`. С этим признаком метрики
# улучшаются: R² поднимается примерно на две сотых, а средняя ошибка падает
# заметно. Доверять этому нельзя — признак вычисляется из самой цены, то есть
# модель получает часть ответа на вход.
#
# Любопытно, что выигрыш не катастрофический, хотя утечка здесь полная:
# `price = price_per_sqft × size_in_sqft`. Причина в том, что линейная модель умеет
# только складывать признаки с весами, а не перемножать их. Восстановить
# произведение двух колонок она не может и поэтому использует утечку лишь частично.
# Дерево или бустинг на тех же данных показали бы почти идеальный результат — и это
# было бы чистым самообманом.
#
# Что можно улучшить. Линейная модель плохо ловит нелинейность: на дорогих объектах
# она систематически ошибается сильнее, что видно по графику real vs predicted —
# точки справа уходят от диагонали. Помогли бы деревья или градиентный бустинг,
# логарифмирование цены, а также признаки, которых в таблице нет вовсе: этаж, год
# постройки, расстояние до моря и состояние ремонта.
