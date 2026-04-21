namespace Backend.Filters
{
    /// <summary>
    /// Marca un endpoint como idempotente. Las peticiones con el mismo header
    /// "X-Idempotency-Key" devolverán la respuesta cacheada en lugar de
    /// ejecutar la acción nuevamente. Diseñado para proteger operaciones
    /// críticas como la emisión de comprobantes contra duplicaciones por
    /// doble clic, reintentos por timeout o micro-cortes de red.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class IdempotentAttribute : Attribute
    {
        /// <summary>
        /// Tiempo de vida de la respuesta cacheada. Por defecto 24 horas.
        /// </summary>
        public int CacheLifetimeInHours { get; set; } = 24;
    }
}
