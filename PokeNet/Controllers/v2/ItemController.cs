using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PokeNet.Application.UseCases;

namespace PokeNet.API.Controllers
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly ItemUseCase _useCase;

        public ItemController(ItemUseCase useCase)
        {
            _useCase = useCase;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var itens = await _useCase.BuscarTodos();
            return Ok(itens);
        }



        /// <summary>
        /// Busca um item pelo nome ou ID.
        /// </summary>
        [HttpGet("{nomeOuId}")]
        public async Task<IActionResult> GetItem(string nomeOuId)
        {
            var item = await _useCase.BuscarItem(nomeOuId);

            if (item == null)
                return NotFound("Item não encontrado.");

            return Ok(item);
        }
    }
}
