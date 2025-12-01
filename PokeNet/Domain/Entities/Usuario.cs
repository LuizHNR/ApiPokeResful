using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PokeNet.Domain.Enums;

namespace PokeNet.Domain.Entities
{
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("nome")]
        public string Nome { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("senha")]
        public string Senha { get; set; }

        [BsonElement("role")]
        [BsonRepresentation(BsonType.String)]
        public Role Role { get; set; }

        [BsonElement("times")]
        public List<Time> Times { get; set; } = new(); 

        public Usuario(string nome, string email, string senha, Role role, List<Time> times)
        {
            Nome = nome;
            Email = email;
            Senha = senha;
            Role = role;
            Times = times ?? new List<Time>();

        }

        public void Atualizar(string nome, string email, string senha, Role role, List<Time> times)
        {
            Nome = nome;
            Email = email;
            Senha = senha;
            Role = role;
            Times = times ?? new List<Time>();
        }

        internal static Usuario Create(string nome, string email, string senha, Role role, List<Time> times)
        {
            return new Usuario(nome, email, senha, role, times ?? new List<Time>());
        }

        public Usuario() { }

    }
}
