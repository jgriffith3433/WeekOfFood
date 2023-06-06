namespace ContainerNinja.Contracts.DTO.ChatAICommands
{
    public record ChatAICommandDTO
    {
        public string Cmd { get; set; }
        public string Response { get; set; }
    }
}
