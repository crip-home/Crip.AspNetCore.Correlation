using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crip.AspNetCore.Correlation
{
    /// <summary>
    /// Request correlation identifier logging middleware.
    /// </summary>
    public class CorrelationIdLoggingMiddleware
    {
        private readonly CorrelationIdOptions _options;
        private readonly ILogger<CorrelationIdLoggingMiddleware> _logger;
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationIdLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="options">The correlation identifier options.</param>
        /// <param name="logger">The logging service.</param>
        /// <param name="next">The request delegate.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="logger"/> or <paramref name="next"/> is not provided.
        /// </exception>
        public CorrelationIdLoggingMiddleware(
            IOptions<CorrelationIdOptions> options,
            ILogger<CorrelationIdLoggingMiddleware> logger,
            RequestDelegate next)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        /// <summary>
        /// Invokes action for the specified context.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Next middleware output.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="context"/> not provided.
        /// </exception>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var scope = CreateScope(context);
            using (_logger.BeginScope(scope))
            {
                await _next(context);
            }
        }

        private Dictionary<string, object> CreateScope(HttpContext context)
        {
            return new() { { _options.PropertyName, context.TraceIdentifier } };
        }
    }
}