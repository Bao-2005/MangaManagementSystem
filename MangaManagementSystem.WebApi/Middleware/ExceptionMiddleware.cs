using Microsoft.AspNetCore.Mvc;
using MangaManagementSystem.Business.Exceptions;
using System.Net;
using System.Text.Json;

namespace MangaManagementSystem.API.Middleware
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
                _logger.LogError(ex, "An unhandled exception occurred.");
                context.Response.ContentType = "application/json";

                int statusCode = ex switch
                {
                    ArgumentException => (int)HttpStatusCode.BadRequest, // 400
                    ForbiddenAccessException => (int)HttpStatusCode.Forbidden, // 403
                    UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, // 401
                    KeyNotFoundException => (int)HttpStatusCode.NotFound, // 404
                    InvalidOperationException => (int)HttpStatusCode.Conflict, // 409
                    _ => (int)HttpStatusCode.InternalServerError // 500

                };
                context.Response.StatusCode = statusCode;
                var problem = new ProblemDetails
                {
                    Status = statusCode,
                    Title = "Lỗi hệ thống",
                    Detail = ex.Message,
                    Instance = context.Request.Path
                };
                var json = JsonSerializer.Serialize(problem);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
