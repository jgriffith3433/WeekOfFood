using System.Text.Json.Nodes;
using Namotion.Reflection;
using NJsonSchema.Generation;
using NJsonSchema;
using System.Text.Json;

namespace ContainerNinja.Contracts.Common
{
    public class ChatCommandSpecification : Attribute
    {
        public ChatCommandSpecification(string name, string? description)
        {
            Names = new string[]
            {
                name
            };
            Description = description;
        }

        public ChatCommandSpecification(string[] names, string? description)
        {
            Names = names;
            Description = description;
        }

        public dynamic? GetFunctionParametersFromType(Type type)
        {
            var settings = new JsonSchemaGeneratorSettings
            {
                DefaultEnumHandling = EnumHandling.String,
                FlattenInheritanceHierarchy = true,
                UseXmlDocumentation = false,
                ResolveExternalXmlDocumentation = false,
                SchemaNameGenerator = new SchemaNameGenerator(),
                GenerateExamples = false,
                GenerateAbstractSchemas = false,
                AllowReferencesWithProperties = false,
                AlwaysAllowAdditionalObjectProperties = false,
            };
            var jsonString = JsonSchema.FromType(type, settings).ToJson();
            var jsonObject = JsonSerializer.Deserialize<JsonObject>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                AllowTrailingCommas = true,
            });
            if (jsonObject["properties"] == null)
            {
                jsonObject.Add("properties", new JsonObject());
            }
            return jsonObject;
            //return JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions
            //{
            //    PropertyNameCaseInsensitive = true,
            //    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            //    AllowTrailingCommas = true,
            //});
        }

        public string[] Names { get; set; }
        public string? Description { get; set; }
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


/*using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;

namespace ContainerNinja.Contracts.Common
{
    public class ChatCommandSpecification : Attribute
    {
        public ChatCommandSpecification(string name, string? description)
        {
            Names = new string[]
            {
                name
            };
            Description = description;
        }

        public ChatCommandSpecification(string[] names, string? description)
        {
            Names = names;
            Description = description;
        }

        public void CreateFunctionParametersFromType(Type type)
        {
            var properties = type.GetProperties();

            var functionParameterPropertiesObject = new JsonObject();
            var functionParameterPropertiesRequiredListArray = new JsonArray();

            foreach (var property in properties)
            {
                var required = property.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(RequiredAttribute)) != null;
                var description = property.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(DescriptionAttribute));
                var typeName = property.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(JsonSchemaTypeName));
                //var enumList = property.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(JsonSchemaEnumList));

                var functionParameterPropertyObject = new JsonObject();
                {
                    functionParameterPropertyObject.Add("type", typeName.ToString());
                    functionParameterPropertyObject.Add("description", description.ToString());
                    //functionParameterPropertyObject.Add("enum", string.Join(", ", enumList));
                };
                if (required)
                {
                    functionParameterPropertiesRequiredListArray.Add(property.Name);
                }
                functionParameterPropertiesObject.Add(property.Name, functionParameterPropertyObject);
            }
            var functionParametersObject = new JsonObject();
            {
                functionParametersObject.Add("type", "object");
                functionParametersObject.Add("properties", functionParameterPropertiesObject);
                functionParametersObject.Add("required", functionParameterPropertiesRequiredListArray);
            };
        }

        public string[] Names { get; set; }
        public string? Description { get; set; }
        public JsonObject? FunctionParameters { get; set; }
    }
}
*/