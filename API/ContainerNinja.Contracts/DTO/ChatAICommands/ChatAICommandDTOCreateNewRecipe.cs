﻿using ContainerNinja.Contracts.Common;
using ContainerNinja.Contracts.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_new_recipe", "Create a new recipe and returns the RecipeId")]
public record ChatAICommandDTOCreateNewRecipe : ChatAICommandArgumentsDTO
{
    //[Description("When did they create the new recipe")]
    //public DateTime? When { get; set; }
}
