using PokeNet.Application.DTO.External;
using PokeNet.Application.DTO.Response;

namespace PokeNet.Domain.Entities
{
    public class Pokemon
    {
        public int Numero { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; } = "";
        public string CryUrl { get; set; } = "";
        public long Altura { get; set; }
        public long Peso { get; set; }
        public List<PokemonHabilidadeResponse> Habilidades { get; set; } = new();
        public List<string> Tipos { get; set; } = new();
        public List<string> EggGroups { get; set; } = new();
        public List<PokemonEvolucaoDTO> Evolucoes { get; set; } = new();

        public PokemonSprite Sprites { get; set; } = new();

        public List<PokemonStatResponse> Stats { get; set; } = new();

    }
}
