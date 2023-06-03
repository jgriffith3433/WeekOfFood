using ContainerNinja.Contracts.ViewModels;

namespace ContainerNinja.Contracts.Services
{
    public interface IChatAIService
    {
        Task<string> GetChatResponse(List<ChatMessageVM> chatMessages, string currentUrl);
        Task<string> GetTextFromSpeech(byte[] speechBytes);
    }
}
