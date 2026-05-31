using Microsoft.AspNetCore.Mvc;

namespace Eshop.API.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";

            var correlationId = context.Items["X-Correlation-Id"]?.ToString();

            var (status, title, detail) = exception switch
            {
                ArgumentException ex        => (StatusCodes.Status400BadRequest,      "Bad Request",             ex.Message),
                InvalidOperationException ex => (StatusCodes.Status400BadRequest,      "Invalid Operation",       ex.Message),
                KeyNotFoundException ex      => (StatusCodes.Status404NotFound,        "Resource Not Found",      ex.Message),
                UnauthorizedAccessException  => (StatusCodes.Status403Forbidden,       "Forbidden",               "You do not have permission to perform this action."),
                NotImplementedException      => (StatusCodes.Status501NotImplemented,  "Not Implemented",         "This feature is not yet implemented."),
                _                           => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", "An error occurred while processing your request.")
            };

            context.Response.StatusCode = status;

            var problem = new ProblemDetails
            {
                Type = $"https://httpstatuses.com/{status}",
                Title = title,
                Status = status,
                Detail = detail
            };

            if (correlationId != null)
                problem.Extensions["correlationId"] = correlationId;

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
