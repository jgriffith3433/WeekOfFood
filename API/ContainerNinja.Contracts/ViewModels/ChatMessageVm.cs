
namespace ContainerNinja.Contracts.ViewModels
{
    public record ChatMessageVm
    {
        public int From { get; set; }
        public string Message { get; set; }
    }
}