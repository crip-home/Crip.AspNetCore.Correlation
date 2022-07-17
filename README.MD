# CRIP AspNetCore Correlation

The correlation ID is meant to be used to help Admin trace what was happening at the time of an error/request. It
is only valuable as an interactive tool to help the Admin track down your error. An Admin uses the correlation ID as
"breadcrumbs" to retrace a requests or processes in the Logging System to find what leads up to
and causes a problem.

## Installation

TODO: create NuGet package to be able install

## Setup

Configure services:

```csharp
services.AddHttpContextAccessor(); // package requires that HTTP context accessor is available
services.AddCorrelation();
```

Configure pipeline:

```csharp
// Add middlewares as eraly as possible
app.UseCorrelation();
// or
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<CorrelationIdLoggingMiddleware>();
```