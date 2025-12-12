using PokeNet.Application.DTO.External;
using PokeNet.Application.DTO.Response;
using System.Net.Http.Json;

namespace PokeNet.Application.UseCases
{
    public class NatureUseCase
    {
        private readonly HttpClient _http;

        public NatureUseCase(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("PokeApi");
        }

        public async Task<List<NatureResponse>> BuscarTodas()
        {
            var list = await _http.GetFromJsonAsync<NatureListResponse>("nature?limit=100000&offset=0");

            if (list == null || list.Results == null)
                return new List<NatureResponse>();

            var tasks = list.Results.Select(async nature =>
            {
                var detail = await _http.GetFromJsonAsync<NatureDetailResponse>(nature.Url);
                if (detail == null) return null;

                return new NatureResponse
                {
                    Nome = detail.Name,
                    Aumenta = detail.Increased?.Name ?? "none",
                    Diminui = detail.Decreased?.Name ?? "none"
                };
            });

            return (await Task.WhenAll(tasks))
                .Where(x => x != null)
                .ToList()!;
        }

        public async Task<NatureResponse?> BuscarNature(string nomeOuId)
        {
            var detail = await _http.GetFromJsonAsync<NatureDetailResponse>($"nature/{nomeOuId.ToLower()}");

            if (detail == null)
                return null;

            return new NatureResponse
            {
                Nome = detail.Name,
                Aumenta = detail.Increased?.Name ?? "none",
                Diminui = detail.Decreased?.Name ?? "none"
            };
        }
    }
}
