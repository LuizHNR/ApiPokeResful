namespace PokeNet.Application.DTO.External
{
    public class PokemonEvolucaoDTO
    {
        public int Numero { get; set; }
        public string Nome { get; set; } = "";
        public int? NivelParaEvoluir { get; set; }

        public object Links { get; set; } = default!;
    }

}
