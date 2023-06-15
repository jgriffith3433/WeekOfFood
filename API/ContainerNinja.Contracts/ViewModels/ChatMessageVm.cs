using System.Text.Json;

namespace ContainerNinja.Contracts.ViewModels
{
    public record ChatMessageVM
    {
        public string From { get; set; }
        public string? Name { get; set; }
        public string? Content { get; set; }
        public JsonElement? FunctionCall { get; set; }
        public string? To { get; set; }
        public bool Received { get; set; }
    }
}