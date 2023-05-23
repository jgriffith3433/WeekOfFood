using ContainerNinja.API.Filters;
using ContainerNinja.Contracts.Constants;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Handlers.Commands;
using ContainerNinja.Core.Handlers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ContainerNinja.API.Controllers.V1
{
    [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Admin}")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TodoListController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TodoListController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [MapToApiVersion("1.0")]
        [HttpPost]
        [ProducesResponseType(typeof(TodoListDTO), (int)HttpStatusCode.Created)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<IActionResult> Post([FromBody] CreateOrUpdateTodoListDTO model)
        {
            try
            {
                var command = new CreateTodoListCommand(model);
                var response = await _mediator.Send(command);
                return StatusCode((int)HttpStatusCode.Created, response);
            }
            catch (InvalidRequestBodyException ex)
            {
                return BadRequest(new BaseResponseDTO
                {
                    IsSuccess = false,
                    Errors = ex.Errors
                });
            }
        }

        [MapToApiVersion("1.0")]
        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteTodoListCommand(id);
            await _mediator.Send(command);
            return Ok();
        }

        [MapToApiVersion("1.0")]
        [HttpPut]
        [Route("{id}")]
        [TypeFilter(typeof(ETagFilter))]
        [ProducesResponseType(typeof(CreateOrUpdateTodoListDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<IActionResult> Update(int id, [FromBody] CreateOrUpdateTodoListDTO model)
        {
            try
            {
                var command = new UpdateTodoListCommand(id, model);
                var response = await _mediator.Send(command);
                return Ok(response);
            }
            catch (InvalidRequestBodyException ex)
            {
                return BadRequest(new BaseResponseDTO
                {
                    IsSuccess = false,
                    Errors = ex.Errors
                });
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new BaseResponseDTO
                {
                    IsSuccess = false,
                    Errors = new string[] { ex.Message }
                });
            }
        }

        [MapToApiVersion("1.0")]
        [HttpGet]
        [Route("{id}")]
        [TypeFilter(typeof(ETagFilter))]
        [ProducesResponseType(typeof(TodoListDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var query = new GetTodoListByIdQuery(id);
                var response = await _mediator.Send(query);
                return Ok(response);
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new BaseResponseDTO
                {
                    IsSuccess = false,
                    Errors = new string[] { ex.Message }
                });
            }
        }
    }
}
