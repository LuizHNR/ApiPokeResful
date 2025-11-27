namespace PokeNet.Application.DTO.Response
{
    public class PokemonTimeResponse
    {
        public int Numero { get; set; }
        public string Nome { get; set; }
        public List<string> Tipos { get; set; }
        public string Sprite { get; set; } = "";
    }

}
