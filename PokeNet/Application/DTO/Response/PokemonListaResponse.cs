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
}
