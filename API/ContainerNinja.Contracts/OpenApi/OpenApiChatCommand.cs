

namespace ContainerNinja.Contracts.OpenApi
{
    public record OpenApiChatCommand
    {
        public string Cmd { get; set; }
        public string Response { get; set; }
    }
}
