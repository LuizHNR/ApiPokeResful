using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PokeNet.Application.DTO.Request;
using PokeNet.Application.UseCase;

namespace PokeNet.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthUseCase _authUseCase;

        public AuthController(AuthUseCase authUseCase)
        {
            _authUseCase = authUseCase;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var response = await _authUseCase.LoginAsync(request);

            if (response == null)
                return Unauthorized("E-mail ou senha inválidos.");

            return Ok(response);
        }
    }
}
