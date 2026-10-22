using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentApi.Controllers;
using StudentApi.Data;
using StudentApi.Models;
using Xunit;

namespace StudentApi.Tests;

public class StudentControllerTests
{
    [Fact]
    public async Task Add_SavesStudent_AndReturnsOk()
    {
        var repo = new Mock<IStudentRepository>();
        var controller = new StudentController(repo.Object);
        var student = new Student { Name = "Арсений", Course = 2 };

        var result = await controller.Add(student);

        Assert.IsType<OkObjectResult>(result);
        repo.Verify(r => r.AddAsync(student), Times.Once);
    }

    [Fact]
    public async Task Add_ReturnsBadRequest_WhenStudentIsNull()
    {
        var repo = new Mock<IStudentRepository>();
        var controller = new StudentController(repo.Object);

        var result = await controller.Add(null!);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Данные студента не переданы.", bad.Value);
        repo.Verify(r => r.AddAsync(It.IsAny<Student>()), Times.Never);
    }
}
