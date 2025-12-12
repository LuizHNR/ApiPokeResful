using Microsoft.Extensions.Caching.Memory;
using PokeNet.Application.DTO.External;
using PokeNet.Application.DTO.Response;
using System.Net.Http.Json;

namespace PokeNet.Application.UseCases
{
    public class VersionGroupUseCase
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;

        public VersionGroupUseCase(IHttpClientFactory factory, IMemoryCache cache)
        {
            _http = factory.CreateClient("PokeApi");
            _cache = cache;
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

        public async Task<List<VersionGroupResponse>> BuscarTodos()
        {
            return await GetOrCreateAsync(
                "version_group_all_cache",
                TimeSpan.FromHours(24),
                async () =>
                {
                    var list = await _http.GetFromJsonAsync<VersionGroupListResponse>("version-group?limit=2000");

                    if (list == null || list.Results == null)
                        return new List<VersionGroupResponse>();

                    var tasks = list.Results.Select(async vg =>
                    {
                        var id = ExtractIdFromUrl(vg.Url);
                        return await BuscarPorId(id);
                    });

                    return (await Task.WhenAll(tasks))
                        .Where(x => x != null)
                        .ToList()!;
                }
            );
        }

        public async Task<VersionGroupResponse?> BuscarPorId(string id)
        {
            return await GetOrCreateAsync(
                $"version_group_detail_{id}",
                TimeSpan.FromHours(24),
                async () =>
                {
                    var detail = await _http.GetFromJsonAsync<VersionGroupDetailResponse>($"version-group/{id}");

                    if (detail == null) return null;

                    return new VersionGroupResponse
                    {
                        Nome = detail.Name,
                        Geracao = detail.Generation?.Name ?? "",
                        Pokedexes = detail.Pokedexes.Select(p => p.Name).ToList(),
                        Regioes = detail.Regions.Select(r => r.Name).ToList(),
                        Versoes = detail.Versions.Select(v => v.Name).ToList()
                    };
                }
            );
        }
    }
}
