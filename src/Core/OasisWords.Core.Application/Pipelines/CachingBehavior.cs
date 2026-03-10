using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace OasisWords.Core.Application.Pipelines;

public interface ICachableRequest
{
    string CacheKey { get; }
    bool BypassCache { get; }
    string? CacheGroupKey { get; }
    TimeSpan? SlidingExpiration { get; }
}

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICachableRequest
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly CacheSettings _cacheSettings;

    public CachingBehavior(
        IDistributedCache cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger,
        CacheSettings cacheSettings)
    {
        _cache = cache;
        _logger = logger;
        _cacheSettings = cacheSettings;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request.BypassCache)
            return await next();

        byte[]? cachedResponse = await _cache.GetAsync(request.CacheKey, cancellationToken);

        if (cachedResponse is not null)
        {
            _logger.LogInformation("Cache hit for key: {CacheKey}", request.CacheKey);
            return JsonSerializer.Deserialize<TResponse>(Encoding.UTF8.GetString(cachedResponse))!;
        }

        TResponse response = await next();

        DistributedCacheEntryOptions options = new()
        {
            SlidingExpiration = request.SlidingExpiration
                ?? TimeSpan.FromSeconds(_cacheSettings.SlidingExpiration)
        };

        byte[] serializedData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
        await _cache.SetAsync(request.CacheKey, serializedData, options, cancellationToken);

        _logger.LogInformation("Cache set for key: {CacheKey}", request.CacheKey);
        return response;
    }
}

public class CacheSettings
{
    public int SlidingExpiration { get; set; } = 300; // seconds
}
