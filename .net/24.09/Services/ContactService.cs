using ClosedXML.Excel;
using ContactApi.Data;
using ContactApi.Entities;

namespace ContactApi.Services;

public class ContactService : IContactService
{
    private static readonly string[] Headers =
        ["Имя", "Телефон", "Альт. телефон", "Email", "Описание"];

    private readonly IContactRepo _repo;

    public ContactService(IContactRepo repo) => _repo = repo;

    public Task<IReadOnlyList<Contact>> GetAllAsync() => _repo.GetAllAsync();

    public async Task<int> UploadAsync(Stream file, CancellationToken ct)
    {
        var contacts = new List<Contact>();

        using var reader = new StreamReader(file);

        while (await reader.ReadLineAsync(ct) is { } line)
        {
            var p = line.Split(';', StringSplitOptions.TrimEntries);

            if (p.Length < 2 || p[0].Length == 0 || p[1].Length == 0)
                continue;

            contacts.Add(new Contact
            {
                Name = p[0],
                Phone = p[1],
                AltPhone = At(p, 2),
                Email = At(p, 3),
                Note = At(p, 4),
            });
        }

        if (contacts.Count == 0)
            return 0;

        return await _repo.AddAsync(contacts);
    }

    public async Task<byte[]> ExportAsync()
    {
        var contacts = await _repo.GetAllAsync();

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Contacts");

        for (var i = 0; i < Headers.Length; i++)
            ws.Cell(1, i + 1).Value = Headers[i];

        ws.Row(1).Style.Font.Bold = true;

        for (var i = 0; i < contacts.Count; i++)
        {
            var c = contacts[i];
            var row = i + 2;

            ws.Cell(row, 1).Value = c.Name;
            ws.Cell(row, 2).Value = c.Phone;
            ws.Cell(row, 3).Value = c.AltPhone;
            ws.Cell(row, 4).Value = c.Email;
            ws.Cell(row, 5).Value = c.Note;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);

        return ms.ToArray();
    }

    private static string At(string[] p, int i) => i < p.Length ? p[i] : string.Empty;
}
