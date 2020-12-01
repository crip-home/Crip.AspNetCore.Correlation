using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Display;
using Serilog.Sinks.TestCorrelator;
using Xunit;

namespace Crip.AspNetCore.Correlation.Tests.Middlewares
{
    public class CorrelationIdLoggingMiddlewareShould
    {
        [Fact, Trait("Category", "Unit")]
        public void CorrelationIdLoggingMiddleware_Construct_CanCreateInstance()
        {
            // Arrange
            IOptions<CorrelationIdOptions> options = Options.Create<CorrelationIdOptions>(new());
            Mock<ILogger<CorrelationIdLoggingMiddleware>> loggerMock = new();
            Task RequestDelegate(HttpContext ctx) => Task.CompletedTask;

            // Act
            Action act = () => new CorrelationIdLoggingMiddleware(options, loggerMock.Object, RequestDelegate);

            // Assert
            act.Should().NotThrow();
        }

        [Fact, Trait("Category", "Unit")]
        public async Task CorrelationIdLoggingMiddleware_Invoke_LogsDelegateActionWithRequiredScopeProperty()
        {
            // Arrange
            var logger = CreateLoggerFactory().CreateLogger<CorrelationIdLoggingMiddleware>();
            IOptions<CorrelationIdOptions> options = Options.Create<CorrelationIdOptions>(new()
            {
                PropertyName = "P1",
            });

            Task RequestDelegate(HttpContext ctx)
            {
                logger.LogDebug("context log message");
                return Task.CompletedTask;
            }

            CorrelationIdLoggingMiddleware middleware = new(options, logger, RequestDelegate);
            HttpContext context = new DefaultHttpContext { TraceIdentifier = "T1" };

            // Act
            using var _ = TestCorrelator.CreateContext();
            logger.LogInformation("Before");
            await middleware.Invoke(context);
            logger.LogInformation("After");

            // Assert
            List<string> logs = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
            var sourceContext = "SourceContext: \"Crip.AspNetCore.Correlation.CorrelationIdLoggingMiddleware\"";
            logs.Should().BeEquivalentTo(
                $"Information: Before {{ {sourceContext} }}",
                $"Debug: context log message {{ {sourceContext}, P1: \"T1\" }}",
                $"Information: After {{ {sourceContext} }}");
        }

        [Fact, Trait("Category", "Unit")]
        public async Task CorrelationIdLoggingMiddleware_Invoke_FailsIfContextNotProvided()
        {
            // Arrange
            IOptions<CorrelationIdOptions> options = Options.Create<CorrelationIdOptions>(new());
            Mock<ILogger<CorrelationIdLoggingMiddleware>> loggerMock = new();
            Task RequestDelegate(HttpContext ctx) => Task.CompletedTask;
            CorrelationIdLoggingMiddleware middleware = new(options, loggerMock.Object, RequestDelegate);

            // Act
            Func<Task> act = async () => await middleware.Invoke(null!);

            // Assert
            await act.Should()
                .ThrowExactlyAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'context')");
        }

        private static ILoggerFactory CreateLoggerFactory()
        {
            var logger = CreateCorrelatorLogger();

            return new SerilogLoggerFactory(logger);
        }

        private static Logger CreateCorrelatorLogger()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.TestCorrelator()
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        private string FormatLogEvent(LogEvent logEvent)
        {
            const string template = "{Level}: {Message:lj} {Properties}";

            var culture = CultureInfo.InvariantCulture;
            MessageTemplateTextFormatter formatter = new(template, culture);
            StringWriter writer = new();
            formatter.Format(logEvent, writer);

            return writer.ToString();
        }
    }
}