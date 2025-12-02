using PokeNet.Application.DTO.External;
using System.Text.Json.Serialization;

namespace PokeNet.Application.DTO.Response
{
    public class PokemonMovesResponse
    {
        public List<PokemonLevelUpMove> LevelUp { get; set; } = new();
        public List<PokemonMoveDetail> Machine { get; set; } = new();
        public List<PokemonMoveDetail> Tutor { get; set; } = new();
        public List<PokemonMoveDetail> Egg { get; set; } = new();
        public List<string> Outros { get; set; } = new();
    }



    public class PokemonLevelUpMove
    {
        public string Nome { get; set; } = "";
        public int Level { get; set; }

        public string Tipo { get; set; } = "";
        public string Categoria { get; set; } = ""; 
        public int? Poder { get; set; }
        public int? Accuracy { get; set; }
        public int PP { get; set; }
        public string Efeito { get; set; } = "";
        public string EfeitoCurto { get; set; } = "";
        public int? ChanceEfeito { get; set; }

    }


    public class MoveDetailResponse
    {
        public int? accuracy { get; set; }
        public int? power { get; set; }
        public int pp { get; set; }

        public NamedAPIResource type { get; set; } = new();

        [JsonPropertyName("damage_class")]
        public NamedAPIResource DamageClass { get; set; } = new();

        [JsonPropertyName("effect_entries")]
        public List<MoveEffectEntry> EffectEntries { get; set; } = new();

        [JsonPropertyName("effect_chance")]
        public int? EffectChance { get; set; }
    }



    public class MoveEffectEntry
    {
        [JsonPropertyName("effect")]
        public string Effect { get; set; } = "";

        [JsonPropertyName("short_effect")]
        public string ShortEffect { get; set; } = "";

        public LanguageInfo Language { get; set; } = new();
    }



}
