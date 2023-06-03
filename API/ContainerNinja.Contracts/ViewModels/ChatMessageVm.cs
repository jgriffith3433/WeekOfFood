
namespace ContainerNinja.Contracts.ViewModels
{
    public record ChatMessageVM
    {
        public string Role { get; set; }
        public string Content { get; set; }
        public string RawContent { get; set; }
        public string? Name { get; set; }
    }
}