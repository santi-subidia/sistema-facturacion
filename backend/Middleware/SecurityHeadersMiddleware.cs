namespace Backend.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Prevenir que el navegador adivine el Content-Type
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            
            // Prevenir que la app sea embebida en un iframe (clickjacking)
            context.Response.Headers["X-Frame-Options"] = "DENY";
            
            // Activar protección XSS del navegador
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            
            // Controlar qué información se envía en el Referer
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            
            // Content Security Policy - restringir orígenes de contenido
            context.Response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: blob:; " +
                "font-src 'self'; " +
                "connect-src 'self' https://wsaahomo.afip.gov.ar https://wsaa.afip.gov.ar https://wswhomo.afip.gov.ar https://servicios1.afip.gov.ar; " +
                "frame-ancestors 'none'";
            
            // Desactivar APIs del navegador que no se usan
            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            await _next(context);
        }
    }
}
