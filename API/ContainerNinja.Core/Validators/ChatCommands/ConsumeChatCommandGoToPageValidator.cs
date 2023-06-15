﻿using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using System.Linq;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandGoToPageValidator : AbstractValidator<ConsumeChatCommandGoToPage>
    {
        private string[] m_Pages = new string[]
        {
            "home",
            "todo",
            "product stocks",
            "products",
            "completed orders",
            "recipes",
            "logged recipes",
            "called ingredients"
        };

        public ConsumeChatCommandGoToPageValidator()
        {
            RuleFor(v => v.Command.Page)
                .NotEmpty().WithMessage("Page is required.").
                Must(page => page != null && m_Pages.Contains(page.ToLower())).WithMessage(v => "Unknown page " + v.Command.Page + ". The available pages are: " + string.Join(", ", m_Pages) + ".");
        }
    }
}
