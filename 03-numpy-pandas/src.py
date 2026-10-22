# %% [markdown]
# # Урок 3. NumPy и Pandas
#
# Домашнее задание: работа с массивами NumPy, таблицами Pandas и мини-аналитика
# по успеваемости группы.

# %%
import numpy as np
import pandas as pd

pd.set_option("display.width", 120)
pd.set_option("display.max_columns", 20)

# %% [markdown]
# ## Часть 1. NumPy

# %%
scores = np.array([78, 95, 60, 88, 73, 100, 54, 69, 82, 91, 47, 76, 85, 90, 66])
print("Массив оценок:", scores)
print("Количество студентов:", scores.size)

# %%
print("Средняя оценка:", round(scores.mean(), 2))
print("Максимальная:", scores.max())
print("Минимальная:", scores.min())

# %%
print("Оценки выше 80:", scores[scores > 80])
print("Оценки ниже 60:", scores[scores < 60])

# %%
raised = scores * 1.1
print("После повышения на 10%:", raised.round(1))

limited = np.clip(raised, 0, 100)
print("С ограничением до 100:", limited.round(1))

# %%
passing = 70
passed = scores[scores >= passing]
print("Проходной балл:", passing)
print("Сдали:", passed.size, "из", scores.size)
print("Процент сдавших:", round(passed.size / scores.size * 100, 1), "%")

# %% [markdown]
# ## Часть 2. Pandas

# %%
students = pd.DataFrame({
    "name": ["Ali", "Amina", "Dias", "Dana", "Timur",
             "Aisha", "Nurlan", "Elena", "Serik", "Madina", "Arman", "Zarina"],
    "group": ["RA-521", "RA-521", "RA-521", "RA-522", "RA-522", "RA-522",
              "RA-521", "RA-522", "RA-521", "RA-522", "RA-521", "RA-522"],
    "age": [18, 19, 18, 20, 19, 18, 21, 19, 20, 18, 19, 20],
    "python_score": [78, 95, 60, 88, 73, 100, 54, 69, 82, 91, 47, 76],
    "math_score": [70, 88, 65, 92, 60, 95, 58, 74, 80, 85, 52, 81],
    "attendance": [92, 98, 64, 88, 71, 100, 55, 83, 90, 95, 48, 79],
})
students

# %%
students.head()

# %%
students.info()

# %%
students.describe()

# %%
print("Средний балл по Python:", round(students["python_score"].mean(), 2))
print("Средний балл по математике:", round(students["math_score"].mean(), 2))

# %%
students["average_score"] = ((students["python_score"] + students["math_score"]) / 2).round(2)
students["passed"] = students["average_score"] >= 70
students

# %%
print("Сдали:")
print(students[students["passed"]][["name", "group", "average_score"]])

# %%
print("Не сдали:")
print(students[~students["passed"]][["name", "group", "average_score"]])

# %%
print("Посещаемость ниже 70:")
print(students[students["attendance"] < 70][["name", "attendance", "average_score"]])

# %%
best = students.loc[students["average_score"].idxmax()]
worst_attendance = students.loc[students["attendance"].idxmin()]
print("Лучший студент:", best["name"], "со средним", best["average_score"])
print("Самая низкая посещаемость:", worst_attendance["name"], "-", worst_attendance["attendance"], "%")

# %%
by_group = students.groupby("group")[["python_score", "math_score", "average_score", "attendance"]].mean().round(2)
by_group

# %%
students.sort_values("average_score", ascending=False)[
    ["name", "group", "average_score", "attendance", "passed"]
]

# %% [markdown]
# ## Часть 3. Мини-аналитика

# %%
total = len(students)
passed_count = int(students["passed"].sum())
group_best = by_group["average_score"].idxmax()
risk = students[students["attendance"] < 70]["name"].tolist()

print(f"Всего студентов: {total}")
print(f"Средний балл по всей группе: {students['average_score'].mean().round(2)}")
print(f"Сдали: {passed_count} ({round(passed_count / total * 100, 1)}%)")
print(f"Лучший результат: {best['name']} — {best['average_score']}")
print(f"Лучшая группа: {group_best}")
print(f"В зоне риска по посещаемости: {risk}")

# %% [markdown]
# ### Вывод
#
# Средняя успеваемость группы держится около 75 баллов, то есть чуть выше
# проходного порога в 70. Сдали больше половины студентов, но запас небольшой:
# несколько человек находятся прямо на границе, и падение на 3–5 баллов переведёт
# их в число несдавших.
#
# Лучший результат показал студент с самым высоким средним баллом по двум
# предметам, и у него же одна из самых высоких посещаемостей. Это первый признак
# связи между присутствием на занятиях и итоговой оценкой.
#
# В зоне риска находятся студенты с посещаемостью ниже 70 процентов — у всех у них
# средний балл заметно ниже группового. Группа RA-521 и RA-522 отличаются по
# среднему результату, и разрыв объясняется в основном этими отстающими.
#
# Для будущей AI-модели полезными признаками выглядят посещаемость, баллы по
# отдельным предметам и возраст. Дополнительно пригодились бы количество сданных
# домашних заданий, время, потраченное на практику, и результаты промежуточных
# тестов — они позволили бы предсказывать итоговую оценку заранее, а не
# констатировать её по факту.
