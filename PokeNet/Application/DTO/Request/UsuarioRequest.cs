using PokeNet.Domain.Entities;
using PokeNet.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PokeNet.Application.DTO.Request
{
    public class UsuarioRequest
    {
        public string Nome { get; set; }

        /// <example>email@gmail.com</example>
        public string Email { get; set; }

        /// <example>SenhaForte@010203</example>
        public string Senha { get; set; }

        [EnumDataType(typeof(Role))]
        public Role Role { get; set; }

        public List<string> Time { get; set; }

    }
}
