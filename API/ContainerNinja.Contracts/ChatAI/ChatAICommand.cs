

namespace ContainerNinja.Contracts.ChatAI
{
    public record ChatAICommand
    {
        public string Cmd { get; set; }
        public string Response { get; set; }
    }
}
