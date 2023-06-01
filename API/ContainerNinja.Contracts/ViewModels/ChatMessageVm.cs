
namespace ContainerNinja.Contracts.ViewModels
{
    public record ChatMessageVM
    {
        public int From { get; set; }
        public int To { get; set; }
        public string Message { get; set; }
    }
}