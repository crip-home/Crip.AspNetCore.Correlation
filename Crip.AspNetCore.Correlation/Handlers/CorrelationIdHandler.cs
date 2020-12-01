using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Crip.AspNetCore.Correlation
{
    /// <summary>
    /// HttpClient handler to add CorrelationId header value to outgoing HTTP
    /// requests. Component requires that DI has registration for HttpContext
    /// accessor <see cref="HttpServiceCollectionExtensions.AddHttpContextAccessor(IServiceCollection)"/>.
    /// </summary>
    public class CorrelationIdHandler : DelegatingHandler
    {
        private readonly CorrelationIdOptions _options;
        private readonly IHttpContextAccessor _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationIdHandler"/> class.
        /// </summary>
        /// <param name="options">The correlation identifier options.</param>
        /// <param name="context">The HTTP context accessor.</param>
        public CorrelationIdHandler(
            IOptions<CorrelationIdOptions> options,
            IHttpContextAccessor context)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Create new unique identifier.
        /// </summary>
        /// <returns>New and unique correlation identifier.</returns>
        protected virtual string CreateNewIdentifier()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Get existing correlation identifier from current HTTP context accessor.
        /// </summary>
        /// <returns>Correlation identifier value or <c>null</c>, if not found.</returns>
        protected virtual string? GetExistingIdentifier()
        {
            return _context?.HttpContext?.TraceIdentifier;
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (MissingValueIn(request.Headers))
            {
                string correlationId = GetExistingIdentifier() ?? CreateNewIdentifier();
                request.Headers.Add(_options.Header, correlationId);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private bool MissingValueIn(HttpRequestHeaders request)
        {
            return request.All(x => x.Key != _options.Header);
        }
    }
}