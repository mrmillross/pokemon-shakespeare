using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PokemonShakespeareApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        public IActionResult Get(string pokemonName)
        {
            _logger.LogInformation($"Getting pokemon details for {pokemonName}");
            var pokemonDescription = _pokemonService.GetDescription(pokemonName);
            _logger.LogInformation($"Finding shakespeare for {pokemonName}");
            var translation = _shakespeareService.GetTranslation(pokemonDescription);
            return new JsonResult(new {name = pokemonName, description = translation});
        }
    }

    public interface IGetPokemonDetails
    {
        public string GetDescription(string pokemonName);
    }

    public interface ITranslateIntoShakespeare
    {
        public string GetTranslation(string input);
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

        public string GetTranslation(string input)
        {
            throw new System.NotImplementedException();
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

        public string GetDescription(string pokemonName)
        {
            throw new System.NotImplementedException();
        }
    }

    public class PokemonConfig
    {
        public string Uri => "https://pokeapi.co/api/v2/pokemon-species/{0}"; // pokemon name goes here
    }
}
