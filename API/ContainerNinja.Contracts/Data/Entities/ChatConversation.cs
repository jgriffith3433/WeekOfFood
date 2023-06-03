namespace ContainerNinja.Contracts.Data.Entities
{
    public class ChatConversation : AuditableEntity
    {
        public string Content { get; set; }
        public string? Error { get; set; }
        public List<ChatCommand> ChatCommands { get; set;}
    }
}