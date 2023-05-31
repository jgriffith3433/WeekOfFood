using ContainerNinja.Contracts.ViewModels;

namespace ContainerNinja.Contracts.Services
{
    public interface IChatAIService
    {
        Task<string> GetChatResponse(string message, List<ChatMessageVM> previousMessages, string currentUrl);
        Task<string> GetChatResponseFromSystem(string message, List<ChatMessageVM> previousMessages, string currentUrl);
        Task<string> GetTextFromSpeech(byte[] speechBytes);
    }
}
