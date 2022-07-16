using System.Collections.Generic;
using Crip.AspNetCore.Correlation.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Crip.AspNetCore.Correlation.Services;

/// <summary>
/// Request correlation service.
/// </summary>
public class CorrelationService : ICorrelationService
{
    private readonly IOptions<CorrelationIdOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationService"/> class.
    /// </summary>
    /// <param name="options">The correlation identifier options.</param>
    public CorrelationService(IOptions<CorrelationIdOptions> options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public string? Get(HttpContext httpContext) =>
        httpContext.Features.Get<ICorrelationFeature>()?.Id;

    /// <inheritdoc />
    public void Set(HttpContext httpContext, string id)
    {
        CorrelationFeature feature = new(id);
        httpContext.Features.Set<ICorrelationFeature>(feature);
    }

    /// <inheritdoc />
    public Dictionary<string, object?> Scope(HttpContext httpContext) =>
        new() { { _options.Value.PropertyName, Get(httpContext) } };
}