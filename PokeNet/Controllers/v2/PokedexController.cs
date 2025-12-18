using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PokeNet.Application.UseCases;

namespace PokeNet.Api.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    public class PokedexController : ControllerBase
    {
        private readonly PokedexUseCase _useCase;

        public PokedexController(PokedexUseCase useCase)
        {
            _useCase = useCase;
        }



        [HttpGet("{idOuNome}")]
        public async Task<IActionResult> Get(string idOuNome, [FromQuery] string? search = null)
        {
            var result = await _useCase.Buscar(idOuNome, search);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

    }
}
