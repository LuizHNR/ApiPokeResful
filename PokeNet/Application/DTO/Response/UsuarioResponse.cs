using PokeNet.Domain.Entities;
using PokeNet.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PokeNet.Application.DTO.Response
{
    public class UsuarioResponse
    {
        public string Id { get; set; }
        public string Nome { get; set; }

        public string Email { get; set; }

        public string Senha { get; set; }

        [EnumDataType(typeof(Role))]
        public Role Role { get; set; }


        public static UsuarioResponse FromEntity(Usuario u)
        {
            return new UsuarioResponse
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Senha = u.Senha,
                Role = u.Role
            };
        }
    }
}
