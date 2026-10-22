using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace AutoApi.Dtos;

[SwaggerSchema("Данные автомобиля для создания или обновления")]
public class AutoInput
{
    [Required]
    [StringLength(100)]
    [SwaggerSchema("Модель")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [SwaggerSchema("Производитель")]
    public string Producer { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [SwaggerSchema("Цвет")]
    public string Color { get; set; } = string.Empty;

    [Range(0, 100_000_000)]
    [SwaggerSchema("Стоимость в рублях")]
    public decimal Cost { get; set; }
}
