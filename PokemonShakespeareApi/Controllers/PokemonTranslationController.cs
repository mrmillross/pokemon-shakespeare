using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonShakespeareApi.Interfaces;

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
            var speciesFlavorText = await _pokemonService.GetDescription(pokemonName);
            _logger.LogInformation($"Finding shakespeare for {pokemonName}");
            var translation = await _shakespeareService.GetTranslation(pokemonName, speciesFlavorText);
            _logger.LogInformation("Translation complete!");
            return Content(translation, "application/json");
        }

        [Route("health")]
        public IActionResult Healthy()
        {
            return new StatusCodeResult(200);
        }
    }
}