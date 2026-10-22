using StudentApi.Models;

namespace StudentApi.Data;

public interface IStudentRepository
{
    Task AddAsync(Student student);

    Task<IEnumerable<Student>> GetAllAsync();
}
