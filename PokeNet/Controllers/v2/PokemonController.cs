using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeNet.Application.DTO.Request;
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
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null,
            [FromQuery] string? gen = null,
            [FromQuery] string? types = null,
            [FromQuery] string? order = null
        )
        {
            var filter = new PokemonFilterRequest
            {
                Page = page,
                PageSize = pageSize,
                Search = search,
                Order = order,
                Generations = gen?.Split(',').Select(int.Parse).ToList() ?? new(),
                Types = types?.Split(',').Select(t => t.ToLower()).ToList() ?? new()
            };

            var result = await _useCase.BuscarTodos(filter);

            return Ok(new
            {
                result.Page,
                result.PageSize,
                result.TotalItems,
                result.TotalPages,
                items = result.Items
            });
        }






        /// <summary>
        /// Busca um Pokémon pelo nome ou número.
        /// </summary>
        /// <param name="id">id do registro</param>
        [HttpGet("{nomeOuNumero}")]
        public async Task<IActionResult> GetPokemon(string nomeOuNumero)
        {
            var pokemon = await _useCase.BuscarPokemon(nomeOuNumero);

            if (pokemon == null)
                return NotFound("Pokémon não encontrado.");

            return Ok(pokemon);
        }



        /// <summary>
        /// Busca movimentos do pokemon.
        /// </summary>
        [HttpGet("{nomeOuNumero}/movimentos")]
        public async Task<IActionResult> GetMovimentos(
            string nomeOuNumero,
            [FromQuery] string? method = null,
            [FromQuery] string? types = null
        )
        {
            var filter = new MoveFilterRequest
            {
                Methods = method?.Split(',').Select(m => m.ToLower()).ToList() ?? new(),
                Types = types?.Split(',').Select(t => t.ToLower()).ToList() ?? new()
            };

            var result = await _useCase.BuscarMovimentos(nomeOuNumero, filter);

            if (result == null)
                return NotFound();

            return Ok(result);
        }


    }
}
