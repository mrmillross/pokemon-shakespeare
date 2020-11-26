using System.Text.Json.Serialization;

namespace PokemonShakespeareApi.Models
{
    public class Language
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}