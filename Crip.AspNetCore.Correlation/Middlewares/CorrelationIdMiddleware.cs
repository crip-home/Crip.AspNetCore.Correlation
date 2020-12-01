using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Crip.AspNetCore.Correlation
{
    /// <summary>
    /// Request correlation identifier middleware. Sets context identifier from
    /// request headers or generates new value for it.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CorrelationIdOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="CorrelationIdMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware delegate.</param>
        /// <param name="options">The correlation identifier options.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="next"/> or <paramref name="options"/> is not provided.
        /// </exception>
        public CorrelationIdMiddleware(
            RequestDelegate next,
            IOptions<CorrelationIdOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Invokes middleware with the specified context.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Next middleware output.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="context"/> is not provided.
        /// </exception>
        public Task Invoke(HttpContext context)
        {
            if (context.Request?.Headers == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.TraceIdentifier = GetIdentifier(context) ?? CreateIdentifier();

            if (_options.IncludeInResponse)
            {
                // Apply the correlation identifier to the response header for client side tracking.
                context.Response.OnStarting(AddToResponseHeaders(context));
            }

            return _next(context);
        }

        /// <summary>
        /// Create new unique identifier.
        /// </summary>
        /// <returns>New and unique correlation identifier.</returns>
        protected virtual string CreateIdentifier()
        {
            return Guid.NewGuid().ToString();
        }

        private Func<Task> AddToResponseHeaders(HttpContext context)
        {
            return () =>
            {
                context.Response.Headers.Add(_options.Header, context.TraceIdentifier);
                return Task.CompletedTask;
            };
        }

        private string? GetIdentifier(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(_options.Header, out var correlationId))
            {
                return correlationId;
            }

            return null;
        }
    }
}