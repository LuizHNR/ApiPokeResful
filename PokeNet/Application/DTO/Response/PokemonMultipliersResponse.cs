namespace PokeNet.Application.DTO.Response
{
    public class PokemonMultipliersResponse
    {
        public Dictionary<string, double> Fraquezas { get; set; } = new();
        public Dictionary<string, double> Resistencias { get; set; } = new();
        public Dictionary<string, double> Imunidades { get; set; } = new();
    }
}
