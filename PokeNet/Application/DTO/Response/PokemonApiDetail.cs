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

        public List<PokemonMoveSlot> Moves { get; set; } = new();


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

        public string front_shiny { get; set; } = "";
        public string back_shiny { get; set; } = "";
    }





    public class PokemonSpeciesApi
    {
        [JsonPropertyName("flavor_text_entries")]
        public List<FlavorTextEntry> FlavorTextEntries { get; set; } = new();

        [JsonPropertyName("egg_groups")]
        public List<EggGroupEntry> EggGroups { get; set; } = new();
    }

    public class FlavorTextEntry
    {
        [JsonPropertyName("flavor_text")]
        public string FlavorText { get; set; } = "";

        [JsonPropertyName("language")]
        public LanguageInfo Language { get; set; } = new();
    }

    public class EggGroupEntry
    {
        public string Name { get; set; } = "";
    }

    public class LanguageInfo
    {
        public string Name { get; set; } = "";
    }




    public class NamedAPIResource
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("url")]
        public string Url { get; set; } = "";
    }


    public class PokemonMoveSlot
    {
        [JsonPropertyName("move")]
        public NamedAPIResource Move { get; set; } = new();

        [JsonPropertyName("version_group_details")]
        public List<VersionGroupDetail> Version_Group_Details { get; set; } = new();
    }


    public class VersionGroupDetail
    {
        [JsonPropertyName("level_learned_at")]
        public int Level { get; set; }

        [JsonPropertyName("move_learn_method")]
        public NamedAPIResource MoveLearnMethod { get; set; } = new();

        [JsonPropertyName("version_group")]
        public NamedAPIResource VersionGroup { get; set; } = new();
    }

}
