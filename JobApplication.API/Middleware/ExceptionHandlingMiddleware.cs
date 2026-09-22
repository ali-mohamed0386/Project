using JobApplication.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.API.Middleware
{
    /// <summary>
    /// Translates exceptions into appropriate HTTP status codes so that
    /// business rules stay in the Application layer and are not repeated in
    /// every controller.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (Exception exception)
            {
                // If the response has already started, the status code can no
                // longer be changed and nothing more can be written — log and
                // return.
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning(exception,
                        "The response has already started — the error payload could not be written.");
                    return;
                }

                await WriteErrorResponseAsync(context, exception);
            }
        }

        private async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title) = MapException(exception);

            // Only 500s are logged. The 400/403/404 responses are expected
            // outcomes, not server faults, so they do not need log noise.
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception,
                    "Unhandled error while processing {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
            }

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                // SECURITY: internal exception details are hidden for 500s so
                // that infrastructure information is not leaked.
                Detail = statusCode == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred on the server."
                    : exception.Message,
                Instance = context.Request.Path,
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problem);
        }

        private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
            BadRequestException => (StatusCodes.Status400BadRequest, "Bad Request"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
        };
    }
}
