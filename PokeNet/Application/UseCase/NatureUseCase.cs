using PokeNet.Application.DTO.External;
using PokeNet.Application.DTO.Response;
using System.Net.Http.Json;

namespace PokeNet.Application.UseCases
{
    public class NatureUseCase
    {
        private readonly HttpClient _http;

        public NatureUseCase(HttpClient http)
        {
            _http = http;
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
            var nature = await _http.GetFromJsonAsync<NatureDetailResponse>($"https://pokeapi.co/api/v2/nature/{nomeOuId.ToLower()}");


            if (nature == null)
                return null;

            return new NatureResponse
            {
                Nome = nature.Name,
                Aumenta = nature.Increased?.Name ?? "none",
                Diminui = nature.Decreased?.Name ?? "none"
            };
        }
    }
}
