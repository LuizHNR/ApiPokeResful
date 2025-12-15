namespace PokeNet.Application.DTO.Request
{
    public class PokemonFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;

        public string? Search { get; set; }

        // Gerações (ex: 7 ou "7,8")
        public List<int> Generations { get; set; } = new();

        // Tipos (ex: ghost,fairy)
        public List<string> Types { get; set; } = new();

        // Ordenação
        public string? Order { get; set; }
    }
}
