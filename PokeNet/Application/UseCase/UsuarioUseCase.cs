using Infrastructure.Repositories;
using PokeNet.Application.DTO.Request;
using PokeNet.Application.DTO.Response;
using PokeNet.Application.Services;
using PokeNet.Domain.Entities;

namespace PokeNet.Application.UseCase
{
    public class UsuarioUseCase
    {
        private readonly IRepository<Usuario> _repository;
        private readonly PokemonApiService _pokemonApi;

        public UsuarioUseCase(IRepository<Usuario> repository, PokemonApiService pokemonApi)
        {
            _repository = repository;
            _pokemonApi = pokemonApi;
        }

        public async Task<UsuarioResponse> CreateUsuarioAsync(UsuarioRequest request)
        {
            var usuario = Usuario.Create(
                request.Nome,
                request.Email,
                request.Senha,
                request.Role,
                request.Times
            );

            await _repository.AddAsync(usuario);

            return await MapToResponse(usuario);
        }

        public async Task<List<UsuarioResponse>> GetAllUsuariosAsync(int page, int pageSize)
        {
            var usuarios = await _repository.GetAllAsync();

            var paged = usuarios
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new List<UsuarioResponse>();

            foreach (var u in paged)
                result.Add(await MapToResponse(u));

            return result;
        }

        public async Task<UsuarioResponse?> GetByIdAsync(string id)
        {
            var usuario = await _repository.GetByIdAsync(id);
            if (usuario == null) return null;

            return await MapToResponse(usuario);
        }

        public async Task<UsuarioResponse?> UpdateUsuarioAsync(string id, UsuarioRequest request)
        {
            var usuario = await _repository.GetByIdAsync(id);
            if (usuario == null) return null;

            usuario.Atualizar(
                request.Nome,
                request.Email,
                request.Senha,
                request.Role,
                request.Times
            );

            await _repository.UpdateAsync(id, usuario);
            return await MapToResponse(usuario);
        }

        public async Task<bool> DeleteUsuarioAsync(string id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }



        private async Task<UsuarioResponse> MapToResponse(Usuario u)
        {
            var timesDetalhados = new List<TimeResponse>();

            foreach (var time in u.Times)
            {
                var pokemonsDetalhados = new List<PokemonTimeResponse>();

                foreach (var nomeOuId in time.Pokemons)
                {
                    var p = await _pokemonApi.BuscarPokemon(nomeOuId);
                    if (p == null) continue;

                    pokemonsDetalhados.Add(new PokemonTimeResponse
                    {
                        Numero = p.Numero,
                        Nome = p.Nome,
                        Tipos = p.Tipos,
                        Sprite = p.Sprites.front_default
                    });
                }

                timesDetalhados.Add(new TimeResponse
                {
                    Nome = time.Nome,
                    Pokemons = pokemonsDetalhados
                });
            }

            return UsuarioResponse.FromEntity(u, timesDetalhados);
        }

    }
}
