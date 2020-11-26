using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PokemonShakespeareApi.Config;
using PokemonShakespeareApi.Interfaces;
using PokemonShakespeareApi.Models;

namespace PokemonShakespeareApi.Services
{
    /// <summary>
    ///     A simple pokemon getting service, with a (naive) caching implementation to prevent spamming the remote api for the same pokemon
    /// </summary>
    /// <remarks>
    ///     If I had more time, I would...
    ///         1.) add cache locking, so that the implementation is less naive/concurrent-access-friendly
    ///         2.) not cache the 429 response from the pokemon api when rate limited!
    /// </remarks>
    public class PokemonGetterService : IGetPokemonDetails
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<PokemonConfig> _config;

        public PokemonGetterService(IMemoryCache cache, IHttpClientFactory httpClientFactory, IOptions<PokemonConfig> config)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<string> GetDescription(string pokemonName)
        {
            var stringResult = await GetCacheResponse(pokemonName, () => GetFlavorTextFromRemoteApi(pokemonName));
            
            var parsedSpecies = JsonSerializer.Deserialize<PokemonSpecies>(stringResult);

            // this is a little bit belt-and-braces, but for the purposes of this tech test I think it's okay.
            var flavorText = parsedSpecies.FlavorTextEntries
                .Where(x => x.Language.Name == "en")
                .Select(x => x.FlavorText)
                .Distinct();

            return string.Join(", ", flavorText);
        }

        private async Task<string> GetFlavorTextFromRemoteApi(string pokemonName)
        {
            var client = _httpClientFactory.CreateClient("PokemonClient");
            var result = await client.GetAsync(string.Format(_config.Value.Url, pokemonName));
            if (!result.IsSuccessStatusCode)
                throw new Exception($"Unable to retrieve content for {pokemonName}");

            var stringResult = await result.Content.ReadAsStringAsync();
            return stringResult;
        }

        private async Task<string> GetCacheResponse(string cacheKey, Func<Task<string>> getFromRemoteResourceFunc)
        {
            var fullCacheKey = $"{cacheKey}-pokemon";
            if (_cache.TryGetValue(fullCacheKey, out string flavorText))
            {
                return flavorText;
            }

            var result = await getFromRemoteResourceFunc();
            _cache.Set(fullCacheKey, result, TimeSpan.FromMinutes(_config.Value.CacheExpiryMinutes));
            return result;
        }
    }
}