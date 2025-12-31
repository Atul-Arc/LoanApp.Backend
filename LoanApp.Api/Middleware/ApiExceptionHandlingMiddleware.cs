using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LoanApp.Api.Middleware;

/// <summary>
/// Centralized exception handling for the API.
/// Produces RFC7807 ProblemDetails responses and logs exceptions once.
/// Also handles request-abort cancellations gracefully.
/// </summary>
public sealed class ApiExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ApiExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ApiExceptionHandlingMiddleware> logger,
    IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected / proxy aborted the request.
            _logger.LogInformation(
            "Request aborted. {Method} {Path}{QueryString} TraceId={TraceId}",
            context.Request.Method,
            context.Request.Path.Value,
            context.Request.QueryString.Value,
            context.TraceIdentifier);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;

            _logger.LogError(
            ex,
            "Unhandled exception. {Method} {Path}{QueryString} TraceId={TraceId}",
            context.Request.Method,
            context.Request.Path.Value,
            context.Request.QueryString.Value,
            traceId);

            if (context.Response.HasStarted)
            {
                throw;
            }

            var (statusCode, title) = MapException(ex);

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = $"https://httpstatuses.com/{statusCode}",
                Instance = context.Request.Path,
            };

            // Safe, non-sensitive details.
            problem.Extensions["traceId"] = traceId;

            // Only include raw exception details in Development.
            if (_env.IsDevelopment())
            {
                problem.Detail = ex.Message;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
        }
    }

    private static (int StatusCode, string Title) MapException(Exception ex)
    {
        // Extend this mapping as you introduce domain/application-specific exceptions.
        return ex switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request."),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Operation could not be completed."),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
        };
    }
}
