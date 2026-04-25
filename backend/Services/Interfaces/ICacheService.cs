using System;
using System.Threading.Tasks;

namespace Backend.Services.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? timeout = null);
        void Remove(string key);
        void RemoveByPrefix(string prefix);
    }
}
