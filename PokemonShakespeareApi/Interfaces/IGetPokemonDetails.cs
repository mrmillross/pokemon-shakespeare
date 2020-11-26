using System.Threading.Tasks;

namespace PokemonShakespeareApi.Interfaces
{
    public interface IGetPokemonDetails
    {
        public Task<string> GetDescription(string pokemonName);
    }
}