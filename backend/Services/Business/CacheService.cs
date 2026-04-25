using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Backend.Services.Business
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;
        private readonly ConcurrentDictionary<string, bool> _cacheKeys = new();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? timeout = null)
        {
            if (_memoryCache.TryGetValue(key, out T? cachedValue))
            {
                _logger.LogDebug("Cache HIT para la key {Key}", key);
                return cachedValue;
            }

            _logger.LogDebug("Cache MISS para la key {Key}", key);

            var keyLock = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            
            await keyLock.WaitAsync();
            try
            {
                // Double check after lock
                if (_memoryCache.TryGetValue(key, out cachedValue))
                {
                    _logger.LogDebug("Cache HIT (después de lock) para la key {Key}", key);
                    return cachedValue;
                }

                var value = await factory();

                if (value != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions();
                    if (timeout.HasValue)
                    {
                        cacheOptions.SetAbsoluteExpiration(timeout.Value);
                    }
                    
                    _memoryCache.Set(key, value, cacheOptions);
                    _cacheKeys.TryAdd(key, true);
                    _logger.LogDebug("Valor guardado en cache para la key {Key} (Timeout: {Timeout})", key, timeout);
                }

                return value;
            }
            finally
            {
                keyLock.Release();
            }
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
            _logger.LogDebug("Cache eliminada para la key {Key}", key);
        }

        public void RemoveByPrefix(string prefix)
        {
            var keysToRemove = _cacheKeys.Keys.Where(k => k.StartsWith(prefix)).ToList();
            
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _cacheKeys.TryRemove(key, out _);
            }

            if (keysToRemove.Any())
            {
                _logger.LogInformation("Caché invalidada para prefix {Prefix}. Keys afectadas: {Count}", prefix, keysToRemove.Count);
            }
        }
    }
}
