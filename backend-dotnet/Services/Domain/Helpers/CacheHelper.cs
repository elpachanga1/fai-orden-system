using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Services.Domain.Helpers
{
    public class CacheHelper
    {
        // Crear una instancia de MemoryCache
        private MemoryCache cache;
        private readonly IConfiguration _configuration;
        private readonly int cacheActivityTime;
        private readonly string cacheItemFlag;

        public CacheHelper(IConfiguration configuration) {
            var options = new MemoryCacheOptions();
            cache = new MemoryCache(options);
            _configuration = configuration;
            cacheActivityTime = int.Parse(_configuration["cache:cacheActivityTime"]);
            cacheItemFlag = _configuration["cache:cacheItemFlag"];
        }

        public Models.Item[]? GetCacheData(string key)
        {
            Models.Item[]? cachedData = cache.Get(key) as Models.Item[];
            return cachedData;
        }

        public bool SetCacheData(Models.Item[] items)
        {
            // Almacenar en caché durante 10 minutos (puedes ajustar según tus necesidades)
            cache.Set(cacheItemFlag, items, DateTimeOffset.Now.AddMinutes(cacheActivityTime));
            return true;
        }
    }
}
