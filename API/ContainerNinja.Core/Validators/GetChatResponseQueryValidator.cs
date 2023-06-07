using ContainerNinja.Core.Handlers.Commands;
using ContainerNinja.Core.Handlers.Queries;
using FluentValidation;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Validators
{
    public class GetChatResponseQueryValidator : AbstractValidator<GetChatResponseQuery>
    {
        public GetChatResponseQueryValidator()
        {
            RuleFor(v => v.ChatMessages)
                .NotEmpty().WithMessage("ChatMessages is required");

            RuleFor(v => v.ChatConversation)
                .NotEmpty().WithMessage("ChatConversation is required");

            RuleFor(v => v.SendToRole)
                .Must(v =>
                v == StaticValues.ChatMessageRoles.Assistant ||
                v == StaticValues.ChatMessageRoles.User ||
                v == StaticValues.ChatMessageRoles.System)
                .WithMessage($"SendToRole must be {StaticValues.ChatMessageRoles.Assistant}, {StaticValues.ChatMessageRoles.User}, or {StaticValues.ChatMessageRoles.System}");

            RuleFor(v => v.CurrentUrl)
                .NotEmpty().WithMessage("CurrentUrl is required");

            RuleFor(v => v.CurrentSystemToAssistantChatCalls)
                .GreaterThan(0).WithMessage("CurrentSystemToAssistantChatCalls must be greater than 0.")
                .LessThan(7).WithMessage("CurrentSystemToAssistantChatCalls must be less than 7.");

            /*
            0 = unknown cmd: edit-recipe-ingredient
            1 = Okay, I have updated your recipe.
            2 = unknown cmd: edit-recipe-ingredient <<----- Loop start
            */
            RuleFor(v => v.ChatMessages)
                .Must(cm =>
                {
                    if (cm.Count >= 3 && cm[cm.Count - 3].RawContent == cm[cm.Count - 1].RawContent)
                    {
                        if (cm[cm.Count - 3].Role == StaticValues.ChatMessageRoles.System)
                        {
                            //Loop detected
                            return false;
                        }
                    }
                    return true;
                }).WithMessage("Could not negotiate a command");
        }
    }
}
