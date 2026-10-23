# %% [markdown]
# # Урок 9. Анализ фотографии через Computer Vision
#
# Практическая работа состоит из двух частей. Сначала фотография разбирается
# классическими методами OpenCV: перевод в оттенки серого, размытие, выделение
# границ алгоритмом Canny, поиск контуров и сбор таблицы признаков. Затем та же
# фотография подаётся в готовую модель YOLO, и результаты двух подходов
# сравниваются.

# %%
import cv2
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd

pd.set_option("display.width", 130)
pd.set_option("display.max_columns", 20)


def show(image, title, cmap=None, size=(7, 8)):
    plt.figure(figsize=size)
    plt.imshow(image, cmap=cmap)
    plt.title(title)
    plt.axis("off")
    plt.tight_layout()
    plt.show()

# %% [markdown]
# ## 1. Загрузка изображения
#
# OpenCV читает картинку в порядке каналов BGR, а matplotlib ожидает RGB,
# поэтому перед показом каналы переставляются.

# %%
image = cv2.imread("photo.jpg")
print("Размер изображения:", image.shape)
print("Высота:", image.shape[0], "| Ширина:", image.shape[1], "| Каналов:", image.shape[2])

rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
show(rgb, "Исходное изображение")

# %% [markdown]
# ## 2. Перевод в grayscale
#
# Цвет для поиска границ не нужен: важен перепад яркости. Один канал вместо трёх
# упрощает обработку и ускоряет её втрое.

# %%
gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
print("Размер после перевода:", gray.shape)
show(gray, "Grayscale", cmap="gray")

# %% [markdown]
# ## 3. Размытие
#
# Гауссово размытие убирает мелкий шум и текстуру. Без него Canny найдёт тысячи
# ложных границ на каждой неровности.

# %%
blurred = cv2.GaussianBlur(gray, (7, 7), 0)
show(blurred, "После размытия (GaussianBlur 7x7)", cmap="gray")

# %% [markdown]
# ## 4. Границы объектов через Canny

# %%
edges = cv2.Canny(blurred, threshold1=50, threshold2=150)
print("Пикселей-границ:", int((edges > 0).sum()))
show(edges, "Границы (Canny)", cmap="gray")

# %% [markdown]
# ## 5. Поиск контуров и рамки
#
# Canny часто оставляет границу разорванной: тень или блик разрезают контур объекта
# на куски. Перед поиском контуров границы расширяются морфологической операцией
# `dilate`, которая смыкает мелкие разрывы, иначе один объект распадётся на десятки
# обрывков.
#
# Дальше контуры фильтруются по площади и минимальной стороне: мелкие пятна шума
# отбрасываются, остаются крупные осмысленные области.

# %%
kernel = np.ones((5, 5), np.uint8)
closed = cv2.dilate(edges, kernel, iterations=2)
closed = cv2.morphologyEx(closed, cv2.MORPH_CLOSE, kernel)
show(closed, "Границы после смыкания разрывов", cmap="gray")

# %%
contours, _ = cv2.findContours(closed, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
print("Найдено контуров всего:", len(contours))

MIN_AREA = 600
MIN_SIDE = 20

kept = []
for contour in contours:
    area = cv2.contourArea(contour)
    x, y, w, h = cv2.boundingRect(contour)

    if area < MIN_AREA or w < MIN_SIDE or h < MIN_SIDE:
        continue

    kept.append((contour, x, y, w, h, area))

print("Осталось после фильтрации:", len(kept))

# %%
boxed = rgb.copy()
for i, (_, x, y, w, h, _) in enumerate(kept, 1):
    cv2.rectangle(boxed, (x, y), (x + w, y + h), (37, 108, 223), 3)
    cv2.putText(boxed, str(i), (x + 5, y + 30), cv2.FONT_HERSHEY_SIMPLEX, 1.0, (37, 108, 223), 2)

show(boxed, f"OpenCV: найдено областей — {len(kept)}")

# %% [markdown]
# ## 6. Таблица признаков по найденным областям

# %%
rows = []
crops = []

for i, (contour, x, y, w, h, area) in enumerate(kept, 1):
    crop = gray[y:y + h, x:x + w]
    crops.append(rgb[y:y + h, x:x + w])

    rows.append({
        "region": i,
        "x": x,
        "y": y,
        "width": w,
        "height": h,
        "area": round(area, 1),
        "perimeter": round(cv2.arcLength(contour, True), 1),
        "mean_brightness": round(float(crop.mean()), 1),
    })

features = pd.DataFrame(rows)
features

# %%
features.describe().round(1)

# %% [markdown]
# ### Вырезанные области

# %%
show_count = min(len(crops), 6)
if show_count:
    fig, axes = plt.subplots(1, show_count, figsize=(3 * show_count, 3.5))
    axes = np.atleast_1d(axes)
    for ax, crop, idx in zip(axes, crops, range(1, show_count + 1)):
        ax.imshow(crop)
        ax.set_title(f"Область {idx}")
        ax.axis("off")
    plt.tight_layout()
    plt.show()

# %% [markdown]
# ## 7. Готовая AI-модель YOLO
#
# YOLO обучена на датасете COCO и умеет не просто находить области, а называть
# объекты. Веса `yolov8n.pt` — самая лёгкая версия, её достаточно для демонстрации.

# %%
from ultralytics import YOLO

model = YOLO("yolov8n.pt")
results = model(image, verbose=False)[0]

print("Найдено объектов:", len(results.boxes))

# %%
detections = []
for box in results.boxes:
    x1, y1, x2, y2 = box.xyxy[0].tolist()
    detections.append({
        "object": results.names[int(box.cls[0])],
        "confidence": round(float(box.conf[0]), 3),
        "x1": round(x1), "y1": round(y1),
        "x2": round(x2), "y2": round(y2),
        "width": round(x2 - x1),
        "height": round(y2 - y1),
    })

yolo_table = pd.DataFrame(detections).sort_values("confidence", ascending=False).reset_index(drop=True)
yolo_table

# %%
for row in detections:
    print(f"{row['object']:<12} confidence={row['confidence']:.3f}  "
          f"рамка=({row['x1']}, {row['y1']}) - ({row['x2']}, {row['y2']})")

# %%
yolo_image = rgb.copy()
for row in detections:
    cv2.rectangle(yolo_image, (row["x1"], row["y1"]), (row["x2"], row["y2"]), (60, 141, 64), 3)
    label = f"{row['object']} {row['confidence']:.2f}"
    cv2.putText(yolo_image, label, (row["x1"], max(row["y1"] - 10, 20)),
                cv2.FONT_HERSHEY_SIMPLEX, 0.8, (60, 141, 64), 2)

show(yolo_image, f"YOLO: найдено объектов — {len(detections)}")

# %% [markdown]
# ## 8. Сравнение подходов

# %%
fig, axes = plt.subplots(1, 2, figsize=(13, 8))
axes[0].imshow(boxed)
axes[0].set_title(f"OpenCV — {len(kept)} областей")
axes[0].axis("off")
axes[1].imshow(yolo_image)
axes[1].set_title(f"YOLO — {len(detections)} объектов")
axes[1].axis("off")
plt.tight_layout()
plt.show()

# %%
print(f"OpenCV нашёл областей:   {len(kept)}")
print(f"YOLO нашла объектов:     {len(detections)}")
print(f"\nЧто назвала YOLO: {sorted(set(d['object'] for d in detections))}")
print(f"Средняя уверенность: {np.mean([d['confidence'] for d in detections]):.3f}")

# %% [markdown]
# ## Вывод
#
# **Что смог найти OpenCV.** Классический подход выделил области с сильным
# перепадом яркости: границы объектов, крупные однородные пятна, резкие контрасты
# между предметом и фоном. Он честно ответил на вопрос «где на картинке что-то
# есть», но не имеет ни малейшего представления о том, что именно он нашёл. Для
# OpenCV область с автобусом и область с тенью на асфальте одинаковы: и то, и
# другое — замкнутый контур с определённой площадью и периметром.
#
# **Что смогла определить YOLO.** Модель не просто нашла области, а назвала
# объекты и указала уверенность для каждого. Она разделила людей и транспорт,
# хотя по пикселям эти области ничем принципиально не отличаются от остальных.
#
# **Где были ошибки.** У OpenCV они видны прямо в таблице признаков. Самая крупная
# найденная область занимает практически весь кадр: после смыкания границ контур
# автобуса слился с фоном и краями изображения в один огромный прямоугольник.
# Несколько других областей, наоборот, оказались мелкими фрагментами по 600–1200
# пикселей — это куски разметки и теней, не соответствующие никакому объекту.
#
# То есть обе типичные беды проявились сразу: один объект распадается на куски,
# а разные объекты сливаются в одну область. Результат сильно зависит от
# подобранных вручную порогов Canny, размера ядра размытия и числа итераций
# dilate. До смыкания разрывов фильтр оставлял всего одну область из 824 контуров,
# после — восемь из четырнадцати. Ни одно из этих чисел не отражает реального
# количества объектов на фотографии.
# У YOLO ошибки другого рода: она может пропустить мелкий или частично закрытый
# объект и иногда присваивает невысокую уверенность тому, что человек распознаёт
# мгновенно.
#
# **Почему результаты отличаются.** Это два принципиально разных способа смотреть
# на изображение. OpenCV работает с пикселями по жёстким правилам, которые задал
# программист: найти перепад яркости, замкнуть контур, отфильтровать по площади.
# Никакого знания о мире у него нет. YOLO использует обученную на размеченных
# фотографиях нейросеть: она выучила, как выглядят восемьдесят классов объектов,
# и ищет на картинке знакомые образы, а не просто границы.
#
# **Главный вывод.** OpenCV помогает находить границы и области изображения через
# пиксели и правила, а YOLO использует обученную модель и может определять, что
# именно находится на фотографии. Классические методы остаются полезными там, где
# задача формальная и условия съёмки стабильны: измерить размер детали, найти
# документ на столе, отследить движение. Как только требуется назвать объект,
# нужна обученная модель.
