using CityEfApi.Models;

namespace CityEfApi.Services;

public interface ICityService
{
    IEnumerable<CityDTO> CityGetAll();

    CityDTO? CityGetById(int id);

    string CityAdd(CityDTO city);

    string CityEdit(CityDTO city, int id);

    string CityDelete(int id);
}
