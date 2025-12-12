using System.Text.Json.Serialization;

public class RegionDetailDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("locations")]
    public List<NamedAPIResource> Locations { get; set; }
}

public class LocationDetailDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("areas")]
    public List<NamedAPIResource> Areas { get; set; }
}

public class LocationAreaDetailDto
{
    [JsonPropertyName("pokemon_encounters")]
    public List<LocationAreaPokemonEncounterDto> PokemonEncounters { get; set; }
}

public class LocationAreaPokemonEncounterDto
{
    [JsonPropertyName("pokemon")]
    public NamedAPIResource Pokemon { get; set; }

    [JsonPropertyName("version_details")]
    public List<LocationAreaVersionDetailDto> VersionDetails { get; set; }
}

public class LocationAreaVersionDetailDto
{
    [JsonPropertyName("max_chance")]
    public int MaxChance { get; set; }

    [JsonPropertyName("encounter_details")]
    public List<LocationAreaEncounterDetailDto> EncounterDetails { get; set; }
}

public class LocationAreaEncounterDetailDto
{
    [JsonPropertyName("chance")]
    public int Chance { get; set; }
}

public class NamedAPIResource
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}
