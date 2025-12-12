namespace PokeNet.Application.DTO.External
{
    public class PokedexDetailDto
    {
        public string Name { get; set; }
        public List<DescriptionDto> Descriptions { get; set; }
        public List<PokedexPokemonEntryDto> Pokemon_Entries { get; set; }
    }

    public class DescriptionDto
    {
        public string Description { get; set; }
        public ApiResource Language { get; set; }
    }

    public class PokedexPokemonEntryDto
    {
        public int Entry_Number { get; set; }
        public ApiResource Pokemon_Species { get; set; }
    }

    public class ApiResource
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
