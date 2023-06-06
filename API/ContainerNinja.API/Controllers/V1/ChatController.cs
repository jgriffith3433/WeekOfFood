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
        [ProducesResponseType(typeof(ChatResponseVM), (int)HttpStatusCode.Created)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<ChatResponseVM>> Create(GetChatResponseVM getChatResponse)
        {
            foreach (var chatMessage in getChatResponse.ChatMessages)
            {
                chatMessage.Received = true;
            }
            var chatConversation = await _mediator.Send(new GetChatConversation
            {
                ChatConversationId = getChatResponse.ChatConversationId,
            });
            var response = await _mediator.Send(new GetChatResponseQuery
            {
                ChatMessages = getChatResponse.ChatMessages,
                CurrentUrl = getChatResponse.CurrentUrl,
                SendToRole = getChatResponse.SendToRole,
                ChatConversation = chatConversation,
                CurrentSystemToAssistantChatCalls = 1,
            });
            if (response.Error)
            {
                return BadRequest(response);
            }
            else
            {
                return Ok(response);
            }
        }

        [MapToApiVersion("1.0")]
        [HttpPost("Normal")]
        [ProducesResponseType(typeof(ChatResponseVM), (int)HttpStatusCode.Created)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<ChatResponseVM>> CreateNormal(GetChatResponseVM getChatResponse)
        {
            foreach (var chatMessage in getChatResponse.ChatMessages)
            {
                chatMessage.Received = true;
            }
            var chatConversation = await _mediator.Send(new GetChatConversation
            {
                ChatConversationId = getChatResponse.ChatConversationId,
            });
            var response = await _mediator.Send(new GetNormalChatResponseQuery
            {
                ChatMessages = getChatResponse.ChatMessages,
                CurrentUrl = getChatResponse.CurrentUrl,
                SendToRole = getChatResponse.SendToRole,
                ChatConversation = chatConversation,
            });
            if (response.Error)
            {
                return BadRequest(response);
            }
            else
            {
                return Ok(response);
            }
        }

        [MapToApiVersion("1.0")]
        [HttpPost("Speech")]
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


        [MapToApiVersion("1.0")]
        [HttpGet("TextToSpeech")]
        [ProducesResponseType(typeof(FileResult), (int)HttpStatusCode.Created)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<FileResult> TextToSpeech([FromQuery] GetChatTextToSpeechQuery query)
        {
            var bytes = await _mediator.Send(query);

            return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, "text-to-speech.mp3");
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