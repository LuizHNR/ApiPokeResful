namespace PokeNet.Application.DTO.Response
{
    public class PokedexPokemonResponse
    {
        public int Numero { get; set; }
        public string Nome { get; set; } = "";
    }

    public class PokedexResponse
    {
        public string Nome { get; set; } = "";
        public string Descricao { get; set; } = "";
        public List<PokedexPokemonResponse> Pokemons { get; set; } = new();
    }
}
