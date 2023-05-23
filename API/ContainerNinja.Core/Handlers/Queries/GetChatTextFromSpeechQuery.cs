using AutoMapper;
using ContainerNinja.Contracts.Data;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.ViewModels;

namespace ContainerNinja.Core.Handlers.Queries
{
    public record GetChatTextFromSpeechQuery : IRequest<GetChatTextFromSpeechVm>
    {
        public byte[] Speech { get; set; }
    }

    public class GetChatTextFromSpeechQueryHandler : IRequestHandler<GetChatTextFromSpeechQuery, GetChatTextFromSpeechVm>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly IOpenApiService _openApiService;

        public GetChatTextFromSpeechQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache, IOpenApiService openApiService)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _openApiService = openApiService;
        }

        public async Task<GetChatTextFromSpeechVm> Handle(GetChatTextFromSpeechQuery request, CancellationToken cancellationToken)
        {
            var speechToTextMessage = await _openApiService.GetTextFromSpeech(request.Speech);

            return new GetChatTextFromSpeechVm
            {
                Text = speechToTextMessage
            };
        }
    }
}