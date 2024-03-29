﻿#pragma warning disable CS1574

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Crip.AspNetCore.Correlation.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Crip.AspNetCore.Correlation;

/// <summary>
/// HttpClient handler to add X-Correlation-Id header value to outgoing HTTP
/// requests. Component requires that DI has registration for HttpContext
/// accessor <see cref="HttpServiceCollectionExtensions.AddHttpContextAccessor(IServiceCollection)"/>.
/// </summary>
public class CorrelationIdHeaderHandler : DelegatingHandler
{
    private const StringComparison CompareIgnoreCase = StringComparison.InvariantCultureIgnoreCase;

    private readonly IOptions<CorrelationIdOptions> _options;
    private readonly IHttpCorrelationAccessor _correlation;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationIdHeaderHandler"/> class.
    /// </summary>
    /// <param name="options">The correlation identifier options.</param>
    /// <param name="correlation">The HTTP context correlation identifier accessor.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="options"/> or <paramref name="correlation"/> is not provided.
    /// </exception>
    public CorrelationIdHeaderHandler(
        IOptions<CorrelationIdOptions> options,
        IHttpCorrelationAccessor correlation)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    /// <summary>
    /// Create new unique identifier.
    /// </summary>
    /// <returns>New and unique correlation identifier.</returns>
    protected virtual string CreateNewIdentifier() => Guid.NewGuid().ToString();

    /// <summary>
    /// Get existing correlation identifier from current HTTP context accessor.
    /// </summary>
    /// <returns>Correlation identifier value or <c>null</c>, if not found.</returns>
    protected virtual string? GetExistingIdentifier() => _correlation.Get();

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        if (MissingValueIn(request.Headers))
        {
            var correlationId = GetExistingIdentifier() ?? CreateNewIdentifier();
            request.Headers.Add(_options.Value.Header, correlationId);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private bool MissingValueIn(HttpRequestHeaders request) =>
        request.Any(IsCorrelationHeader) is false;

    private bool IsCorrelationHeader(KeyValuePair<string, IEnumerable<string>> header) =>
        string.Equals(header.Key, _options.Value.Header, CompareIgnoreCase);
}