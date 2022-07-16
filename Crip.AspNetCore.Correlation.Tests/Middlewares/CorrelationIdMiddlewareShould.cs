﻿using Crip.AspNetCore.Correlation.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Crip.AspNetCore.Correlation.Tests.Middlewares;

public class CorrelationIdMiddlewareShould
{
    readonly RequestDelegate _delegate = _ => Task.CompletedTask;
    readonly Mock<IOptions<CorrelationIdOptions>> _options = new();
    readonly Mock<ICorrelationService> _correlation = new();

    [Fact, Trait("Category", "Unit")]
    public void Constructor_CanCreateInstance()
    {
        var act = () => new CorrelationIdMiddleware(_delegate, _options.Object, _correlation.Object);

        act.Should().NotThrow();
    }

    [Fact, Trait("Category", "Unit")]
    public void Constructor_FailsIfDelegateNotProvided()
    {
        var act = () => new CorrelationIdMiddleware(null!, _options.Object, _correlation.Object);

        act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'next')");
    }

    [Fact, Trait("Category", "Unit")]
    public void Constructor_FailsIfOptionsNotProvided()
    {
        var act = () => new CorrelationIdMiddleware(_delegate, null!, _correlation.Object);

        act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'options')");
    }

    [Fact, Trait("Category", "Unit")]
    public void Constructor_FailsIfCorrelationNotProvided()
    {
        var act = () => new CorrelationIdMiddleware(_delegate, _options.Object, null!);

        act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'correlation')");
    }

    [Fact, Trait("Category", "Unit")]
    public async Task Invoke_SetsCorrelationIdFromRequestHeaders()
    {
        var middleware = Middleware();
        MockOptions(new());
        const string headerKey = CorrelationIdOptions.CorrelationHeaderName;
        DefaultHttpContext httpContext = new() { Request = { Headers = { { headerKey, "header-id" } } } };

        await middleware.Invoke(httpContext);

        _correlation.Verify(correlation => correlation.Set(It.IsAny<HttpContext>(), "header-id"));
    }

    [Fact, Trait("Category", "Unit")]
    public async Task Invoke_SetsCorrelationIdFromRequestHeadersCaseInsensitive()
    {
        var middleware = Middleware();
        MockOptions(new());
        var headerKey = CorrelationIdOptions.CorrelationHeaderName.ToLower();
        DefaultHttpContext httpContext = new() { Request = { Headers = { { headerKey, "header-id" } } } };

        await middleware.Invoke(httpContext);

        _correlation.Verify(correlation => correlation.Set(It.IsAny<HttpContext>(), "header-id"));
    }

    [Fact, Trait("Category", "Unit")]
    public async Task Invoke_SetsRandomCorrelationId()
    {
        var middleware = Middleware();
        MockOptions(new());
        DefaultHttpContext httpContext = new();

        await middleware.Invoke(httpContext);

        _correlation.Verify(correlation => correlation.Set(
                It.IsAny<HttpContext>(),
                It.Is<string>(value => !string.IsNullOrWhiteSpace(value))),
            Times.Once);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task Invoke_WritesCorrelationHeaderToResponse()
    {
        var middleware = Middleware();
        MockOptions(new() { IncludeInResponse = true, Header = "key" });
        MockCorrelationGet("header-id");
        DefaultHttpContext httpContext = new();
        httpContext.Features.Set(HeadersFeature());

        await middleware.Invoke(httpContext);

        httpContext.Response.Headers.Should().Contain(new KeyValuePair<string, StringValues>("key", "header-id"));
    }

    private void MockCorrelationGet(string result) =>
        _correlation
            .Setup(correlation => correlation.Get(It.IsAny<HttpContext>()))
            .Returns(result);

    private CorrelationIdMiddleware Middleware() => new(_delegate, _options.Object, _correlation.Object);

    private static IHttpResponseFeature HeadersFeature()
    {
        var feature = new Mock<IHttpResponseFeature>();
        var headers = new HeaderDictionary();
        feature.Setup(option => option.Headers).Returns(headers);
        feature
            .Setup(option => option.OnStarting(It.IsAny<Func<object, Task>>(), It.IsAny<object>()))
            .Callback<Func<object, Task>, object>((callback, state) => callback(state).Wait());

        return feature.Object;
    }

    private void MockOptions(CorrelationIdOptions correlationIdOptions) =>
        _options.Setup(options => options.Value).Returns(correlationIdOptions);
}