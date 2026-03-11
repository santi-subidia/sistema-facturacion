using Backend.Filters;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Filters
{
    public class AfipConfiguracionFilter : IAsyncActionFilter
    {
        private readonly IAfipConfiguracionService _afipConfiguracionService;

        public AfipConfiguracionFilter(IAfipConfiguracionService afipConfiguracionService)
        {
            _afipConfiguracionService = afipConfiguracionService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var path = context.HttpContext.Request.Path.Value?.ToLower();

            // Permitir endpoints de autenticación y configuración de AFIP
            if (path != null && (
                path.StartsWith("/api/auth") ||
                path.StartsWith("/api/afipconfiguracion") || // Standard controller route
                path.StartsWith("/api/afip/config") ||       // Custom route for check
                path.StartsWith("/api/afip/parametros")      // Custom route for params
            ))
            {
                await next();
                return;
            }

            // Verificar si el usuario está autenticado
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                await next();
                return;
            }

            // Verificar si existe configuración de AFIP
            var config = await _afipConfiguracionService.GetActivaAsync();
            if (config == null)
            {
                context.Result = new ObjectResult(new { error = "REQ_AFIP_CONFIG", message = "Se requiere configuración de AFIP para continuar." })
                {
                    StatusCode = 403
                };
                return;
            }

            await next();
        }
    }
}
