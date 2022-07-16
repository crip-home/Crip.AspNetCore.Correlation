namespace Crip.AspNetCore.Correlation.Services;

/// <summary>
/// Correlation identifier accessor contract.
/// </summary>
public interface ICorrelationAccessor
{
    /// <summary>
    /// Get correlation identifier from https context accessor.
    /// </summary>
    /// <returns>Correlation identifier if available in HTTP context.</returns>
    string? Get();
}