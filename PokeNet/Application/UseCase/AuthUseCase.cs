using Infrastructure.Repositories;
using k8s.KubeConfigModels;
using Microsoft.IdentityModel.Tokens;
using PokeNet.Application.DTO.Request;
using PokeNet.Application.DTO.Response;
using PokeNet.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PokeNet.Application.UseCase
{
    public class AuthUseCase
    {
        private readonly IRepository<Usuario> _repository;
        private readonly IConfiguration _configuration;

        public AuthUseCase(IRepository<Usuario> repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public async Task<AuthResponse?> LoginAsync(AuthRequest request)
        {
            var usuarios = await _repository.GetAllAsync();
            var usuario = usuarios.FirstOrDefault(u => u.Email == request.Email);

            if (usuario == null)
                return null;

            if (usuario.Senha != request.Senha)
                return null;

            var token = GenerateToken(usuario);

            return new AuthResponse
            {
                Token = token,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Role = usuario.Role.ToString(),
            };
        }

        private string GenerateToken(Usuario usuario)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Role.ToString())
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
