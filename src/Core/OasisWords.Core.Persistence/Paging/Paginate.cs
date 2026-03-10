using Microsoft.EntityFrameworkCore;

namespace OasisWords.Core.Persistence.Paging;

public interface IPaginate<T>
{
    int From { get; }
    int Index { get; }
    int Size { get; }
    int Count { get; }
    int Pages { get; }
    IList<T> Items { get; }
    bool HasPrevious { get; }
    bool HasNext { get; }
}

public class Paginate<T> : IPaginate<T>
{
    public int From { get; init; }
    public int Index { get; init; }
    public int Size { get; init; }
    public int Count { get; init; }
    public int Pages { get; init; }
    public IList<T> Items { get; init; } = new List<T>();
    public bool HasPrevious => Index - From > 0;
    public bool HasNext => Index - From + 1 < Pages;
}

public static class PaginateExtensions
{
    public static IPaginate<T> ToPaginate<T>(this IQueryable<T> source, int index, int size, int from = 0)
    {
        int count = source.Count();
        List<T> items = source.Skip((index - from) * size).Take(size).ToList();

        return new Paginate<T>
        {
            Index = index,
            Size = size,
            From = from,
            Count = count,
            Pages = (int)Math.Ceiling(count / (double)size),
            Items = items
        };
    }

    public static async Task<IPaginate<T>> ToPaginateAsync<T>(
        this IQueryable<T> source,
        int index, int size, int from = 0,
        CancellationToken cancellationToken = default)
    {
        int count = await source.CountAsync(cancellationToken);
        List<T> items = await source
            .Skip((index - from) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new Paginate<T>
        {
            Index = index,
            Size = size,
            From = from,
            Count = count,
            Pages = (int)Math.Ceiling(count / (double)size),
            Items = items
        };
    }
}

public class BasePageableModel
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}
