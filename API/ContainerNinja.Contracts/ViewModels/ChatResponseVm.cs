
namespace ContainerNinja.Contracts.ViewModels
{
    public record GetChatResponseVm
    {
        public int ChatConversationId { get; set; }
        public bool CreateNewChat { get; set; }
        public bool Error { get; set; }
        public bool Dirty { get; set; }
        public string NavigateToPage { get; set; }
        public List<ChatMessageVm> PreviousMessages { get; set; }
        public ChatMessageVm ResponseMessage { get; set; }
    }
}