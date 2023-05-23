namespace ContainerNinja.Contracts.Data.Entities
{
    public class ChatCommand : AuditableEntity
    {
        public string CommandName { get; set; }
        public string? SystemResponse { get; set; }
        public string? Error { get; set; }
        public string RawReponse { get; set; }
        public string? CurrentUrl { get; set; }
        public bool Unknown { get; set; }
        public bool ChangedData { get; set; }
        public string? NavigateToPage { get; set; }
        public ChatConversation ChatConversation { get; set; }
    }
}