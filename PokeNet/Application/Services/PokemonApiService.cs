using PokeNet.Application.DTO.External;
using PokeNet.Application.DTO.Response;
using PokeNet.Domain.Entities;
using System.Net.Http.Json;
using System.Text.Json;

namespace PokeNet.Application.Services
{
    public class PokemonApiService
    {
        private readonly HttpClient _http;

        public PokemonApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<PokemonListaResponse>> BuscarTodos(int page, int pageSize)
        {
            int offset = (page - 1) * pageSize;

            var document = await _http.GetFromJsonAsync<JsonDocument>(
                $"pokemon?limit={pageSize}&offset={offset}"
            );

            var results = document.RootElement
                .GetProperty("results")
                .EnumerateArray();

            var lista = new List<PokemonListaResponse>();

            foreach (var item in results)
            {
                var name = item.GetProperty("name").GetString()!;
                var url = item.GetProperty("url").GetString()!;
                int id = int.Parse(url.TrimEnd('/').Split('/').Last());

                var detalhe = await _http.GetFromJsonAsync<PokemonApiDetail>($"pokemon/{id}");

                if (detalhe == null)
                    continue;

                var finalName = char.ToUpper(name[0]) + name[1..];

                lista.Add(new PokemonListaResponse
                {
                    Numero = id,
                    Nome = finalName,
                    Tipos = detalhe.Types
                        .Select(t => t.Type.Name)
                        .ToList()
                });
            }

            return lista;
        }

        public async Task<Pokemon?> BuscarPokemon(string nomeOuNumero)
        {
            try
            {
                var detalhe = await _http.GetFromJsonAsync<PokemonApiDetail>(
                    $"pokemon/{nomeOuNumero.ToLower()}"
                );

                if (detalhe == null)
                    return null;

                var baseName = detalhe.Name.Split('-')[0];
                var finalName = char.ToUpper(baseName[0]) + baseName[1..];

                return new Pokemon
                {
                    Numero = detalhe.Id,
                    Nome = finalName,
                    Altura = detalhe.Height,
                    Peso = detalhe.Weight,
                    Tipos = detalhe.Types
                        .Select(t => t.Type.Name)
                        .ToList()
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
