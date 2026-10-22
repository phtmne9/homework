class StudentAnalyzer:
    def __init__(self, students):
        self.students = students

    def analyze_students(self):
        for student in self.students:
            scores = student.get("scores", [])
            average = round(sum(scores) / len(scores), 2) if scores else 0
            student["average"] = average
            student["status"] = self._status(average)

        return self.students

    def get_best_student(self):
        if not self.students:
            return None

        return max(self.students, key=lambda s: s.get("average", 0))

    def get_bad_students(self):
        return [s for s in self.students if s.get("status") == "bad"]

    def get_group_average(self):
        if not self.students:
            return 0

        averages = [s.get("average", 0) for s in self.students]

        return round(sum(averages) / len(averages), 2)

    @staticmethod
    def _status(average):
        if average >= 10:
            return "good"
        if average >= 7:
            return "normal"
        return "bad"
