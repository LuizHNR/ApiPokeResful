namespace PokeNet.Application.DTO.Response
{
    public class VersionGroupResponse
    {
        public int id { get; set; }
        public string Nome { get; set; } = "";
        public string Geracao { get; set; } = "";
        public List<string> Pokedexes { get; set; } = new();
        public List<string> Regioes { get; set; } = new();
        public List<string> Versoes { get; set; } = new();
    }
}
