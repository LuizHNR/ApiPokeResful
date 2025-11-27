namespace PokeNet.Application.DTO.External
{
    public class PokemonSpecies
    {
        public EvolutionChainLink Evolution_Chain { get; set; } = new();
    }

    public class EvolutionChainLink
    {
        public string Url { get; set; } = "";
    }

    public class EvolutionChainResponse
    {
        public ChainLink Chain { get; set; } = new();
    }

    public class ChainLink
    {
        public SpeciesInfo Species { get; set; } = new();
        public List<EvolutionDetail> Evolution_Details { get; set; } = new();
        public List<ChainLink> Evolves_To { get; set; } = new();
    }

    public class SpeciesInfo
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
    }

    public class EvolutionDetail
    {
        public int? Min_Level { get; set; }
    }
}
