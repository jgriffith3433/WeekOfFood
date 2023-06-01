using ContainerNinja.Contracts.ViewModels;

namespace ContainerNinja.Contracts.Services
{
    public interface IChatAIService
    {
        Task<string> GetChatResponse(string message, int from, List<ChatMessageVM> previousMessages, string currentUrl);
        Task<string> GetTextFromSpeech(byte[] speechBytes);
    }
}
