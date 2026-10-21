namespace ContactApi.Entities;

public class Contact
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string AltPhone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Note { get; set; } = string.Empty;
}
