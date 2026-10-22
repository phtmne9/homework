from review_analyzer import ReviewAnalyzer
from student_analyzer import StudentAnalyzer

reviews = [
    "Отличный курс, всё понятно и интересно",
    "Ужасно, ничего не работает, зря потратил время",
    "Доставка быстро, качество хорошее, рекомендую",
    "Сайт медленно грузится и интерфейс неудобный",
    "Нормально, ничего особенного",
    "Супер, прекрасно объясняют, доволен",
    "Товар сломалось сразу, отвратительно",
    "Удобно и качественно, классно сделано",
    "Долго ждал, дорого, непонятно за что",
    "Просто текст без эмоций",
]

students = [
    {"name": "Ali", "scores": [10, 12, 9]},
    {"name": "Amina", "scores": [8, 7, 10]},
    {"name": "Dias", "scores": [12, 12, 11]},
    {"name": "Dana", "scores": [5, 7, 6]},
]


def main():
    analyzer = ReviewAnalyzer()
    analyzer.add_positive_word("шикарно")
    analyzer.add_negative_word("жесть")

    analyzed = analyzer.analyze_reviews(reviews)
    analyzer.print_report(analyzed)

    print("\nПроверка пустого списка:")
    analyzer.print_report(analyzer.analyze_reviews([]))

    print("\nПроверка новых слов:")
    for text in ["Шикарно сделано", "Полная жесть", "Обычный отзыв"]:
        result = analyzer.analyze_text(text)
        print(f"  {result['sentiment']:<9} {text}")

    print("\n" + "=" * 60)
    print("АНАЛИЗ СТУДЕНТОВ")
    print("=" * 60)

    sa = StudentAnalyzer(students)
    for student in sa.analyze_students():
        print(f"  {student['name']:<6} average={student['average']:<6} {student['status']}")

    best = sa.get_best_student()
    print(f"\nЛучший студент:   {best['name']} ({best['average']})")
    print(f"Отстающие:        {[s['name'] for s in sa.get_bad_students()]}")
    print(f"Средний по группе: {sa.get_group_average()}")


if __name__ == "__main__":
    main()
