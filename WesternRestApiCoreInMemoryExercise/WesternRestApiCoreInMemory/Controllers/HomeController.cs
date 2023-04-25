using AutoMapper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography.Xml;

namespace WesternRestApiCoreInMemory.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;

        public HomeController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string cacheKey) 
        {
            if (_memoryCache.TryGetValue(cacheKey, out var item))
            {
                return Ok(new { item = item });
            }

            item = await LongRunningProcess();

            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
                SlidingExpiration = TimeSpan.FromSeconds(1200)
            };

            //_memoryCache.Set(cacheKey, item, options);
            _memoryCache.Set(cacheKey, item);
            return Ok(new { item = item });
        }

        private static async Task<int> LongRunningProcess()
        {
            await Task.Delay(1000);

            var random = new Random();
            return random.Next(1000, 2000);
        }
    }
}
