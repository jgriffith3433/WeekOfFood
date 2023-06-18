﻿using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandDeleteRecipeValidator : AbstractValidator<ConsumeChatCommandDeleteRecipe>
    {
        public ConsumeChatCommandDeleteRecipeValidator()
        {
            RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("Ask user if you can run the command");
            RuleFor(v => v.Command.RecipeId).NotEmpty().WithMessage("RecipeId is required");
        }
    }
}
