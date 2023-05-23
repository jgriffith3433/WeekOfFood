using ContainerNinja.Contracts.Constants;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Handlers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ContainerNinja.Controllers.V1
{
    [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Admin}")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ChatController(IMediator mediator, IWebHostEnvironment webHostEnvironment)
        {
            _mediator = mediator;
            _webHostEnvironment = webHostEnvironment;
        }

        [MapToApiVersion("1.0")]
        [HttpPost]
        [ProducesResponseType(typeof(GetChatResponseVm), (int)HttpStatusCode.Created)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<GetChatResponseVm>> Create(GetChatResponseQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [MapToApiVersion("1.0")]
        [HttpPost("speech")]
        [ProducesResponseType(typeof(GetChatTextFromSpeechVm), (int)HttpStatusCode.Created)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<GetChatTextFromSpeechVm>> Speech([FromForm] IFormFile speech)
        {
            var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            var filePath = Path.Combine(uploads, speech.FileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await speech.CopyToAsync(fileStream);
            }

            return await _mediator.Send(new GetChatTextFromSpeechQuery
            {
                Speech = ConvertToByteArrayContent(speech)
            });
        }

        private byte[] ConvertToByteArrayContent(IFormFile audofile)
        {
            byte[] data;

            using (var br = new BinaryReader(audofile.OpenReadStream()))
            {
                data = br.ReadBytes((int)audofile.OpenReadStream().Length);
            }
            return data;
        }
    }
}