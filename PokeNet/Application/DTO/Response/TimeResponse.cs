using PokeNet.Application.DTO.Response;
using PokeNet.Domain.Entities;

public class TimeResponse
{
    public string Nome { get; set; }
    public List<PokemonTimeResponse> Pokemons { get; set; }

    public static TimeResponse FromEntity(Time t, List<PokemonTimeResponse> pokemons)
    {
        return new TimeResponse
        {
            Nome = t.Nome,
            Pokemons = pokemons
        };
    }
}
