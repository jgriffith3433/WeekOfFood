using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using LinqKit;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "delete_product" })]
    public class ConsumeChatCommandDeleteProduct : IRequest<ChatResponseVM>, IChatCommandConsumer<ChatAICommandDTODeleteProduct>
    {
        public ChatAICommandDTODeleteProduct Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDeleteProductHandler : IRequestHandler<ConsumeChatCommandDeleteProduct, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDeleteProductHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommandDeleteProduct model, CancellationToken cancellationToken)
        {
            var predicate = PredicateBuilder.New<Product>();
            var searchTerms = string.Join(' ', model.Command.Product.ToLower().Split('-')).Split(' ');
            foreach (var searchTerm in searchTerms)
            {
                predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm));
            }
            var query = _repository.Products.Include<Product, ProductStock>(p => p.ProductStock)
                .AsNoTracking()
                .AsExpandable()
                .Where(predicate).ToList();

            if (query.Count == 0)
            {
                var systemResponse = "Error: Could not find product by name: " + model.Command.Product;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                if (query.Count == 1 && query[0].Name.ToLower() == model.Command.Product.ToLower())
                {
                    //exact match, go ahead and delete
                    _repository.Products.Delete(query[0].Id);
                }
                else
                {
                    //unsure, ask user
                    var productNames = query.Select(p => p.Name).ToList();
                    var systemResponse = "Which product are you referring to?\n" + string.Join(", ", productNames);
                    throw new ChatAIException(systemResponse);
                }
            }
            return model.Response;
        }
    }
}