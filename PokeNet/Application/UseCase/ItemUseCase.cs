using Microsoft.Extensions.Caching.Memory;
using PokeNet.Application.DTO.External;
using PokeNet.Application.DTO.Request;
using PokeNet.Application.DTO.Response;
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





        public async Task<PagedResponse<ItemResponse>> BuscarTodos(ItemFilterRequest filter)
        {
            int page = filter.Page < 1 ? 1 : filter.Page;
            int pageSize = filter.PageSize is < 1 or > 100 ? 50 : filter.PageSize;

            var list = await GetOrCreateAsync(
                "itens_todos_cache",
                TimeSpan.FromHours(24),
                async () =>
                    await _http.GetFromJsonAsync<ItemListResponse>(
                        "item?limit=100000&offset=0"
                    )
            );

            if (list?.Results == null)
                return new PagedResponse<ItemResponse>();

            // Base
            var query = list.Results
                .Select(p => new ListItemFilter
                {
                    Item = p,
                    Id = int.Parse(ExtractIdFromUrl(p.Url))
                });

            // FILTRO
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();

                query = query.Where(x =>
                    x.Item.Name.Contains(search) ||
                    x.Id.ToString() == search
                );
            }

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // PAGINAÇÃO
            var paged = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Busca detalhes só da página
            var tasks = paged.Select(x =>
                BuscarItem(x.Id.ToString())
            );

            var items = (await Task.WhenAll(tasks))
                .Where(x => x != null)
                .ToList()!;

            return new PagedResponse<ItemResponse>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = items
            };
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
