using System.Linq.Dynamic.Core;

namespace OasisWords.Core.Persistence.Dynamic;

public class DynamicQuery
{
    public IEnumerable<Sort>? Sort { get; set; }
    public Filter? Filter { get; set; }
}

public class Sort
{
    public string Field { get; set; } = string.Empty;
    public string Dir { get; set; } = "asc"; // "asc" | "desc"
}

public class Filter
{
    public string Field { get; set; } = string.Empty;
    public string? Operator { get; set; }
    public string? Value { get; set; }
    public string? Logic { get; set; }
    public IEnumerable<Filter>? Filters { get; set; }
}

public static class DynamicQueryExtensions
{
    private static readonly IDictionary<string, string> Operators = new Dictionary<string, string>
    {
        { "eq", "=" },
        { "neq", "!=" },
        { "lt", "<" },
        { "lte", "<=" },
        { "gt", ">" },
        { "gte", ">=" },
        { "startswith", "StartsWith" },
        { "endswith", "EndsWith" },
        { "contains", "Contains" },
        { "doesnotcontain", "Contains" }
    };

    public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, DynamicQuery dynamicQuery)
    {
        if (dynamicQuery.Filter is not null)
            query = query.Filter(dynamicQuery.Filter);

        if (dynamicQuery.Sort is not null && dynamicQuery.Sort.Any())
            query = query.Sort(dynamicQuery.Sort);

        return query;
    }

    private static IQueryable<T> Filter<T>(this IQueryable<T> queryable, Filter filter)
    {
        List<object> values = new();
        string where = Transform(filter, values);

        if (!string.IsNullOrEmpty(where))
            queryable = queryable.Where(where, values.ToArray());

        return queryable;
    }

    private static IQueryable<T> Sort<T>(this IQueryable<T> queryable, IEnumerable<Sort> sort)
    {
        string ordering = string.Join(", ", sort.Select(s => $"{s.Field} {s.Dir}"));
        return string.IsNullOrEmpty(ordering) ? queryable : queryable.OrderBy(ordering);
    }

    private static string Transform(Filter filter, List<object> values)
    {
        int index = values.Count;
        string comparison = filter.Operator!;

        if (!Operators.TryGetValue(comparison.ToLower(), out string? op))
            throw new NotSupportedException($"Operator '{comparison}' is not supported.");

        string filterText;
        if (op is "StartsWith" or "EndsWith" or "Contains")
        {
            values.Add(filter.Value!);
            filterText = $"np({filter.Field}).{op}(@{index})";
        }
        else
        {
            values.Add(filter.Value!);
            filterText = $"np({filter.Field}) {op} @{index}";
        }

        if (filter.Filters is not null && filter.Filters.Any())
        {
            string logic = filter.Logic!.ToUpper();
            IEnumerable<string> childFilters = filter.Filters.Select(f => Transform(f, values));
            filterText = $"({filterText} {logic} {string.Join($" {logic} ", childFilters)})";
        }

        return filterText;
    }
}
