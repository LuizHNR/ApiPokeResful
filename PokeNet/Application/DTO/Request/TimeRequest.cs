using MongoDB.Bson.Serialization.Attributes;

namespace PokeNet.Application.DTO.Request
{
    public class TimeRequest
    {
        public string Nome { get; set; }
        public List<string> Pokemons { get; set; } = new();
    }
}
