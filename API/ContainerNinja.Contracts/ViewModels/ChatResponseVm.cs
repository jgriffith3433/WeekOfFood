
namespace ContainerNinja.Contracts.ViewModels
{
    public record GetChatResponseVM
    {
        public int ChatConversationId { get; set; }
        public bool CreateNewChat { get; set; }
        public bool Error { get; set; }
        public bool Dirty { get; set; }
        public string NavigateToPage { get; set; }
        public List<ChatMessageVM> PreviousMessages { get; set; }
        public ChatMessageVM ResponseMessage { get; set; }
    }
}