using PokeNet.Application.DTO.External;
using PokeNet.Application.DTO.Response;
using PokeNet.Application.Services;
using PokeNet.Domain.Entities;

namespace PokeNet.Application.UseCase
{
    public class PokemonUseCase
    {
        private readonly PokemonApiService _api;

        public PokemonUseCase(PokemonApiService api)
        {
            _api = api;
        }

        public async Task<List<PokemonListaResponse>> BuscarTodos(int page, int pageSize)
        {
            var lista = await _api.BuscarTodos(page, pageSize);

            return lista;
        }




        public async Task<PokemonApiDetailResponse?> BuscarPokemon(string nomeOuNumero)
        {
            var pokemon = await _api.BuscarPokemon(nomeOuNumero);

            if (pokemon == null)
                return null;

            return new PokemonApiDetailResponse
            {
                Numero = pokemon.Numero,
                Nome = pokemon.Nome,
                Altura = ConverterAltura(pokemon.Altura),
                Peso = ConverterPeso(pokemon.Peso),
                Tipos = pokemon.Tipos,
                Evolucoes = pokemon.Evolucoes,
                Sprite = pokemon.Sprites
            };
        }

        private string ConverterAltura(long alturaDm)
        {
            double cm = alturaDm * 10;

            if (cm < 100)
                return $"{cm} cm";

            double m = cm / 100;

            if (m < 100)
                return $"{m:0.0} m";

            double km = m / 1000;
            return $"{km:0.00} km";
        }

        private string ConverterPeso(long pesoHg)
        {
            double kg = pesoHg * 0.1;

            if (kg < 1000)
                return $"{kg} kg";

            double t = kg / 1000;
            return $"{t:0.00} t";

        }
    }
}
