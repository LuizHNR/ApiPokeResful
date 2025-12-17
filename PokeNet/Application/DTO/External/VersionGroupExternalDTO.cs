namespace PokeNet.Application.DTO.External
{
    public class VersionGroupListResponse
    {
        public List<NamedApiResource> Results { get; set; } = new();
    }

    public class VersionGroupDetailResponse
    {
        public int Id { get; set; } 
        public string Name { get; set; } = "";
        public NamedApiResource Generation { get; set; } = new();
        public List<NamedApiResource> Pokedexes { get; set; } = new();
        public List<NamedApiResource> Regions { get; set; } = new();
        public List<NamedApiResource> Versions { get; set; } = new();
    }

    public class NamedApiResource
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
    }
}
