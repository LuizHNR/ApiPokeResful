using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PokeNet.Application.DTO.Request;
using PokeNet.Application.DTO.Response;
using PokeNet.Application.UseCase;
using PokeNet.Application.Validators;
using System;
using System.Net;

namespace PokeNet.Controllers.v2
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    [Tags("CRUD Usuario")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioUseCase _useCase;
        private readonly RequestUsuarioValidator _validationUsuario;

        // ILogger
        private readonly ILogger<UsuarioController> _logger;


        public UsuarioController(UsuarioUseCase useCase, RequestUsuarioValidator validationUsuario,
        ILogger<UsuarioController> logger)
        {
            _useCase = useCase;
            _validationUsuario = validationUsuario;
            _logger = logger;
        }



        /// <summary>
        /// Retorna todos os Usuarios.
        /// </summary>
        /// <param name="page">Número da página (default = 1)</param>
        /// <param name="pageSize">Quantidade de itens por página (default = 10)</param>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Iniciando busca de todos os usuarios...");

                var usuarios = await _useCase.GetAllUsuariosAsync(page, pageSize);

                _logger.LogInformation("Busca de usuários concluída. {count} registros encontrados.", usuarios.Count());

                var result = usuarios.Select(d => new
                {
                    d.Id,
                    d.Nome,
                    d.Email,
                    links = new
                    {
                        self = Url.Action(nameof(GetById), new { id = d.Id })
                    }
                });

                return Ok(new
                {
                    page,
                    pageSize,
                    totalItems = usuarios.Count(),
                    items = result
                });

            }
            catch (MongoException ex)
            {
                return BadRequest(new { erro = "Erro no MongoDB: " + ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado.");
                return StatusCode(500, new { erro = ex.Message });
            }


        }



        /// <summary>
        /// Retorna um Usuario pelo ID.
        /// </summary>
        /// <param name="id">id do registro</param>
        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UsuarioResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetById(string id)
        {

            try
            {
                _logger.LogInformation("Buscando usuario com id {id}", id);

                var usuario = await _useCase.GetByIdAsync(id);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario {id} não encontrado.", id);
                    return NotFound("Usuário não encontrado.");

                }

                _logger.LogInformation("Usuario {id} encontrado com sucesso.", id);
                return Ok(usuario);

            }
            catch (MongoException ex)
            {
                return BadRequest(new { erro = "Erro no MongoDB: " + ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado.");
                return StatusCode(500, new { erro = ex.Message });
            }
        }



        /// <summary>
        /// Cria um novo Usuario.
        /// </summary>
        /// <param name="request">Payload para criação</param>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(UsuarioResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> PostUsuario([FromBody] UsuarioRequest request)
        {

            try
            {
                // Valida entrada
                _validationUsuario.ValidateAndThrow(request);

                var created = await _useCase.CreateUsuarioAsync(request);

                return CreatedAtAction(nameof(GetById), new { id = created.Id, version = "2" }, created);

            }
            catch (ValidationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (MongoException ex)
            {
                return BadRequest(new { erro = "Erro no MongoDB: " + ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado.");
                return StatusCode(500, new { erro = ex.Message });
            }

        }




        /// <summary>
        /// Atualiza um Usuario existente.
        /// </summary>
        /// <param name="id">ID do registro</param>
        /// <param name="request">Payload para atualização</param>
        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PutUsuario(string id, [FromBody] UsuarioRequest request)
        {

            try
            {
                _logger.LogInformation("Atualizando usuario {id}", id);

                var updated = await _useCase.UpdateUsuarioAsync(id, request);

                if (updated == null)
                    return NotFound("Usuário não encontrado.");

                return Ok(updated);

            }
            catch (ValidationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (MongoException ex)
            {
                return BadRequest(new { erro = "Erro no MongoDB: " + ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado.");
                return StatusCode(500, new { erro = ex.Message });
            }

        }





        /// <summary>
        /// Deleta um Usuario existente.
        /// </summary>
        /// <param name="id">ID do registro</param>
        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteUsuario(string id)
        {

            try
            {
                var deleted = await _useCase.DeleteUsuarioAsync(id);
                if (!deleted)
                    return NotFound("Usuário não encontrado.");

                return Ok(new { mensagem = "Usuário deletado com sucesso." });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao deletar usuario.");
                return StatusCode(500, new { erro = ex.Message });
            }

        }
    }
}
