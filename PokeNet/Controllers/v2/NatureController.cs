using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PokeNet.Application.UseCases;

namespace PokeNet.API.Controllers
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class NatureController : ControllerBase
    {
        private readonly NatureUseCase _useCase;

        public NatureController(NatureUseCase useCase)
        {
            _useCase = useCase;
        }


        [HttpGet]
        public async Task<IActionResult> BuscarTodas()
        {
            var natures = await _useCase.BuscarTodas();
            return Ok(natures);
        }




        /// <summary>
        /// Busca uma nature pelo nome ou ID.
        /// </summary>
        [HttpGet("{nomeOuId}")]
        public async Task<IActionResult> BuscarNature(string nomeOuId)
        {
            var natureza = await _useCase.BuscarNature(nomeOuId);

            if (natureza == null)
                return NotFound("Nature não encontrada.");

            return Ok(natureza);
        }
    }
}
