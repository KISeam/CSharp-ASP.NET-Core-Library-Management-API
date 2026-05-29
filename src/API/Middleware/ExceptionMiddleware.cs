using System.Text.Json;
using LibraryAPI.Domain.Common;

namespace LibraryAPI.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(
                "Validation failed: {Errors}",
                string.Join(
                    "; ",
                    ex.Errors.SelectMany(e => e.Value)));

            await WriteValidationErrorAsync(
                ctx,
                ex.Errors);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(
                ex,
                "Domain rule violation: {Message}",
                ex.Message);

            await WriteErrorAsync(
                ctx,
                ex.StatusCode,
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception on {Path}",
                ctx.Request.Path);

            await WriteErrorAsync(
                ctx,
                500,
                "An unexpected error occurred. Please try again later.");
        }
    }

    private static Task WriteErrorAsync(
        HttpContext ctx,
        int statusCode,
        string message)
    {
        ctx.Response.ContentType =
            "application/json";

        ctx.Response.StatusCode =
            statusCode;

        var response = JsonSerializer.Serialize(
            new
            {
                success = false,
                statusCode,
                message,
                timestamp = DateTime.UtcNow
            },
            new JsonSerializerOptions
            {
                PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase
            });

        return ctx.Response.WriteAsync(response);
    }

    private static Task WriteValidationErrorAsync(
        HttpContext ctx,
        IDictionary<string, string[]> errors)
    {
        ctx.Response.ContentType =
            "application/json";

        ctx.Response.StatusCode =
            422;

        var response = JsonSerializer.Serialize(
            new
            {
                success = false,
                statusCode = 422,
                message = "Validation failed.",
                errors,
                timestamp = DateTime.UtcNow
            },
            new JsonSerializerOptions
            {
                PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase
            });

        return ctx.Response.WriteAsync(response);
    }
}
