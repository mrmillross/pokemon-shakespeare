using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PokemonShakespeareApi.Config;
using PokemonShakespeareApi.Interfaces;

namespace PokemonShakespeareApi.Services
{
    /// <summary>
    ///     A simple shakespeare translation service, with a (naive) caching implementation
    /// </summary>
    /// <remarks>
    ///     If I had more time, I would...
    ///         1.) add cache locking, so that the implementation is less naive/concurrent-access-friendly
    /// </remarks>
    public class ShakespeareTranslationService : ITranslateIntoShakespeare
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<ShakespeareConfig> _config;

        public ShakespeareTranslationService(IMemoryCache cache, IHttpClientFactory httpClientFactory, IOptions<ShakespeareConfig> config)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<string> GetTranslation(string pokemonName, string rawTextInput)
        {
            var stringResult = await GetCacheResponse(pokemonName, () => GetShakespeareTranslationFromRemoteApi(rawTextInput));
            return stringResult;
        }

        private async Task<string> GetShakespeareTranslationFromRemoteApi(string rawTextInput)
        {
            var client = _httpClientFactory.CreateClient("TranslationClient");
            var requestBody = new { text = rawTextInput };
            var result = await client.PostAsync(_config.Value.Url, new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));
            return await result.Content.ReadAsStringAsync();
        }

        private async Task<string> GetCacheResponse(string cacheKey, Func<Task<string>> getFromRemoteResourceFunc)
        {
            var fullCacheKey = $"{cacheKey}-translation";
            if (_cache.TryGetValue(fullCacheKey, out string shakeSpeareTranslation))
            {
                return shakeSpeareTranslation;
            }

            var result = await getFromRemoteResourceFunc();
            _cache.Set(fullCacheKey, result, TimeSpan.FromMinutes(_config.Value.CacheExpiryMinutes));
            return result;
        }
    }
}