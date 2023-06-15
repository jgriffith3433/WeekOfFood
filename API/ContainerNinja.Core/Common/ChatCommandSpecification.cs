using System.Text.Json;

namespace ContainerNinja.Core.Common
{
    public class ChatCommandSpecification : Attribute
    {
        public ChatCommandSpecification(string name, string? description, string? parameters = null)
        {
            Name = name;
            Description = description;
            if (!string.IsNullOrEmpty(parameters))
            {
                Parameters = JsonSerializer.Deserialize<JsonElement>(parameters, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    AllowTrailingCommas = true,
                });
            }
        }

        public string Name { get; set; }
        public string? Description { get; set; }
        public JsonElement? Parameters { get; set; }
    }
}
