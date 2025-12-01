using System.Text.Json.Serialization;

namespace PokeNet.Application.DTO.External
{
    public class PokemonApiDetail
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Height { get; set; }
        public int Weight { get; set; }
        public List<PokemonAbility> Abilities { get; set; } = new();
        public List<PokemonTypeSlot> Types { get; set; } = new();
        public List<PokemonEvolucaoDTO> Evolucoes { get; set; } = new();
        public PokemonSprite Sprites { get; set; } = new();

        public List<PokemonStatsSlot> Stats { get; set; } = new();
    }

    public class PokemonTypeSlot
    {
        public int Slot { get; set; }
        public PokemonType Type { get; set; } = new();
    }

    public class PokemonType
    {
        public string Name { get; set; } = "";
    }










    public class PokemonStatsSlot
    {
        [JsonPropertyName("base_stat")]
        public int StatusBase { get; set; }

        [JsonPropertyName("stat")]
        public PokemonStats Stat { get; set; } = new();
    }

    public class PokemonStats
    {
        public string Name { get; set; } = "";
    }









    public class PokemonAbility
    {
        public AbilityInfo Ability { get; set; } = new();
        public bool Is_Hidden { get; set; }
        public int Slot { get; set; }

    }


    public class AbilityInfo
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
    }











    public class PokemonSprite
    {
        public string front_default { get; set; } = "";
        public string back_default { get; set; } = "";
    }


}
