using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PokeNet.Application.DTO.Request;
using PokeNet.Application.UseCases;
using System;

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




        /// <summary>
        /// Busca um todos os itens.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null
            )
        {
            var filter = new ItemFilterRequest
            {
                Page = page,
                PageSize = pageSize,
                Search = search
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
