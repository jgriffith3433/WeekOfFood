using OpenAI.ObjectModels.RequestModels;

namespace ContainerNinja.Contracts.Services
{
    public interface IChatAIService
    {
        Task<string> GetChatResponse(List<ChatMessage> chatMessages, string currentUrl);
        Task<string> GetTextFromSpeech(byte[] speechBytes);
    }
}
