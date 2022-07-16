using Microsoft.AspNetCore.Http;

namespace Crip.AspNetCore.Correlation.Services;

/// <summary>
/// Correlation identifier accessor.
/// </summary>
public class CorrelationAccessor : ICorrelationAccessor
{
    private readonly ICorrelationService _correlation;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationAccessor"/> class.
    /// </summary>
    /// <param name="correlation">The correlation identifier service.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public CorrelationAccessor(
        ICorrelationService correlation,
        IHttpContextAccessor httpContextAccessor)
    {
        _correlation = correlation;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public string? Get() =>
        _correlation.Get(_httpContextAccessor.HttpContext);
}