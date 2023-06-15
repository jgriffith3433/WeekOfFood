using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;
using System.Text.Json;

namespace ContainerNinja.Contracts.Common
{
    public class ChatCommandSpecification : Attribute
    {
        public ChatCommandSpecification(string name, string? description, string? parametersSchema = null)
        {
            Name = name;
            Description = description;
            if (parametersSchema != null)
            {
                CreateParametersSchemaElementFromString(parametersSchema);
            }
        }

        public void CreateParametersSchemaFromType(Type type)
        {
            var settings = new JsonSchemaGeneratorSettings
            {
                FlattenInheritanceHierarchy = true,
                UseXmlDocumentation = false,
                ResolveExternalXmlDocumentation = false,
                SchemaNameGenerator = new SchemaNameGenerator(),
                GenerateExamples = false,
                GenerateAbstractSchemas = false,
                
                
            };
            var json = JsonSchema.FromType(type, settings).ToJson();
            CreateParametersSchemaElementFromString(json);
        }

        protected void CreateParametersSchemaElementFromString(string parameters)
        {
            ParametersSchema = JsonSerializer.Deserialize<JsonElement>(parameters, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                AllowTrailingCommas = true,
            });
        }


        public string Name { get; set; }
        public string? Description { get; set; }
        public JsonElement? ParametersSchema { get; set; }
    }

    public class SchemaNameGenerator : ISchemaNameGenerator
    {
        /// <summary>Generates the name of the JSON Schema.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The new name.</returns>
        public virtual string Generate(Type type)
        {
            return null;
        }

        private static string GetName(CachedType cType)
        {
            return null;
        }

        private static string GetNullableDisplayName(CachedType type, string actual)
        {
            return null;
        }
    }
}
