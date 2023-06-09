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
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "delete_product" })]
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

            Product product;
            if (query.Count == 0)
            {
                var systemResponse = "Error: Could not find product by name: " + model.Command.Product;
                throw new ChatAIException(systemResponse);
            }
            else if (query.Count == 1)
            {
                if (query[0].Name.ToLower() == model.Command.Product.ToLower())
                {
                    //exact match
                    product = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Error: Could not find product by name '" + model.Command.Product + "'. Did you mean: " + query[0].Name + "?";
                    throw new ChatAIException(systemResponse);
                }
            }
            else
            {
                var exactMatch = query.FirstOrDefault(r => r.Name.ToLower() == model.Command.Product.ToLower());
                if (exactMatch != null)
                {
                    //exact match
                    product = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Error: Multiple records found: " + string.Join(", ", query.Select(r => r.Name));
                    throw new ChatAIException(systemResponse);
                }
            }
            if (product != null)
            {
                _repository.Products.Delete(query[0].Id);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return model.Response;
        }
    }
}