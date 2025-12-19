using Microsoft.Extensions.Caching.Memory;
using PokeNet.Application.DTO.Response;
using PokeNet.Application.Services;
using System.Net.Http.Json;

namespace PokeNet.Application.UseCases
{
    public class RegionUseCase
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;

        private readonly PokemonApiService _pokemonApi;

        public RegionUseCase(IHttpClientFactory factory, IMemoryCache cache, PokemonApiService pokemonApi)
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

        private string ExtractIdFromUrl(string url) =>
            url.TrimEnd('/').Split('/').Last();

        public async Task<RegionResponse?> Buscar(string idOuNome)
        {
            return await GetOrCreateAsync(
                $"region_{idOuNome}",
                TimeSpan.FromHours(24),
                async () =>
                {
                    // --- 1) REGIÃO ---
                    var regionUrl = $"region/{idOuNome}";
                    var region = await _http.GetFromJsonAsync<RegionDetailDto>(regionUrl);

                    if (region == null) return null;

                    var response = new RegionResponse
                    {
                        Nome = region.Name
                    };

                    // --- 2) CADA LOCATION DA REGIÃO ---
                    var rotaTasks = region.Locations.Select(async loc =>
                    {
                        var locId = ExtractIdFromUrl(loc.Url);
                        var locationDetail = await _http.GetFromJsonAsync<LocationDetailDto>($"location/{locId}");

                        if (locationDetail == null)
                            return null;

                        var rota = new RegionLocationResponse
                        {
                            Nome = locationDetail.Name
                        };

                        // --- 3) PEGAR LOCATION AREAS ---
                        foreach (var area in locationDetail.Areas)
                        {
                            var areaId = ExtractIdFromUrl(area.Url);

                            var areaDetail = await _http.GetFromJsonAsync<LocationAreaDetailDto>($"location-area/{areaId}");

                            if (areaDetail?.PokemonEncounters != null)
                            {
                                foreach (var enc in areaDetail.PokemonEncounters)
                                {
                                    var version = enc.VersionDetails.FirstOrDefault();
                                    var encounterDetail = version?.EncounterDetails.FirstOrDefault();

                                    var pokemonDetalhe = await _pokemonApi.BuscarPokemon(enc.Pokemon.Name);
                                    if (pokemonDetalhe == null) continue;

                                    rota.Encounters.Add(new RegionPokemonEncounterResponse
                                    {
                                        Pokemon = new RegionPokemonResponse
                                        {
                                            Numero = pokemonDetalhe.Numero,
                                            Nome = pokemonDetalhe.Nome,
                                            Tipos = pokemonDetalhe.Tipos,
                                            Sprite = pokemonDetalhe.Sprites
                                        },
                                        Rate = encounterDetail?.Chance ?? 0,
                                        BaseScore = version?.MaxChance ?? 0
                                    });

                                }
                            }
                        }

                        return rota;
                    });

                    var rotas = (await Task.WhenAll(rotaTasks))
                        .Where(r => r != null)
                        .ToList()!;

                    response.Rotas = rotas;

                    return response;
                });
        }
    }
}
