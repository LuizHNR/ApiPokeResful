using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PokeNet.Domain.Entities
{
    public class Time
    {

        [BsonElement("nome")]
        public string Nome { get; set; }

        [BsonElement("pokemons")]
        public List<string> Pokemons { get; set; } = new();

    }
}
