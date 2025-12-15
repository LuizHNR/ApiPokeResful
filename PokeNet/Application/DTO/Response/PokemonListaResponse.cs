using PokeNet.Application.DTO.External;

namespace PokeNet.Application.DTO.Response
{
    public class PokemonListaResponse
    {
        public int Numero { get; set; }
        public string Nome { get; set; } = "";
        public List<string> Tipos { get; set; } = new();

        public PokemonSprite Sprite { get; set; } = new();
    }

    public class PagedResponse<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public List<T> Items { get; set; } = new();
    }

    public class PokeApiListResponse
    {
        public int Count { get; set; }
        public List<NamedAPIResource> Results { get; set; } = new();
    }

}
