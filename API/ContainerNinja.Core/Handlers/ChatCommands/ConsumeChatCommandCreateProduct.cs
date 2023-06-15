using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "create_product" })]
    public class ConsumeChatCommandCreateProduct : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOCreateProduct>
    {
        public ChatAICommandDTOCreateProduct Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandCreateProductHandler : IRequestHandler<ConsumeChatCommandCreateProduct, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandCreateProductHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandCreateProduct model, CancellationToken cancellationToken)
        {
            var productEntity = _repository.Products.CreateProxy();
            {
                productEntity.Name = model.Command.ProductName;
            };

            //always ensure a product stock record exists for each product
            var productStockEntity = _repository.ProductStocks.CreateProxy();
            {
                productStockEntity.Name = model.Command.ProductName;
                productStockEntity.Units = 1;
            };
            productStockEntity.Product = productEntity;
            _repository.ProductStocks.Add(productStockEntity);
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            return $"Successfully created product {model.Command.ProductName}";
        }
    }
}