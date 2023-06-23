using ContainerNinja.Contracts.Common;
using ContainerNinja.Contracts.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_new_kitchen_inventory", "Creates a new kitchen inventory record")]
public record ChatAICommandDTOCreateNewKitchenInventory : ChatAICommandArgumentsDTO
{
}