using PokeNet.Application.DTO.External;

namespace PokeNet.Application.DTO.Response
{
    public class PokedexPokemonResponse
    {
        public int NumeroRegional { get; set; } // 1

        public int NumeroGlobal { get; set; }   // 252
        public string Nome { get; set; } = "";
        public List<string> Tipos { get; set; } = new();

        public PokemonSprite Sprite { get; set; } = new();
    }

    public class PokedexResponse
    {
        public string Nome { get; set; } = "";
        public string Descricao { get; set; } = "";
        public List<PokedexPokemonResponse> Pokemons { get; set; } = new();
    }
}
