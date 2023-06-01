using OpenAI.ObjectModels.RequestModels;

namespace ContainerNinja.Contracts.ViewModels
{
    public record ConsumeChatCommandResponseVM
    {
        public int ChatConversationId { get; set; }
        public bool CreateNewChat { get; set; }
        public bool Error { get; set; }
        public bool Dirty { get; set; }
        public string NavigateToPage { get; set; }
        public List<ChatMessage> ChatMessages { get; set; }
    }
}