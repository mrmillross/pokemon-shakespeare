using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PokemonShakespeareApi.Controllers
{
    [ApiController]
    [Route("api/pokemon")]
    public class PokemonTranslationController : ControllerBase
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
        [Route("translation")]
        public async Task<IActionResult> GetTranslation(string pokemonName)
        {
            _logger.LogInformation($"Getting pokemon details for {pokemonName}");
            var pokemonDescription = await _pokemonService.GetDescription(pokemonName);
            _logger.LogInformation($"Finding shakespeare for {pokemonName}");
            var translation = await _shakespeareService.GetTranslation(pokemonDescription);
            return new JsonResult(new { name = pokemonName, description = translation });
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
            var tempStr = new { text = "hello there dear boy I am Matt and I live in a hat" };
            var result = await client.PostAsync(_config.Value.Url, new StringContent(JsonSerializer.Serialize(tempStr), Encoding.UTF8, "application/json"));
            return await result.Content.ReadAsStringAsync();
        }
    }

    public class ShakespeareConfig
    {
        /*
        {
            "text": "hello there dear boy I am a Matt and I live in a hat"
        }
        */
        public string Url => "https://api.funtranslations.com/translate/shakespeare";
    }

    public class PokemonGetterService : IGetPokemonDetails
    {
        private IMemoryCache _cache;
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
            var client = _httpClientFactory.CreateClient("PokemonClient");
            var result = await client.GetAsync(string.Format(_config.Value.Uri, pokemonName));
            return await result.Content.ReadAsStringAsync();
        }
    }

    public class PokemonConfig
    {
        public string Uri => "https://pokeapi.co/api/v2/pokemon-species/{0}"; // pokemon name goes here
    }
}
