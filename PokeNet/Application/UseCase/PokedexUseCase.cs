using Microsoft.Extensions.Caching.Memory;
using PokeNet.Application.DTO.External;
using PokeNet.Application.DTO.Response;
using PokeNet.Application.Services;
using System.Net.Http.Json;

namespace PokeNet.Application.UseCases
{
    public class PokedexUseCase
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly PokemonApiService _pokemonApi;


        public PokedexUseCase(IHttpClientFactory factory, IMemoryCache cache, PokemonApiService pokemonApi
)
        {
            _http = factory.CreateClient("PokeApi");
            _cache = cache;
            _pokemonApi = pokemonApi;

        }

        private async Task<T> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<Task<T>> factory)
        {
            if (_cache.TryGetValue(key, out T value))
                return value;

            value = await factory();
            _cache.Set(key, value, ttl);

            return value;
        }


        public async Task<PokedexResponse?> Buscar(string nomeOuId, string? search)
        {
            return await GetOrCreateAsync(
                $"pokedex_{nomeOuId}_search_{search?.ToLower() ?? "all"}",
                TimeSpan.FromHours(24),
                async () =>
                {
                    var dto = await _http.GetFromJsonAsync<PokedexDetailDto>($"pokedex/{nomeOuId}");
                    if (dto == null) return null;

                    var descricao = dto.Descriptions?
                        .FirstOrDefault(d => d.Language.Name == "en")
                        ?.Description ?? "";

                    var response = new PokedexResponse
                    {
                        Nome = dto.Name,
                        Descricao = descricao,
                        Pokemons = new List<PokedexPokemonResponse>()
                    };

                    foreach (var entry in dto.Pokemon_Entries)
                    {
                        int pokemonId = int.Parse(
                            entry.Pokemon_Species.Url.TrimEnd('/').Split('/').Last()
                        );

                        var detalhe = await _pokemonApi.BuscarPokemon(pokemonId.ToString());
                        if (detalhe == null) continue;

                        response.Pokemons.Add(new PokedexPokemonResponse
                        {
                            NumeroRegional = entry.Entry_Number,
                            Numero = pokemonId,
                            Nome = char.ToUpper(entry.Pokemon_Species.Name[0]) +
                                   entry.Pokemon_Species.Name[1..],
                            Tipos = detalhe.Tipos,
                            Sprite = detalhe.Sprites
                        });
                    }

                    //FILTRO 
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        var term = search.Trim().ToLower();

                        response.Pokemons = response.Pokemons
                            .Where(p =>
                                p.Nome.ToLower().Contains(term) ||
                                p.Numero.ToString() == term ||
                                p.NumeroRegional.ToString() == term
                            )
                            .ToList();
                    }

                    return response;
                }
            );
        }


    }
}
