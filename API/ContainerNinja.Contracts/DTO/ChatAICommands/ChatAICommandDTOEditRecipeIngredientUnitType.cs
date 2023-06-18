﻿using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("edit_recipe_ingredient_unit_type", "Changes the unit type for the recipe's ingredient")]
public record ChatAICommandDTOEditRecipeIngredientUnitType : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Whether or not the user gave permission to change the unit type for the recipe's ingredient")]
    public bool? UserGavePermission { get; set; }
    [Required]
    [Description("Name of the recipe")]
    public string RecipeName { get; set; }
    [Required]
    [Description("Name of the ingredient")]
    public string IngredientName { get; set; }
    [Required]
    [Description("New unit type")]
    public string UnitType { get; set; }
}
