namespace PokeNet.Domain.Entities
{
    public class Pokemon
    {
        public int Numero { get; set; }
        public string Nome { get; set; }
        public long Altura { get; set; }
        public long Peso { get; set; }
        public List<string> Tipos { get; set; } = new();
    }
}
