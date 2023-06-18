﻿using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandAddCookedRecipeIngredientValidator : AbstractValidator<ConsumeChatCommandAddCookedRecipeIngredient>
    {
        public ConsumeChatCommandAddCookedRecipeIngredientValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.RecipeName).NotEmpty().WithMessage("RecipeName is required");
            RuleFor(v => v.Command.IngredientName).NotEmpty().WithMessage("IngredientName is required");
            RuleFor(v => v.Command.Units).NotEmpty().WithMessage("Units is required");
            RuleFor(v => v.Command.UnitType).NotEmpty().WithMessage("UnitType is required");
        }
    }
}
