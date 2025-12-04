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

        public async Task<List<PokemonListaResponse>> BuscarTodos()
        {
            var lista = await _api.BuscarTodos();

            return lista;
        }




        public async Task<PokemonApiDetailResponse?> BuscarPokemon(string nomeOuNumero)
        {
            var pokemon = await _api.BuscarPokemon(nomeOuNumero);

            if (pokemon == null)
                return null;

            var multipliers = await _api.ObterFraquezasEVantagens(pokemon.Tipos);

            return new PokemonApiDetailResponse
            {
                Numero = pokemon.Numero,
                Nome = pokemon.Nome,
                Descricao = pokemon.Descricao,
                CryUrl = pokemon.CryUrl,
                Altura = ConverterAltura(pokemon.Altura),
                Peso = ConverterPeso(pokemon.Peso),
                Habilidades = pokemon.Habilidades,
                Tipos = pokemon.Tipos,
                EggGroups = pokemon.EggGroups,
                Multipliers = multipliers,
                Evolucoes = pokemon.Evolucoes,
                Sprite = pokemon.Sprites,
                Stats = pokemon.Stats
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

            // Se for menor que 1 kg, converte para gramas
            if (kg < 1)
            {
                double g = kg * 1000;
                return $"{g:0} g";
            }

            // Se for menor que 1000 kg, fica em kg
            if (kg < 1000)
            {
                return $"{kg:0.0} kg";
            }

            // Acima de 1000 kg vira toneladas
            double t = kg / 1000;
            return $"{t:0.00} t";
        }



        public async Task<PokemonMovesResponse?> BuscarMovimentos(string nomeOuNumero)
        {
            return await _api.BuscarTodosMovimentos(nomeOuNumero);
        }

    }
}
