using PokeNet.Domain.Entities;
using PokeNet.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PokeNet.Application.DTO.Request
{
    public class UsuarioRequest
    {
        public string Nome { get; set; }

        public string Email { get; set; }

        public string Senha { get; set; }

        [EnumDataType(typeof(Role))]
        public Role Role { get; set; }

    }
}
