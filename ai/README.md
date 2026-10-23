# Создание AI-приложений с использованием Python

Домашние работы по предмету. Студент: Плисак Арсений Сергеевич.

| Папка | Дата | Тема | Что внутри |
|---|---|---|---|
| `02-oop` | 08.06 | Структуры данных и ООП | `ReviewAnalyzer`, `StudentAnalyzer`, ответы на теорию и SOLID |
| `03-numpy-pandas` | 10.06 | NumPy и Pandas | Ноутбук: массивы, DataFrame, groupby, мини-аналитика |
| `04-regression` | 11.06 | Линейная регрессия | Теория, баллы студентов, своя задача про цены машин |
| `05-normalization` | 15.06 | Нормализация и корреляция | StandardScaler, data leakage, predict по недвижимости |
| `06-classification` | 17.06 | Классификация | LogisticRegression и KNN, метрики, confusion matrix |
| `07-trees` | 18.06 | KNN, Decision Tree, Random Forest | Подбор K, max_depth, n_estimators, визуализация дерева |
| `08-ml-pipeline` | 22.06 | Полный ML-пайплайн | Регрессия и классификация на California Housing, 20 заданий |
| `09-cv` | 24.06 | Computer Vision | OpenCV-пайплайн и сравнение с YOLO |
| `10-video` | 25.06 | Видео и FastAPI | Четыре операции над видео плюс API |
| `10-pytorch` | 29.06 | Нейросети и PyTorch | Три уровня: эпохи, архитектуры, FastAPI-эндпоинт |
| `11-web-ui` | 01.07 | Модель и веб-интерфейс | Check API, Show Features, Model Metrics, загрузка и ошибки |
| `12-ml-project` | 02.07 | Full ML Project | Датасет, обучение, модель, backend и статический фронтенд |
| `13-sentiment` | 08.07 | Использование готовых моделей | AI Sentiment Analyzer: FastAPI, SQLite, React |

## Окружение

```
python -m venv .venv
.venv/bin/pip install numpy pandas scikit-learn matplotlib openpyxl jupyter \
    opencv-python-headless ultralytics transformers fastapi uvicorn python-multipart
.venv/bin/pip install --index-url https://download.pytorch.org/whl/cpu torch torchvision
```

PyTorch ставится из CPU-индекса намеренно: на машине нет видеокарты NVIDIA, а
обычная установка тянет несколько гигабайт пакетов CUDA впустую.

## Ноутбуки

Код работ написан в `src.py` в формате ячеек, а `.ipynb` собирается и выполняется
скриптом `_tools/mknb.py`:

```
.venv/bin/python _tools/mknb.py 04-regression/src.py 04-regression/urok4.ipynb
```

Ноутбуки в репозитории лежат уже выполненными — со всеми графиками и выводами.

## Где лежали условия уроков 8 и 11

У этих двух заданий во вложениях пусто, а условие указано в поле комментария к
заданию: оно отсылает к репозиторию преподавателя
`github.com/madiardzetibergen-hash/PA521`.

* Урок 8 — файл `8lesson/8 урок самостоятельная работа.docx`.
* Урок 11 — папка `house-price-ai`, она же основа проекта. Бэкенд и обученная
  модель взяты оттуда без изменений, написан фронтенд.

## Задания, которые не выполнены

**Сдача проектов от 13.07.** Итоговый проект, делается отдельно.

## Сгенерированные датасеты

К урокам 5 и 6 нужны файлы `properties_data.csv` и
`student_exam_classification.xlsx`, которых нет во вложениях — в методичках есть
только их паспорта. Датасеты сгенерированы скриптами `make_dataset.py` строго по
этим паспортам: совпадают число строк, состав и типы колонок, баланс классов.
В начале каждого ноутбука это оговорено.
