using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> GetAll([FromQuery] int page = 1,[FromQuery] int pageSize = 50)
        {
            var result = await _useCase.BuscarTodos(page, pageSize);

            return Ok(new
            {
                result.Page,
                result.PageSize,
                result.TotalItems,
                result.TotalPages,
                items = result.Items.Select(p => new
                {
                    p.Numero,
                    p.Nome,
                    p.Tipos,
                    p.Sprite,
                    links = new
                    {
                        self = Url.Action(nameof(GetPokemon), new { nomeOuNumero = p.Numero })
                    }
                })
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
        public async Task<IActionResult> GetMovimentos(string nomeOuNumero)
        {
            var result = await _useCase.BuscarMovimentos(nomeOuNumero);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

    }
}
