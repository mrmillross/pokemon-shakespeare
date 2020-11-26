using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PokemonShakespeareApi.Controllers
{
    [ApiController]
    [Route("api/pokemon")]
    public class PokemonTranslationController : Controller
    {
        private readonly ILogger<PokemonTranslationController> _logger;
        private readonly ITranslateIntoShakespeare _shakespeareService;
        private readonly IGetPokemonDetails _pokemonService;

        public PokemonTranslationController(ILogger<PokemonTranslationController> logger, ITranslateIntoShakespeare shakespeareService, IGetPokemonDetails pokemonService)
        {
            _logger = logger;
            _shakespeareService = shakespeareService;
            _pokemonService = pokemonService;
        }

        [HttpGet]
        [Route("translation/{pokemonName}")]
        public async Task<IActionResult> GetTranslation(string pokemonName)
        {
            _logger.LogInformation($"Getting pokemon details for {pokemonName}");
            var pokemonDescription = await _pokemonService.GetDescription(pokemonName);
            _logger.LogInformation($"Finding shakespeare for {pokemonName}");
            var translation = await _shakespeareService.GetTranslation(pokemonDescription);
            return Json(translation);
        }

        [Route("health")]
        public IActionResult Healthy()
        {
            return new StatusCodeResult(200);
        }
    }

    public interface IGetPokemonDetails
    {
        public Task<string> GetDescription(string pokemonName);
    }

    public interface ITranslateIntoShakespeare
    {
        public Task<string> GetTranslation(string input);
    }

    public class ShakespeareTranslationService : ITranslateIntoShakespeare
    {
        private IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<ShakespeareConfig> _config;

        public ShakespeareTranslationService(IMemoryCache cache, IHttpClientFactory httpClientFactory, IOptions<ShakespeareConfig> config)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<string> GetTranslation(string input)
        {
            var client = _httpClientFactory.CreateClient("TranslationClient");
            var tempStr = new { text = input };
            var result = await client.PostAsync(_config.Value.Url, new StringContent(JsonSerializer.Serialize(tempStr), Encoding.UTF8, "application/json"));
            return await result.Content.ReadAsStringAsync();
        }
    }

    public class ShakespeareConfig
    {
        public string Url { get; set; }
    }

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

            // Phew - this is a little bit belt-and-braces, but I'm not going to fuss over it too much in the context of this test
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
            var stringResult = await result.Content.ReadAsStringAsync();
            return stringResult;
        }

        private async Task<string> GetCacheResponse(string cacheKey, Func<Task<string>> getFromRemoteResourceFunc)
        {
            if (_cache.TryGetValue(cacheKey, out string flavorText))
            {
                return flavorText;
            }

            var result = await getFromRemoteResourceFunc();
            _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
            return result;
        }
    }

    public class PokemonConfig
    {
        public string Url { get; set; }
    }

    public class PokemonSpecies
    {
        [JsonPropertyName("flavor_text_entries")]
        public List<FlavorTextEntry> FlavorTextEntries { get; set; }
    }

    public class FlavorTextEntry
    {
        [JsonPropertyName("flavor_text")]
        public string FlavorText { get; set; }

        [JsonPropertyName("language")]
        public Language Language { get; set; }
    }

    public class Language
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}