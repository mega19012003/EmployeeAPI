using System.Text.Json;
using EmployeeAPI.Base;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        public static Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger<ExceptionMiddleware> logger)
        {
            int statusCode;
            string message;

            switch (exception)
            {
                case ArgumentException:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = "Invalid Input";
                    break;

                case UnauthorizedAccessException:
                    statusCode = StatusCodes.Status403Forbidden;
                    message = "Access Denied";
                    break;

                case KeyNotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    message = "Data not found";
                    break;

                case DbUpdateException:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = "Database error";
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "Unexpected error";
                    break;
            }

            logger.LogError(exception, "Exception caught in middleware");

            var result = JsonSerializer.Serialize(ApiResponse<string>.ReturnResult(message, exception.Message, statusCode));
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsync(result);
        }
    }
}
