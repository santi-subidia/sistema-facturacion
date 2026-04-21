using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Backend.Filters
{
    /// <summary>
    /// Filtro de acción asíncrono que implementa idempotencia basada en IMemoryCache.
    /// Intercepta peticiones que contengan el header "X-Idempotency-Key" en endpoints
    /// decorados con [Idempotent]. Si la clave ya existe en caché, devuelve la respuesta
    /// guardada sin re-ejecutar la acción. Si no existe, ejecuta la acción y cachea
    /// la respuesta exitosa (2xx) para futuras peticiones con la misma clave.
    /// </summary>
    public class IdempotencyFilter : IAsyncActionFilter
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<IdempotencyFilter> _logger;

        private const string IdempotencyHeader = "X-Idempotency-Key";

        public IdempotencyFilter(IMemoryCache cache, ILogger<IdempotencyFilter> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. Verificar si el endpoint tiene el atributo [Idempotent]
            var idempotentAttribute = context.ActionDescriptor.EndpointMetadata
                .OfType<IdempotentAttribute>()
                .FirstOrDefault();

            if (idempotentAttribute is null)
            {
                // No es un endpoint idempotente, continuar normalmente
                await next();
                return;
            }

            // 2. Obtener la clave de idempotencia del header
            if (!context.HttpContext.Request.Headers.TryGetValue(IdempotencyHeader, out var idempotencyKeyValues)
                || string.IsNullOrWhiteSpace(idempotencyKeyValues.FirstOrDefault()))
            {
                // No se proporcionó clave: rechazar la petición para forzar buenas prácticas
                _logger.LogWarning(
                    "Petición al endpoint idempotente {Endpoint} sin header {Header}.",
                    context.ActionDescriptor.DisplayName,
                    IdempotencyHeader);

                context.Result = new BadRequestObjectResult(new
                {
                    success = false,
                    message = $"El header '{IdempotencyHeader}' es obligatorio para este endpoint."
                });
                return;
            }

            var idempotencyKey = idempotencyKeyValues.First()!.Trim();
            var cacheKey = $"Idempotency:{idempotencyKey}";

            // 3. Buscar en caché si ya existe una respuesta para esta clave
            if (_cache.TryGetValue(cacheKey, out CachedResponse? cachedResponse) && cachedResponse is not null)
            {
                _logger.LogInformation(
                    "Respuesta idempotente encontrada en caché para la clave {Key}. " +
                    "Devolviendo respuesta cacheada (Status {StatusCode}).",
                    idempotencyKey,
                    cachedResponse.StatusCode);

                // Devolver la respuesta cacheada sin re-ejecutar la acción
                context.Result = new ContentResult
                {
                    Content = cachedResponse.Body,
                    ContentType = cachedResponse.ContentType,
                    StatusCode = cachedResponse.StatusCode
                };
                return;
            }

            // 4. Marcar la clave como "en proceso" para evitar ejecuciones concurrentes
            //    (protección contra doble clic ultra-rápido)
            var processingKey = $"IdempotencyProcessing:{idempotencyKey}";
            if (!_cache.TryGetValue(processingKey, out _))
            {
                _cache.Set(processingKey, true, TimeSpan.FromMinutes(5));
            }
            else
            {
                _logger.LogWarning(
                    "La clave de idempotencia {Key} ya está siendo procesada. Petición rechazada.",
                    idempotencyKey);

                context.Result = new ConflictObjectResult(new
                {
                    success = false,
                    message = "Esta operación ya está siendo procesada. Por favor, espere."
                });
                return;
            }

            try
            {
                // 5. Ejecutar la acción del controlador
                var executedContext = await next();

                // 6. Si la ejecución fue exitosa (2xx), cachear la respuesta
                if (executedContext.Exception is null && executedContext.Result is ObjectResult objectResult)
                {
                    var statusCode = objectResult.StatusCode ?? 200;

                    if (statusCode >= 200 && statusCode < 300)
                    {
                        var serializedBody = JsonSerializer.Serialize(objectResult.Value, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                        });

                        var responseToCache = new CachedResponse
                        {
                            StatusCode = statusCode,
                            Body = serializedBody,
                            ContentType = "application/json; charset=utf-8"
                        };

                        var ttl = TimeSpan.FromHours(idempotentAttribute.CacheLifetimeInHours);

                        _cache.Set(cacheKey, responseToCache, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = ttl,
                            // Prioridad alta para que no sea desalojada ante presión de memoria
                            Priority = CacheItemPriority.High
                        });

                        _logger.LogInformation(
                            "Respuesta cacheada exitosamente para clave de idempotencia {Key} " +
                            "(Status {StatusCode}, TTL {TTL}h).",
                            idempotencyKey,
                            statusCode,
                            idempotentAttribute.CacheLifetimeInHours);
                    }
                    else
                    {
                        _logger.LogDebug(
                            "Respuesta no cacheada para clave {Key}: status {StatusCode} no es 2xx.",
                            idempotencyKey,
                            statusCode);
                    }
                }
                else if (executedContext.Exception is not null)
                {
                    _logger.LogWarning(
                        executedContext.Exception,
                        "Excepción durante la ejecución del endpoint idempotente con clave {Key}. " +
                        "La respuesta no será cacheada.",
                        idempotencyKey);
                }
            }
            finally
            {
                // 7. Limpiar la marca de "en proceso"
                _cache.Remove(processingKey);
            }
        }

        /// <summary>
        /// Modelo interno para almacenar la respuesta en caché.
        /// </summary>
        private sealed class CachedResponse
        {
            public int StatusCode { get; init; }
            public string Body { get; init; } = string.Empty;
            public string ContentType { get; init; } = "application/json";
        }
    }
}
