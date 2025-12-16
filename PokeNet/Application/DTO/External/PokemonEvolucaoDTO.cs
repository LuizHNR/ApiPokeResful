namespace PokeNet.Application.DTO.External
{
    public class PokemonEvolucaoDTO
    {
        public int Numero { get; set; }
        public string Nome { get; set; } = "";

        public int? NivelParaEvoluir { get; set; }

        public string Metodo { get; set; } = "";   // level, item, trade, happiness...
        public string? Detalhe { get; set; }        // Nv.16, Fire Stone, Troca, etc

        public PokemonSprite Sprite { get; set; } = new();
        public object Links { get; set; } = default!;
    }


}
