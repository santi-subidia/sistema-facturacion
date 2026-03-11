using System.Net;
using System.Text.Json;

namespace Backend.Middleware
{
    public class JsonErrorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JsonErrorMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public JsonErrorMiddleware(RequestDelegate next, ILogger<JsonErrorMiddleware> logger, IWebHostEnvironment env)
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
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("No se puede manejar la excepción porque la respuesta ya comenzó.");
                return;
            }

            _logger.LogError(ex, "Error no controlado: {Message}", ex.Message);

            var statusCode = HttpStatusCode.InternalServerError;
            var message = "Ha ocurrido un error interno en el servidor";

            if (_env.IsDevelopment())
            {
                message = ex.Message;
            }

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var errorResponse = new { error = message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }

    public static class JsonErrorMiddlewareExtensions
    {
        public static IApplicationBuilder UseJsonErrorMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JsonErrorMiddleware>();
        }
    }
}