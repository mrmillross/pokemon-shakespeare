using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PokemonShakespeareApi.Models
{
    public class PokemonSpecies
    {
        [JsonPropertyName("flavor_text_entries")]
        public List<FlavorTextEntry> FlavorTextEntries { get; set; }
    }
}