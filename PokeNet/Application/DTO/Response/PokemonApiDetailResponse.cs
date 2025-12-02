using PokeNet.Application.DTO.External;

namespace PokeNet.Application.DTO.Response
{
    public class PokemonApiDetailResponse
    {
        public int Numero { get; set; }
        public string Nome { get; set; } = "";
        public string Descricao { get; set; } = "";
        public string CryUrl { get; set; } = "";
        public string Altura { get; set; } = "";
        public string Peso { get; set; } = "";
        public List<PokemonHabilidadeResponse> Habilidades { get; set; } = new();
        public List<string> Tipos { get; set; } = new();
        public List<string> EggGroups { get; set; } = new();
        public PokemonMultipliersResponse Multipliers { get; set; } = new();

        public List<PokemonEvolucaoDTO> Evolucoes { get; set; } = new();
        public PokemonSprite Sprite { get; set; } = new();

        public List<PokemonStatResponse> Stats { get; set; } = new();

    }

}
