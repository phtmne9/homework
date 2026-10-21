using ContactApi.Entities;

namespace ContactApi.Data;

public interface IContactRepo
{
    Task InitAsync();

    Task<IReadOnlyList<Contact>> GetAllAsync();

    Task<int> AddAsync(IEnumerable<Contact> contacts);
}
