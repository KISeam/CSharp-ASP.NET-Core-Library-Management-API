namespace LibraryAPI.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        var stopwatch =
            System.Diagnostics.Stopwatch.StartNew();

        await _next(ctx);

        stopwatch.Stop();

        _logger.LogInformation(
            "{Method} {Path} → {StatusCode} ({Elapsed}ms)",
            ctx.Request.Method,
            ctx.Request.Path,
            ctx.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
