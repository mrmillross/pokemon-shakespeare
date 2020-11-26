using System.Threading.Tasks;

namespace PokemonShakespeareApi.Interfaces
{
    public interface ITranslateIntoShakespeare
    {
        public Task<string> GetTranslation(string pokemonName, string rawTextInput);
    }
}