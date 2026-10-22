class ReviewAnalyzer:
    def __init__(self):
        self.positive_words = [
            "хорошо", "отлично", "супер", "понравилось", "классно", "быстро",
            "удобно", "качественно", "рекомендую", "доволен", "прекрасно", "интересно",
        ]
        self.negative_words = [
            "плохо", "ужасно", "кошмар", "медленно", "неудобно", "сломалось",
            "брак", "разочарован", "отвратительно", "долго", "дорого", "непонятно",
        ]

    def add_positive_word(self, word):
        word = word.strip().lower()
        if word and word not in self.positive_words:
            self.positive_words.append(word)

    def add_negative_word(self, word):
        word = word.strip().lower()
        if word and word not in self.negative_words:
            self.negative_words.append(word)

    def analyze_text(self, text):
        if not text or not text.strip():
            return {"text": text, "score": 0, "sentiment": "neutral"}

        words = text.lower().replace(",", " ").replace(".", " ").split()
        score = 0

        for word in words:
            if word in self.positive_words:
                score += 1
            elif word in self.negative_words:
                score -= 1

        if score > 0:
            sentiment = "positive"
        elif score < 0:
            sentiment = "negative"
        else:
            sentiment = "neutral"

        return {"text": text, "score": score, "sentiment": sentiment}

    def analyze_reviews(self, reviews):
        if not reviews:
            return []

        return [self.analyze_text(review) for review in reviews]

    def get_stats(self, analyzed_reviews):
        if not analyzed_reviews:
            return {"total": 0, "positive": 0, "negative": 0, "neutral": 0, "average": 0}

        stats = {
            "total": len(analyzed_reviews),
            "positive": 0,
            "negative": 0,
            "neutral": 0,
            "average": 0,
        }

        for review in analyzed_reviews:
            stats[review["sentiment"]] += 1

        total_score = sum(review["score"] for review in analyzed_reviews)
        stats["average"] = round(total_score / len(analyzed_reviews), 2)

        return stats

    def print_report(self, analyzed_reviews):
        if not analyzed_reviews:
            print("Отзывов нет, отчёт пустой")
            return

        print("=" * 60)
        print("ОТЧЁТ ПО ОТЗЫВАМ")
        print("=" * 60)

        for i, review in enumerate(analyzed_reviews, 1):
            mark = {"positive": "+", "negative": "-", "neutral": "="}[review["sentiment"]]
            print(f"{i:>2}. [{mark}] {review['sentiment']:<9} score={review['score']:>3}  {review['text']}")

        stats = self.get_stats(analyzed_reviews)
        print("-" * 60)
        print(f"Всего отзывов:   {stats['total']}")
        print(f"Положительных:   {stats['positive']}")
        print(f"Отрицательных:   {stats['negative']}")
        print(f"Нейтральных:     {stats['neutral']}")
        print(f"Средний балл:    {stats['average']}")
        print("=" * 60)
