using System;
using System.Diagnostics.CodeAnalysis;

namespace Crip.AspNetCore.Correlation.Exceptions;

/// <summary>
/// HTTP request missing headers exception.
/// </summary>
[ExcludeFromCodeCoverage]
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