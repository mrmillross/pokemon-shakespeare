using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using PokemonShakespeareApi.Config;
using PokemonShakespeareApi.Controllers;
using PokemonShakespeareApi.Services;
using Shouldly;
using Xunit;

namespace PokemonShakespeareApi.Tests
{
    public class PokemonTranslationControllerTests
    {
        private readonly PokemonTranslationController _controller;
        private IMemoryCache _shakespeareTranslationCache;
        private IMemoryCache _pokemonFlavorTextCache;
        private IHttpClientFactory _httpClientFactory;

        public PokemonTranslationControllerTests()
        {
            var clientHandlerStub = new FakeMessageHandler((request, cancellationToken) =>
            {
                // a very crude way of returning the translation on a post and the flavor json on a get.
                if (request.Method == HttpMethod.Post)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ExpectedTranslation) });
                }
                else if (request.Method == HttpMethod.Get && request.RequestUri.AbsolutePath.Contains("snorlax"))
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(File.ReadAllText("snorlax_species.json"), Encoding.UTF8, "application/json") });
                }
                else
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
                }

            });
            var httpClient = new HttpClient(clientHandlerStub);
            _shakespeareTranslationCache = Substitute.For<IMemoryCache>();
            _pokemonFlavorTextCache = Substitute.For<IMemoryCache>();
            _httpClientFactory = Substitute.For<IHttpClientFactory>();
            _httpClientFactory.CreateClient("TranslationClient").Returns(httpClient);
            _httpClientFactory.CreateClient("PokemonClient").Returns(httpClient);

            var shakespeareTranslationServiceMock = new ShakespeareTranslationService(_shakespeareTranslationCache, _httpClientFactory, Options.Create(new ShakespeareConfig { CacheExpiryMinutes = 1, Url = "https://shakespeare" }));
            var pokemonFlavorTextServicMock = new PokemonGetterService(_pokemonFlavorTextCache, _httpClientFactory, Options.Create(new PokemonConfig { CacheExpiryMinutes = 1, Url = "https://pokemon/{0}" }));

            _controller = new PokemonTranslationController(Substitute.For<ILogger<PokemonTranslationController>>(), shakespeareTranslationServiceMock, pokemonFlavorTextServicMock);
        }

        [Fact]
        public async Task Should_Return_Translated_Text_For_A_Snorlax()
        {
            var result = (ContentResult)await _controller.GetTranslation("snorlax");
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(200);
            result.Content.ShouldBe(ExpectedTranslation);
        }

        [Fact]
        public async Task Should_Handle_NonExistent_Pokemon_And_Return_A_400()
        {
            var result = (ContentResult)await _controller.GetTranslation("not_pokemon");
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(400);
            result.Content.ShouldBe("Unable to translate pokemon text for not_pokemon");
        }

        [Fact]
        public async Task Should_Cache_Snorlax_Json()
        {
            _ = await _controller.GetTranslation("snorlax");
            _pokemonFlavorTextCache.Received(1).CreateEntry(Arg.Is<string>(x => x == "snorlax-pokemon"));
            _shakespeareTranslationCache.Received(1).CreateEntry(Arg.Is<string>(x => x == "snorlax-translation"));
        }

        private const string ExpectedTranslation =
            "Very distemperate. Just engluts and sleeps. As its rotund bulk builds,  't becomes steadily moo slothful,  wilt englut aught,  coequal if 't be true the food happeneth to beest a dram fusty. 't nev'r gets an did upset stomach,  what sounds like its caterwauling may ac­ tually beest its snores or the rumblings of its fill'd with pangs of hunger belly,  its stomach's di­ gestive juices can dissolve any kind of poison. 't can coequal englut things off the did grind,  this pokémon's stomach is so stout,  coequal eating fusty or rotten food wilt not affect 't,  snorlax’s typical day consists of nothing moo than eating and sleeping. 't is such a docile pokémon yond thither art children who is't useth its expansive belly as a lodging to playeth,  snorlax’s typical day consists of nothing moo than eating and sleeping. 't is such a docile pokémon yond thither art children who is't useth its big belly as a lodging to playeth,  't is not did satisfy unless 't engluts ov'r 880 pounds of food every day. At which hour 't is done eating,  't goeth promptly to catch but a wink,  very distemperate. Just engluts and sleeps. As its rotund bulk builds,  't becomes steadily moo slothful,  its stomach can digest any kind of food,  coequal if 't be true 't happeneth to beest fusty or rotten,  't stops eating only to catch but a wink. 't doesn’t feeleth full unless 't engluts nearly 900 pounds a day,  at which hour its belly is full,  't becomes too lethargic to coequal lift a digit,  so 't is safe to bounce on its belly,  what sounds like its caterwauling may actually beest its snores or the rumblings of its fill'd with pangs of hunger belly,  its stomach’s digestive juices can dissolve any kind of poison. 't can coequal englut things off the did grind,  't is not did satisfy unless 't engluts ov'r 880 pounds of food every day. At which hour 't is done eating,  't goeth promptly to catch but a wink,  its stomach can digest any kind of food,  coequal if 't be true 't happeneth to beest fusty or rotten,  snorlax’s typical day consists of nothing moo than eating and sleeping. 't is such a docile pokémon yond thither art children who is't useth its expansive belly as a lodging to playeth,  its stomach is did doth sayeth to beest incomparably stout. Coequal muk’s poison is nothing moo than a hint of spice on snorlax’s tongue,  't engluts nearly 900 pounds of food every day. 't starts nodding off while eating—and continues to englut coequal while 't’s asleep,  't doesn’t doth aught other than englut and catch but a wink. At which hour prompted to maketh a serious effort,  though,  't apparently displays like a silver bow power,  't hath nay interest in aught other than eating. Coequal if 't be true thee climb up on its stomach while 't’s napping,  't doesn’t seemeth to mind at all,  wilt englut aught,  coequal if 't be true the food happeneth to beest a dram fusty. 't nev'r gets an did upset stomach,  gigantamax energy hath affectioned stray seeds and coequal pebbles yond did get did stick to snorlax,  making those folk groweth to a huge size,  't is not did satisfy unless 't engluts ov'r 880 pounds of food every day. At which hour 't is done eating,  't goeth promptly to catch but a wink,  terrifyingly stout,  this pokémon is the size of a mountain—and moves about as much as one as well,  this pokémon’s stomach is so stout,  coequal eating fusty or rotten food wilt not affect 't";
    }

    public class FakeMessageHandler : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;
        public FakeMessageHandler()
        {
            _handlerFunc = (request, cancellationToken) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        public FakeMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
        {
            _handlerFunc = handlerFunc;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _handlerFunc(request, cancellationToken);
        }
    }
}
