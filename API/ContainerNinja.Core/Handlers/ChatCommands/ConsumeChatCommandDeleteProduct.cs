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
    public class ConsumeChatCommandDeleteProduct : IRequest<string>, IChatCommandConsumer<ChatAICommandDTODeleteProduct>
    {
        public ChatAICommandDTODeleteProduct Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDeleteProductHandler : IRequestHandler<ConsumeChatCommandDeleteProduct, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDeleteProductHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandDeleteProduct model, CancellationToken cancellationToken)
        {
            var predicate = PredicateBuilder.New<Product>();
            var searchTerms = string.Join(' ', model.Command.ProductName.ToLower().Split('-')).Split(' ');
            foreach (var searchTerm in searchTerms)
            {
                predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm));
            }
            var query = _repository.Products.Set.AsExpandable().Where(predicate).ToList();

            Product product;
            if (query.Count == 0)
            {
                var systemResponse = "Could not find product by name: " + model.Command.ProductName;
                throw new ChatAIException(systemResponse);
            }
            else if (query.Count == 1)
            {
                if (query[0].Name.ToLower() == model.Command.ProductName.ToLower())
                {
                    //exact match
                    product = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Could not find product by name '" + model.Command.ProductName + "'. Did you mean: " + query[0].Name + "?";
                    throw new ChatAIException(systemResponse);
                }
            }
            else
            {
                var exactMatch = query.FirstOrDefault(r => r.Name.ToLower() == model.Command.ProductName.ToLower());
                if (exactMatch != null)
                {
                    //exact match
                    product = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Multiple records found: " + string.Join(", ", query.Select(r => r.Name));
                    throw new ChatAIException(systemResponse);
                }
            }
            if (product != null)
            {
                _repository.Products.Delete(query[0].Id);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return "Success";
        }
    }
}