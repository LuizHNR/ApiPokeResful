using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PokeNet.Application.UseCases;

namespace PokeNet.API.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class VersionGroupController : ControllerBase
    {
        private readonly VersionGroupUseCase _useCase;

        public VersionGroupController(VersionGroupUseCase useCase)
        {
            _useCase = useCase;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _useCase.BuscarTodos();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetId(string id)
        {
            var result = await _useCase.BuscarPorId(id);

            if (result == null)
                return NotFound("Version Group não encontrado.");

            return Ok(result);
        }
    }
}
