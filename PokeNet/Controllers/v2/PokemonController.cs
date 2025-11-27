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
        /// <param name="page">Número da página (default = 1)</param>
        /// <param name="pageSize">Quantidade de itens por página (default = 20)</param>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> BuscarTodos([FromQuery] int page = 1,[FromQuery] int pageSize = 20)
        {
            var lista = await _useCase.BuscarTodos(page, pageSize);

            var result = lista.Select(p => new
            {
                p.Numero,
                p.Nome,
                p.Tipos,
                links = new
                {
                    self = Url.Action(nameof(BuscarPokemon), new { nomeOuNumero = p.Numero })
                }
            });

            return Ok(new
            {
                page,
                pageSize,
                totalItems = lista.Count(),
                items = result
            });
        }



        /// <summary>
        /// Busca um Pokémon pelo nome ou número.
        /// </summary>
        /// <param name="id">id do registro</param>
        [Authorize]
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
