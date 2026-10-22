using PvzApi.Entities;

namespace PvzApi.Models;

public class PickupPointDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public int InStock { get; set; }
}

public class ClientDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
}

public class ParcelDto
{
    public int Id { get; set; }

    public string TrackNumber { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    public decimal Price { get; set; }

    public ParcelStatus Status { get; set; }

    public DateTime ArrivedAt { get; set; }

    public DateTime? IssuedAt { get; set; }

    public DateTime Deadline { get; set; }

    public string PickupPointName { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public string ClientPhone { get; set; } = string.Empty;
}

public class ParcelCreateDto
{
    public string TrackNumber { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    public decimal Price { get; set; }

    public int StorageDays { get; set; } = 7;

    public int PickupPointId { get; set; }

    public int ClientId { get; set; }
}

public class PageResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int Total { get; set; }

    public int Pages => PageSize == 0 ? 0 : (int)System.Math.Ceiling((double)Total / PageSize);
}

public class ParcelQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public ParcelStatus? Status { get; set; }

    public int? PickupPointId { get; set; }

    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public bool Desc { get; set; }
}

public class LoginRequest
{
    public string Login { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public string Role { get; set; } = string.Empty;
}

public class IssueRequest
{
    public string Phone { get; set; } = string.Empty;
}
