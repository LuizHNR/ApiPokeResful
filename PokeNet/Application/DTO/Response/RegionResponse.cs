namespace PokeNet.Application.DTO.Response
{
    public class RegionPokemonEncounterResponse
    {
        public string Pokemon { get; set; } = "";
        public int Rate { get; set; }
        public int BaseScore { get; set; }
    }

    public class RegionLocationResponse
    {
        public string Nome { get; set; } = "";
        public List<RegionPokemonEncounterResponse> Encounters { get; set; } = new();
    }

    public class RegionResponse
    {
        public string Nome { get; set; } = "";
        public List<RegionLocationResponse> Rotas { get; set; } = new();
    }
}
