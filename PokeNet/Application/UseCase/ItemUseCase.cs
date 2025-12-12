using Microsoft.Extensions.Caching.Memory;
using PokeNet.Application.DTO.Response;
using PokeNet.Application.DTO.External;
using System.Net.Http.Json;

namespace PokeNet.Application.UseCases
{
    public class ItemUseCase
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;

        public ItemUseCase(IHttpClientFactory factory, IMemoryCache cache)
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

        private string ExtractIdFromUrl(string url)
        {
            return url.TrimEnd('/').Split('/').Last();
        }

        public async Task<List<ItemResponse>> BuscarTodos()
        {
            return await GetOrCreateAsync(
                "itens_todos_cache",
                TimeSpan.FromHours(24),
                async () =>
                {
                    var list = await _http.GetFromJsonAsync<ItemListResponse>("item?limit=100000&offset=0");

                    if (list == null || list.Results == null)
                        return new List<ItemResponse>();

                    var tasks = list.Results.Select(async item =>
                    {
                        var id = ExtractIdFromUrl(item.Url);
                        return await BuscarItem(id);
                    });

                    return (await Task.WhenAll(tasks))
                        .Where(x => x != null)
                        .ToList()!;
                }
            );
        }

        public async Task<ItemResponse?> BuscarItem(string nomeOuId)
        {
            return await GetOrCreateAsync(
                $"item_detail_{nomeOuId}",
                TimeSpan.FromHours(24),
                async () =>
                {
                    var item = await _http.GetFromJsonAsync<ItemDetailResponse>($"item/{nomeOuId}");
                    if (item == null) return null;

                    var efeito = item.EffectEntries
                        .FirstOrDefault(e => e.Language.Name == "en")
                        ?.ShortEffect ?? "";

                    return new ItemResponse
                    {
                        Nome = item.Name,
                        Sprite = item.Sprites.Default,
                        Efeito = efeito
                    };
                }
            );
        }
    }
}
