using ContainerNinja.Contracts.ViewModels;

namespace ContainerNinja.Contracts.Services
{
    public interface IChatAIService
    {
        Task<string> GetChatResponse(List<ChatMessageVM> chatMessages, string currentUrl);
        Task<string> GetNormalChatResponse(List<ChatMessageVM> chatMessages);
        Task<string> GetTextFromSpeech(byte[] speechBytes, string? previousMessage);
    }
}
