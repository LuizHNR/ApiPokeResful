using PokeNet.Application.DTO.External;
using System.Text.Json.Serialization;

namespace PokeNet.Application.DTO.Response
{
    public class PokemonHabilidadeResponse
    {
        public string Nome { get; set; } = "";
        public string Descricao { get; set; } = "";
    }


    public class AbilityDetailResponse
    {
        [JsonPropertyName("effect_entries")]
        public List<AbilityEffectEntry> EffectEntries { get; set; } = new();
    }

    public class AbilityEffectEntry
    {
        [JsonPropertyName("short_effect")]
        public string ShortEffect { get; set; } = "";

        [JsonPropertyName("language")]
        public LanguageInfo Language { get; set; } = new();
    }



}
