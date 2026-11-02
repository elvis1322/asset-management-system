namespace Application.Models;
public class PaginationParams
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;
    private int _pageNumber = 1;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : (value < 1 ? 1 : value);
    }
}
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult() { }

    public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
public static class PagedResultExtensions
{
    public static PagedResult<TResponse> CreatePagedResult<TEntity, TResponse>(
        List<TResponse> itemDtos, 
        int totalCount, 
        PaginationParams paginationParams)
    {
        return new PagedResult<TResponse>(itemDtos, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }
}