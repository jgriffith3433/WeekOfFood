using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ContainerNinja.API.Filters;
using ContainerNinja.Contracts.Constants;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Handlers.Commands;
using ContainerNinja.Core.Handlers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContainerNinja.Controllers.V1
{
    [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Admin}")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProductStocksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductStocksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [MapToApiVersion("1.0")]
        [HttpGet]
        [ProducesResponseType(typeof(GetAllProductStocksVM), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<IActionResult> Get()
        {
            var query = new GetAllProductStocksQuery();
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [MapToApiVersion("1.0")]
        [HttpPut]
        [ProducesResponseType(typeof(GetAllProductStocksVM), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<ProductStockDTO>> Update(int id, UpdateProductStockCommand command)
        {
            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpGet("GetProductStockDetails")]
        [ProducesResponseType(typeof(GetAllProductStocksVM), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<ProductStockDetailsDTO>> GetProductStockDetails([FromQuery] GetProductStockDetailsQuery query)
        {
            return await _mediator.Send(query);
        }

        [MapToApiVersion("1.0")]
        [HttpPut("UpdateProductStockDetails/{id}")]
        [ProducesResponseType(typeof(GetAllProductStocksVM), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<ProductStockDetailsDTO>> UpdateProductStockDetails(int id, UpdateProductStockDetailsCommand command)
        {
            if (command == null || id != command.Id)
            {
                return BadRequest();
            }

            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpPost]
        [ProducesResponseType(typeof(GetAllProductStocksVM), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> Create(CreateProductStockCommand command)
        {
            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(GetAllProductStocksVM), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> Delete(int id)
        {
            return await _mediator.Send(new DeleteProductStockCommand
            {
                Id = id
            });
        }
    }
}