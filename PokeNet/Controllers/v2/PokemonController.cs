using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PokeNet.Application.UseCase;

namespace PokeNet.Controllers.v2
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    public class PokemonController : ControllerBase
    {
        private readonly PokemonUseCase _useCase;

        public PokemonController(PokemonUseCase useCase)
        {
            _useCase = useCase;
        }


        /// <summary>
        /// Busca todos os Pokemons.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BuscarTodos([FromQuery] int page = 1,[FromQuery] int pageSize = 20)
        {
            var lista = await _useCase.BuscarTodos(page, pageSize);
            return Ok(lista);
        }


        /// <summary>
        /// Busca um Pokémon pelo nome ou número.
        /// </summary>
        [HttpGet("{nomeOuNumero}")]
        public async Task<IActionResult> BuscarPokemon(string nomeOuNumero)
        {
            var pokemon = await _useCase.BuscarPokemon(nomeOuNumero);

            if (pokemon == null)
                return NotFound("Pokémon não encontrado.");

            return Ok(pokemon);
        }
    }
}
