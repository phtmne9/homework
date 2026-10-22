using Microsoft.AspNetCore.Mvc;
using StudentApi.Data;
using StudentApi.Models;

namespace StudentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly IStudentRepository _repository;

    public StudentController(IStudentRepository repository) => _repository = repository;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _repository.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Student student)
    {
        if (student == null)
            return BadRequest("Данные студента не переданы.");

        await _repository.AddAsync(student);
        return Ok(new { message = "Студент успешно добавлен", student });
    }
}
