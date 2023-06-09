
namespace ContainerNinja.Contracts.ViewModels
{
    public record ChatMessageVM
    {
        public string From { get; set; }
        public string Content { get; set; }
        public string RawContent { get; set; }
        public string? To { get; set; }
        public bool? Received { get; set; }
    }
}