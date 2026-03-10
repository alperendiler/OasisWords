using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace OasisWords.Core.Application.Pipelines;

public interface ICacheRemoverRequest
{
    string[] CacheGroupKeys { get; }
    bool BypassCache { get; }
}

public class CacheRemovingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheRemoverRequest
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheRemovingBehavior<TRequest, TResponse>> _logger;

    public CacheRemovingBehavior(
        IDistributedCache cache,
        ILogger<CacheRemovingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        TResponse response = await next();

        if (!request.BypassCache)
        {
            foreach (string groupKey in request.CacheGroupKeys)
            {
                await _cache.RemoveAsync(groupKey, cancellationToken);
                _logger.LogInformation("Cache removed for group key: {GroupKey}", groupKey);
            }
        }

        return response;
    }
}
