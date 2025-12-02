using PokeNet.Application.DTO.External;

public class RegionUseCase
{
    private readonly HttpClient _http;

    public RegionUseCase(HttpClient http)
    {
        _http = http;
    }

    // ======================
    // GET ALL
    // ======================
    public async Task<List<RegionDetailResponse>> GetAllAsync()
    {
        var list = await _http.GetFromJsonAsync<RegionListResponse>(
            "region?limit=1000&offset=0"
        );

        var simpleList = new List<RegionDetailResponse>();

        foreach (var item in list.Results)
        {
            simpleList.Add(new RegionDetailResponse
            {
                Name = item.Name,
                LocationsComplete = new() // vazio no GET ALL
            });
        }

        return simpleList;
    }


    // ======================
    // GET BY ID OR NAME
    // ======================
    public async Task<RegionDetailResponse?> GetByIdOrNameAsync(string idOrName)
    {
        var region = await _http.GetFromJsonAsync<RegionDetailResponse>(
            $"region/{idOrName}"
        );

        var enrichedLocations = new List<RegionLocationCompleteDTO>();

        foreach (var loc in region.Locations)
        {
            var id = loc.Url.TrimEnd('/').Split('/').Last();

            var locDetails = await _http.GetFromJsonAsync<LocationDetailResponse>(
                $"location/{id}"
            );

            // Nome localizado (fallback inglês)
            string name =
                locDetails.Names.FirstOrDefault(x => x.Language.Name == "en")?.Name
                ?? loc.Name;

            // ======================
            // DESCRIÇÃO COM FALLBACK INTELIGENTE
            // ======================
            string description = GetFallbackDescription(locDetails);

            if (locDetails.Areas?.Any() == true)
            {
                var areaId = locDetails.Areas.First().Url.TrimEnd('/').Split('/').Last();

                var locArea = await _http.GetFromJsonAsync<LocationAreaDetailResponse>(
                    $"location-area/{areaId}"
                );

                var flavor =
                    locArea.Flavor_Text_Entries
                        .FirstOrDefault(f => f.Language.Name == "pt")?.Flavor_Text ??
                    locArea.Flavor_Text_Entries
                        .FirstOrDefault(f => f.Language.Name == "en")?.Flavor_Text;

                // Se realmente existir flavor text, substitui o fallback
                if (!string.IsNullOrWhiteSpace(flavor))
                    description = flavor;
            }

            enrichedLocations.Add(new RegionLocationCompleteDTO
            {
                Name = name,
                Description = description
            });
        }

        region.Locations = null!;
        region.LocationsComplete = enrichedLocations;

        return region;
    }



    // ======================
    // MÉTODO PRIVADO – FALLBACK DE DESCRIÇÃO MAIS BONITO
    // ======================
    private string GetFallbackDescription(LocationDetailResponse loc)
    {
        var enName = loc.Names
            .FirstOrDefault(n => n.Language.Name == "en")?.Name ?? loc.Id.ToString();

        return $"Local conhecido como '{enName}'. Nenhuma descrição oficial disponível.";
    }
}
