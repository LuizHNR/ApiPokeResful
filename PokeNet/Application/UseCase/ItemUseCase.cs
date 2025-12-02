using PokeNet.Application.DTO.Response;
using PokeNet.Application.DTO.External;
using System.Net.Http.Json;

namespace PokeNet.Application.UseCases
{
    public class ItemUseCase
    {
        private readonly HttpClient _http;

        public ItemUseCase(HttpClient http)
        {
            _http = http;
        }


        public async Task<List<ItemResponse>> BuscarTodos()
        {
            var list = await _http.GetFromJsonAsync<ItemListResponse>("item?limit=100000&offset=0");

            if (list == null || list.Results == null)
                return new List<ItemResponse>();

            // Baixa tudo paralelo
            var tasks = list.Results.Select(async item =>
            {
                var detail = await _http.GetFromJsonAsync<ItemDetailResponse>(item.Url);
                if (detail == null) return null;

                var efeito = detail.EffectEntries
                    .FirstOrDefault(e => e.Language.Name == "en")
                    ?.ShortEffect ?? "";

                return new ItemResponse
                {
                    Nome = detail.Name,
                    Sprite = detail.Sprites.Default,
                    Efeito = efeito
                };
            });

            return (await Task.WhenAll(tasks))
                .Where(x => x != null)
                .ToList()!;
        }




        public async Task<ItemResponse?> BuscarItem(string nomeOuId)
        {
            var item = await _http.GetFromJsonAsync<ItemDetailResponse>($"https://pokeapi.co/api/v2/item/{nomeOuId.ToLower()}");

            if (item == null)
                return null;

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
    }
}
