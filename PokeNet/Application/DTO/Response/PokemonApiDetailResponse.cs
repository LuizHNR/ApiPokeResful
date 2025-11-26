namespace PokeNet.Application.DTO.Response
{
    public class PokemonApiDetailResponse
    {
        public int Numero { get; set; }
        public string Nome { get; set; } = "";
        public string Altura { get; set; } = "";
        public string Peso { get; set; } = "";
        public List<string> Tipos { get; set; } = new();
    }

}
