using StudentApi.Models;

namespace StudentApi.Data;

public class StudentRepository : IStudentRepository
{
    private readonly List<Student> _students = new();

    public Task AddAsync(Student student)
    {
        student.Id = _students.Count + 1;
        _students.Add(student);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Student>> GetAllAsync() =>
        Task.FromResult<IEnumerable<Student>>(_students);
}
