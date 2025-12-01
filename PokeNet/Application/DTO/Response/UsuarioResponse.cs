using PokeNet.Domain.Entities;
using PokeNet.Domain.Enums;

namespace PokeNet.Application.DTO.Response
{
    public class UsuarioResponse
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public Role Role { get; set; }

        public List<TimeResponse> Times { get; set; }

        public static UsuarioResponse FromEntity(Usuario u, List<TimeResponse> times)
        {
            return new UsuarioResponse
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Senha = u.Senha,
                Role = u.Role,
                Times = times
            };
        }
    }
}
