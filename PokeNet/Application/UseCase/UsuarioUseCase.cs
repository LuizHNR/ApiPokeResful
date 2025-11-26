using Infrastructure.Repositories;
using k8s.KubeConfigModels;
using PokeNet.Application.DTO.Request;
using PokeNet.Application.DTO.Response;
using PokeNet.Domain.Entities;

namespace PokeNet.Application.UseCase
{
    public class UsuarioUseCase
    {
        private readonly IRepository<Usuario> _repository;

        public UsuarioUseCase(IRepository<Usuario> repository)
        {
            _repository = repository;
        }

        public async Task<UsuarioResponse> CreateUsuarioAsync(UsuarioRequest request)
        {
            var usuario = Usuario.Create(
                request.Nome,
                request.Email,
                request.Senha,
                request.Role
            );

            await _repository.AddAsync(usuario);

            return UsuarioResponse.FromEntity(usuario);
        }

        public async Task<List<UsuarioResponse>> GetAllUsuariosAsync(int page, int pageSize)
        {
            var usuarios = await _repository.GetAllAsync();

            var paged = usuarios
               .Skip((page - 1) * pageSize)
               .Select(u => UsuarioResponse.FromEntity(u))
               .Take(pageSize)
               .Select(u => new UsuarioResponse
               {
                   Id = u.Id,
                   Nome = u.Nome,
                   Email = u.Email
               })
               .ToList();

            return paged;
        }



        public async Task<UsuarioResponse?> GetByIdAsync(string id)
        {
            var usuario = await _repository.GetByIdAsync(id);
            return usuario == null ? null : UsuarioResponse.FromEntity(usuario);
        }

        public async Task<UsuarioResponse?> UpdateUsuarioAsync(string id, UsuarioRequest request)
        {
            var usuario = await _repository.GetByIdAsync(id);
            if (usuario == null) return null;

            usuario.Atualizar(
                request.Nome,
                request.Email,
                request.Senha,
                request.Role
            );

            await _repository.UpdateAsync(id, usuario);
            return UsuarioResponse.FromEntity(usuario);
        }

        public async Task<bool> DeleteUsuarioAsync(string id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}
