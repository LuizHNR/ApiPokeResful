namespace PokeNet.Application.DTO.External
{
    public class PokemonMoveDetail
    {
        public string Nome { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Categoria { get; set; } = "";
        public int? Poder { get; set; }
        public int? Accuracy { get; set; }
        public int PP { get; set; }
        public string Efeito { get; set; } = "";
        public string EfeitoCurto { get; set; } = "";
        public int? ChanceEfeito { get; set; }
    }

}
