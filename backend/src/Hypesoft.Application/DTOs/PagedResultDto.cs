namespace Hypesoft.Application.DTOs;

public sealed class PagedResultDto<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public IEnumerable<T> Data { get; set; } = [];

    public static PagedResultDto<T> Create(IEnumerable<T> data, int totalRecords, int pageNumber, int pageSize)
    {
        return new PagedResultDto<T>
        {
            Data = data,
            TotalRecords = totalRecords,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
        };
    }
}
