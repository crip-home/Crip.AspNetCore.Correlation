using System;

namespace Crip.AspNetCore.Correlation.Exceptions;

/// <summary>
/// HTTP request missing headers exception.
/// </summary>
public class RequestHeadersMissingException : ApplicationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequestHeadersMissingException"/> class.
    /// </summary>
    public RequestHeadersMissingException()
        : base("Request headers missing")
    {
    }
}