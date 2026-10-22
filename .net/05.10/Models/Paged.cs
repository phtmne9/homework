namespace PagingApi.Models;

public record Paged<T>(IReadOnlyList<T> Items, int Total, int Page, int Size)
{
    public int Pages => Size > 0 ? (Total + Size - 1) / Size : 0;
}
