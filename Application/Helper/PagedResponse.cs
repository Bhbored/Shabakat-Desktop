namespace Shabakat.Application.Helper;

public sealed record PagedResponse<T>(
    IEnumerable<T> Data,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage)
{
    public static PagedResponse<T> Create(
        IEnumerable<T> data,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        pageSize = Math.Max(1, pageSize);
        pageNumber = Math.Max(1, pageNumber);
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResponse<T>(
            Data: data,
            TotalCount: totalCount,
            PageNumber: pageNumber,
            PageSize: pageSize,
            TotalPages: totalPages,
            HasPreviousPage: pageNumber > 1,
            HasNextPage: pageNumber < totalPages);
    }

    public static PagedResponse<T> Create(
        (IEnumerable<T> Items, int TotalCount) page,
        int pageNumber,
        int pageSize)
        => Create(page.Items, page.TotalCount, pageNumber, pageSize);
}
