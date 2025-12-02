using PokeNet.Application.DTO.External;
using System.Text.Json.Serialization;

namespace PokeNet.Application.DTO.Response
{
    public class ItemResponse
    {
        public string Nome { get; set; } = "";
        public string Sprite { get; set; } = "";
        public string Efeito { get; set; } = "";
    }

    public class ItemDetailResponse
    {
        public string Name { get; set; } = "";

        [JsonPropertyName("sprites")]
        public ItemSprites Sprites { get; set; } = new();

        [JsonPropertyName("effect_entries")]
        public List<ItemEffectEntry> EffectEntries { get; set; } = new();
    }

    public class ItemSprites
    {
        [JsonPropertyName("default")]
        public string Default { get; set; } = "";
    }

    public class ItemEffectEntry
    {
        public string Effect { get; set; } = "";

        [JsonPropertyName("short_effect")]
        public string ShortEffect { get; set; } = "";

        public LanguageInfo Language { get; set; } = new();
    }




    public class ItemListResponse
    {
        public int Count { get; set; }
        public string Next { get; set; }
        public string Previous { get; set; }
        public List<ItemListResult> Results { get; set; } = new();
    }

    public class ItemListResult
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
