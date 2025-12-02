using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PokeNet.Application.UseCases;

namespace PokeNet.Api.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    public class RegionController : ControllerBase
    {
        private readonly RegionUseCase _useCase;

        public RegionController(RegionUseCase useCase)
        {
            _useCase = useCase;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _useCase.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{idOrName}")]
        public async Task<IActionResult> GetByIdOrName(string idOrName)
        {
            var result = await _useCase.GetByIdOrNameAsync(idOrName.ToLower());
            return Ok(result);
        }
    }
}
