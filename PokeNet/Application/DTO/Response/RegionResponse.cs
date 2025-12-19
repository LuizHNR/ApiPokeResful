namespace PokeNet.Application.DTO.Response
{
    public class RegionPokemonEncounterResponse
    {
        public RegionPokemonResponse Pokemon { get; set; } = null!;
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


    public class RegionPokemonResponse
    {
        public int Numero { get; set; }
        public string Nome { get; set; } = "";
        public List<string> Tipos { get; set; } = new();
        public object Sprite { get; set; } = null!;
    }

}
