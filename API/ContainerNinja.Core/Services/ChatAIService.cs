using ContainerNinja.Contracts.Services;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using OpenAI;
using OpenAI.Interfaces;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using System.Reflection;
using System.Text.Json;

namespace ContainerNinja.Core.Services
{
    public class ChatAIService : IChatAIService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IOpenAIService _openAIService;
        private IList<ChatFunction> _functionSpecifications;

        public ChatAIService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _openAIService = new OpenAIService(new OpenAiOptions()
            {
                Organization = Environment.GetEnvironmentVariable("OpenAIServiceOrganization"),
                ApiKey = Environment.GetEnvironmentVariable("OpenAIServiceApiKey"),
            });
            _functionSpecifications = GetChatCommandSpecifications();
        }

        public static IList<ChatFunction> GetChatCommandSpecifications()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.GetCustomAttribute<ChatCommandSpecification>() != null).Select(t => t.GetCustomAttribute<ChatCommandSpecification>()).Select(ccs => new ChatFunction(ccs.Name, ccs.Description, ccs.Parameters)).ToList();
        }

        public async Task<ChatMessageVM> GetChatResponse(List<ChatMessageVM> chatMessages, string functionCall)
        {
            var chatCompletionCreateRequest = CreateChatCompletionCreateRequest();
            chatCompletionCreateRequest.FunctionCall = functionCall;
            chatMessages.ForEach(cm => chatCompletionCreateRequest.Messages.Add(new ChatMessage(cm.From, cm.Content, cm.Name)));
            var completionResult = await _openAIService.ChatCompletion.CreateCompletion(chatCompletionCreateRequest);
            if (completionResult.Error != null)
            {
                throw new Exception(completionResult.Error.Message);
            }
            else
            {
                var chatResponse = completionResult.Choices.First().Message;

                return new ChatMessageVM
                {
                    Content = chatResponse.Content,
                    From = chatResponse.Role,
                    To = GetToFromChatResponse(chatResponse),
                    Name = chatResponse.Name,
                    FunctionCall = chatResponse.FunctionCall
                };
            }
        }

        private string GetToFromChatResponse(ChatMessage chatMessage)
        {
            if (IsString(chatMessage.FunctionCall))
            {
                return StaticValues.ChatMessageRoles.Function;
            }
            else
            {
                if (IsDefined(chatMessage.FunctionCall))
                {
                    return StaticValues.ChatMessageRoles.Function;
                }
                else
                {
                    return StaticValues.ChatMessageRoles.User;
                }
            }
        }

        public bool IsDefined(JsonElement? jsonElement)
        {
            return jsonElement.HasValue && jsonElement.Value.ValueKind != JsonValueKind.Null && jsonElement.Value.ValueKind != JsonValueKind.Undefined;
        }

        public bool IsString(JsonElement? jsonElement)
        {
            return jsonElement.HasValue && jsonElement.Value.ValueKind == JsonValueKind.String;
        }

        public async Task<ChatMessageVM> GetNormalChatResponse(List<ChatMessageVM> chatMessages)
        {
            var chatCompletionCreateRequest = CreateNormalChatCompletionCreateRequest();
            chatMessages.ForEach(cm => chatCompletionCreateRequest.Messages.Add(new ChatMessage(cm.From, cm.Content, cm.Name)));
            var completionResult = await _openAIService.ChatCompletion.CreateCompletion(chatCompletionCreateRequest);

            if (completionResult.Error != null)
            {
                throw new Exception(completionResult.Error.Message);
            }
            else
            {
                var chatResponse = completionResult.Choices.First().Message;

                return new ChatMessageVM
                {
                    Content = chatResponse.Content,
                    From = chatResponse.Role,
                    To = GetToFromChatResponse(chatResponse),
                    Name = chatResponse.Name,
                    FunctionCall = chatResponse.FunctionCall
                };
            }
        }

        private ChatCompletionCreateRequest CreateChatCompletionCreateRequest()
        {
            var chatCompletionCreateRequest = new ChatCompletionCreateRequest
            {
                Messages = GetChatPrompt(),
                Model = Models.ChatGpt3_5Turbo0613,
                Functions = _functionSpecifications,
                FunctionCall = "auto",
                Temperature = 0.8f,
                //MaxTokens = 400,
                //FrequencyPenalty = _1,
                //PresencePenalty = _1,
                //TopP = 0.1f
            };
            return chatCompletionCreateRequest;
        }

        private ChatCompletionCreateRequest CreateNormalChatCompletionCreateRequest()
        {
            var chatCompletionCreateRequest = new ChatCompletionCreateRequest
            {
                Messages = GetNormalChatPrompt(),
                Model = Models.ChatGpt3_5Turbo,
                //MaxTokens = 400,
                //FrequencyPenalty = _1,
                //PresencePenalty = _1,
                Temperature = 0.1f
            };
            return chatCompletionCreateRequest;
        }

        private string[] GetCommandsForCurrentUrl(string currentUrl)
        {
            switch (currentUrl)
            {

                case "recipes":
                    return new string[]
                    {
                    "none",
                    "go_to_page",
                    "edit_recipe_name",
                    "add_recipe_ingredient",
                    "create_recipe",
                    "delete_recipe",
                    "edit_recipe_ingredient_unittype",
                    "add_recipe_ingredient",
                    "substitute_recipe_ingredient"
                    };
                case "cooked_recipes":
                    return new string[]
                    {
                    "none",
                    "go_to_page",
                    "add_cooked_recipe_ingredient",
                    "create_cooked_recipe",
                    "substitute_cooked_recipe_ingredient",
                    "edit_cooked_recipe_ingredient_unittype",
                    "delete_cooked_recipe"
                    };
                case "products":
                    return new string[]
                    {
                    "none",
                    "go_to_page",
                    "edit_product_unit_type",
                    "create_product",
                    "delete_product"
                    };
                case "home":
                default:
                    return new string[]
                    {
                    "none",
                    "go_to_page"
                    };
            }
        }

        private string[] GetAllCommands()
        {
            return new string[] {
                "go_to_page",
                "order",
                "edit_recipe_name",
                "substitute_recipe_ingredient",
                "substitute_cooked_recipe_ingredient",
                "add_recipe_ingredient",
                "add_cooked_recipe_ingredient",
                "remove_recipe_ingredient",
                "remove_cooked_recipe_ingredient",
                "edit_recipe_ingredient_unittype",
                "edit_cooked_recipe_ingredient_unittype",
                "edit_product_unit_type",
                "create_product",
                "delete_product",
                "create_recipe",
                "delete_recipe",
                "create_cooked_recipe",
                "none",
                "delete_cooked_recipe",
            };
        }

        private string[] GetPages()
        {
            return new string[]
            {
                "home",
                "todo",
                "product stocks",
                "products",
                "completed orders",
                "recipes",
                "logged recipes",
                "called ingredients"
            };
        }

        private List<ChatMessage> GetChatPrompt()
        {
            var chatPromptList = new List<ChatMessage>
            {
                ChatMessage.FromSystem(@"You are a kitchen assistant. You help the user log meals, create/modify/delete logged meals, recipes, and ingredients. You also help the user in using the website.", StaticValues.ChatMessageRoles.System),
                ChatMessage.FromUser(@"I ate a sandwich for lunch.", StaticValues.ChatMessageRoles.User),
//                ChatMessage.FromAssistant(@"Okay", StaticValues.ChatMessageRoles.Assistant, JsonSerializer.Deserialize<JsonElement>(
//@"{
//	""name"": ""search_recipes"",
//	""arguments"": ""{\""search\"": \""sandwich\""}""
//}")
//                ),
                ChatMessage.FromFunction(@"Results: ham sandwich, turkey sandwich, salami sandwich", StaticValues.ChatMessageRoles.Function),
                ChatMessage.FromAssistant(@"Did you have a ham, turkey, or salami sandwich?", StaticValues.ChatMessageRoles.Assistant),
                ChatMessage.FromUser(@"It was a ham sandwich.", StaticValues.ChatMessageRoles.User),
//                ChatMessage.FromAssistant(@"Okay", StaticValues.ChatMessageRoles.Assistant, JsonSerializer.Deserialize<JsonElement>(
//@"{
//	""name"": ""log_recipe"",
//	""arguments"": ""{\""recipe\"": \""ham sandwich\""}""
//}")
//                ),
                ChatMessage.FromFunction(@"Created logged recipe: ham sandwich", StaticValues.ChatMessageRoles.Function),
                ChatMessage.FromAssistant(@"Okay I have successfully logged your ham sandwich.", StaticValues.ChatMessageRoles.Assistant),
                ChatMessage.FromUser(@"I didn't use any mayo.", StaticValues.ChatMessageRoles.User),
//                ChatMessage.FromAssistant(@"Okay", StaticValues.ChatMessageRoles.Assistant, JsonSerializer.Deserialize<JsonElement>(
//@"{
//	""name"": ""remove_logged_recipe_ingredient"",
//	""arguments"": ""{\""recipename\"": \""ham sandwich\"", \""ingredientname\"": \""mayo\""}""
//}")
//                ),
                ChatMessage.FromFunction(@"Removed mayo from logged recipe ham sandwich", StaticValues.ChatMessageRoles.Function),
                ChatMessage.FromAssistant(@"Okay I have successfully removed mayo from your ham sandwich log.", StaticValues.ChatMessageRoles.Assistant),
            };
            return chatPromptList;
        }

//        private List<ChatMessage> GetChatPromptForCurrentUrl(string currentUrl)
//        {
//            var chatPromptList = new List<ChatMessage>
//                {
//                    ChatMessage.FromSystem(
//                        "You are a kitchen assistant. You must respond only with JSON and no extra text. The cmd and response fields are required. " +
//                        "The user is on the home page. The available commands you can return are: " + GetAllCommands() + ".",
//                        StaticValues.ChatMessageRoles.System
//                        ),
//                    ChatMessage.FromUser("Hello", StaticValues.ChatMessageRoles.User),
//                    ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""none"",
//    ""response"": ""Hello, how can I help you manage your kitchen?"",
//}",
//                        StaticValues.ChatMessageRoles.Assistant
//                        ),
//                        ChatMessage.FromUser("Go to the forum", StaticValues.ChatMessageRoles.User),
//                        ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""go_to_page"",
//    ""page"": ""forum"",
//}",
//                        StaticValues.ChatMessageRoles.Assistant),
//                        ChatMessage.FromSystem("Unknown page forum. The available pages are:" + string.Join(", ", GetPages()), StaticValues.ChatMessageRoles.User),
//                        ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""none"",
//    ""response"": ""I'm sorry, I cannot navigate to the forum page. The available pages are: " + string.Join(", ", GetPages()) + @"."",
//}",
//                        StaticValues.ChatMessageRoles.Assistant),
//                };
//            if (currentUrl != "home")
//            {
//                chatPromptList.AddRange(new List<ChatMessage>
//                        {
//                        ChatMessage.FromUser("Go to " + currentUrl, StaticValues.ChatMessageRoles.User),
//                        ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""go_to_page"",
//    ""response"": ""Okay, navigating to " + currentUrl + @""",
//    ""page"": """ + currentUrl + @""",
//}",
//                        StaticValues.ChatMessageRoles.Assistant
//                        ),
//                        ChatMessage.FromSystem("Success", StaticValues.ChatMessageRoles.System),
//                        ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""none"",
//    ""response"": ""Okay, navigating to " + currentUrl + @""",
//}",
//                        StaticValues.ChatMessageRoles.Assistant
//                        ),
//                    });
//            }
//            switch (currentUrl)
//            {
//                case "recipes":
//                    chatPromptList.AddRange(new List<ChatMessage>
//                        {
//                            ChatMessage.FromUser("Change the bienenstich recipe to bienenstich 1", StaticValues.ChatMessageRoles.User),
//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""edit_recipe_name"",
//    ""original"": ""bienenstich"",
//    ""new"": ""yummy""
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromSystem("Multiple records found: bienenstich_1, bienenstich_2", StaticValues.ChatMessageRoles.System),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""none"",
//    ""response"": ""Do you want to change bienenstich_1 or bienenstich_2?"",
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromUser("bienenstich_1", StaticValues.ChatMessageRoles.User),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""edit_recipe_name"",
//    ""original"": ""bienenstich_1"",
//    ""new"": ""bienenstich 1""
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromSystem("Success", StaticValues.ChatMessageRoles.System),
                            
//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""none"",
//    ""response"": ""Okay, I have changed bienenstich_1 to bienenstich."",
//}",
//                                StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromUser("Can you substitute the bread for gluten-free bread in the ham sandwich recipe?", StaticValues.ChatMessageRoles.User),
//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""edit_recipe_ingredient"",
//    ""recipe"": ""ham sandwich"",
//    ""original"": ""bread"",
//    ""new"": ""gluten-free bread""
//}",
//                                StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromSystem("Success", StaticValues.ChatMessageRoles.System),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""none"",
//    ""response"": ""Okay, I have substituted the bread ingredient for Gluten-free bread."",
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromUser("Give me a barbeque chicken recipe.", StaticValues.ChatMessageRoles.User),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""none"",
//    ""response"": ""Sure. Do you want it sweet or savory?"",
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromUser("Both.", StaticValues.ChatMessageRoles.User),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""create_recipe"",
//    ""name"": ""Honey Barbeque Chicken Thighs"",
//    ""serves"": 2,
//    ""ingredients"": [
//        {
//            ""name"": ""Boneless chicken thighs"",
//            ""units"": 3,
//            ""unittype"": ""pounds""
//        },
//        {
//            ""name"": ""BBQ sauce"",
//            ""units"": 1,
//            ""unittype"": ""bottle""
//        },
//        {
//            ""name"": ""Honey"",
//            ""units"": 2,
//            ""unittype"": ""tablespoons""
//        },
//        {
//            ""name"": ""Soy sauce"",
//            ""units"": 2,
//            ""unittype"": ""tablespoons""
//        },
//    ]
//}",
//                                StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromSystem("Success", StaticValues.ChatMessageRoles.System),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""none"",
//    ""response"": ""Created."",
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromUser("I don't really like honey bbq.", StaticValues.ChatMessageRoles.User),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""none"",
//    ""response"": ""Would you like to use brown sugar instead?."",
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromUser("Sure.", StaticValues.ChatMessageRoles.User),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""edit_recipe_ingredient"",
//    ""recipe"": ""Honey Barbeque Chicken Thighs"",
//    ""original"": ""Honey"",
//    ""new"": ""Brown sugar"",
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromSystem("Success", StaticValues.ChatMessageRoles.System),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""edit_recipe_name"",
//    ""original"": ""Honey Barbeque Chicken Thighs"",
//    ""new"": ""Brown Sugar BBQ Chicken Thighs""
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromSystem("Success", StaticValues.ChatMessageRoles.System),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""none"",
//    ""response"": ""Okay, I substituted honey for brown sugar and renamed the recipe to Brown Sugar BBQ Chicken Thighs."",
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromUser("Remove the spaghetti with meat sauce recipe", StaticValues.ChatMessageRoles.User),
//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""delete_recipe"",
//    ""recipe"": ""spaghetti with meat sauce"",
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromSystem("Success", StaticValues.ChatMessageRoles.System),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""none"",
//    ""response"": ""Removed."",
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromUser("Change the butter unit type to tablespoons in the eggs and spinach recipe", StaticValues.ChatMessageRoles.User),
//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""edit_recipe_ingredient_unittype"",
//    ""recipe"": ""eggs and spinach"",
//    ""ingredient"": ""butter"",
//    ""unittype"": ""tablespoons"",
//}",
//                            StaticValues.ChatMessageRoles.Assistant),


//                            ChatMessage.FromSystem("Success: Unit type changed from cups to tablespoons.", StaticValues.ChatMessageRoles.System),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""none"",
//    ""response"": ""Okay I changed it from cupts to tablespoons."",
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromUser("Add two teaspoons of pepper to the spicy chili recipe", StaticValues.ChatMessageRoles.User),
//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""add_recipe_ingredient"",
//    ""recipe"": ""spicy chili"",
//    ""ingredient"": ""pepper"",
//    ""unittype"": ""teaspoons"",
//    ""units"": 2
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromSystem("Success", StaticValues.ChatMessageRoles.System),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""none"",
//    ""response"": ""Added."",
//}",
//                            StaticValues.ChatMessageRoles.Assistant),


//                            ChatMessage.FromUser("Remove the onions from the spicy chili recipe", StaticValues.ChatMessageRoles.User),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""remove_recipe_ingredient"",
//    ""recipe"": ""spicy chili"",
//    ""ingredient"": ""onions"",
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromSystem("No ingredient 'onions' found on recipe 'spicy chili'. The ingredients are: ground beef, white onion, garlic, bell pepper, diced tomatoes, kidney beans, black beans, tomato paste, chili powder, cumin, paprika, cayenne pepper, salt, pepper", StaticValues.ChatMessageRoles.System),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""remove_recipe_ingredient"",
//    ""recipe"": ""spicy chili"",
//    ""ingredient"": ""white onion"",
//}",
//                            StaticValues.ChatMessageRoles.Assistant),

//                            ChatMessage.FromSystem("Success", StaticValues.ChatMessageRoles.System),

//                            ChatMessage.FromAssistant(
//@"{
//    ""cmd"": ""none"",
//    ""response"": ""Removed."",
//}",
//                            StaticValues.ChatMessageRoles.Assistant),
//                        });
//                    break;
//                case "cooked_recipes":
//                    chatPromptList.AddRange(new List<ChatMessage>
//                            {
//                                ChatMessage.FromUser("I added two teaspoons of salt to the bienenstich recipe.", StaticValues.ChatMessageRoles.User),
//                                ChatMessage.FromAssistant(
//        @"{
//          ""cmd"": ""add_cooked_recipe_ingredient"",
//          ""response"": ""Okay, I have added salt to the cooked recipe."",
//          ""recipe"": ""bienenstich"",
//          ""name"": ""salt"",
//          ""units"": 2,
//          ""unittype"": ""teaspoon""
//        }",
//                                StaticValues.ChatMessageRoles.Assistant
//                            ),
//                                ChatMessage.FromUser("Add syrup to oatmeal", StaticValues.ChatMessageRoles.User),
//                                ChatMessage.FromAssistant(
//        @"{
//          ""cmd"": ""add_cooked_recipe_ingredient"",
//          ""response"": ""Okay, I have added syrup to the oatmeal recipe."",
//          ""recipe"": ""oatmeal"",
//          ""name"": ""syrup"",
//          ""units"": 1,
//          ""unittype"": ""tablespoon""
//        }",
//                                StaticValues.ChatMessageRoles.Assistant
//                            ),
//                                ChatMessage.FromUser("I made tacos tonight.", StaticValues.ChatMessageRoles.User),
//                                ChatMessage.FromAssistant(
//                                @"{
//          ""cmd"": ""create_cooked_recipe"",
//          ""response"": ""Created."",
//          ""recipe"": ""tacos"",
//        }",
//                                StaticValues.ChatMessageRoles.Assistant
//                                ),
//                                ChatMessage.FromUser("Substitute chicken broth for vegetable broth in the chicken and dumplings recipe", StaticValues.ChatMessageRoles.User),
//                                ChatMessage.FromAssistant(
//        @"{
//          ""cmd"": ""substitute_cooked_recipe_ingredient"",
//          ""response"": ""Substituted."",
//          ""recipe"": ""chicken and dumplings"",
//          ""original"": ""chicken broth"",
//          ""new"": ""vegetable broth"",
//        }",
//                                StaticValues.ChatMessageRoles.Assistant
//                                ),
//                                ChatMessage.FromUser("Change the butter unit type to tablespoons in the eggs and spinach recipe", StaticValues.ChatMessageRoles.User),
//                                ChatMessage.FromAssistant(
//        @"{
//          ""cmd"": ""edit_cooked_recipe_ingredient_unittype"",
//          ""response"": ""Changed the butter unit type to tablespoons."",
//          ""recipe"": ""eggs and spinach"",
//          ""ingredient"": ""butter"",
//          ""unittype"": ""tablespoons"",
//        }",
//                                StaticValues.ChatMessageRoles.Assistant
//                                ),
//                                ChatMessage.FromUser("Remove the onions from the spicy chili recipe", StaticValues.ChatMessageRoles.User),
//                                ChatMessage.FromAssistant(
//        @"{
//          ""cmd"": ""remove_cooked_recipe_ingredient"",
//          ""response"": ""Removed."",
//          ""recipe"": ""spicy chili"",
//          ""ingredient"": ""onions"",
//        }",
//                                StaticValues.ChatMessageRoles.Assistant
//                            ),
//                                ChatMessage.FromUser("Remove the chili recipe", StaticValues.ChatMessageRoles.User),
//                                ChatMessage.FromAssistant(
//        @"{
//          ""cmd"": ""delete_cooked_recipe"",
//          ""response"": ""Removed."",
//          ""recipe"": ""chili""
//        }",
//                                StaticValues.ChatMessageRoles.Assistant
//                            )

//                        });
//                    break;
//                case "completed_orders":
//                    chatPromptList.AddRange(new List<ChatMessage>
//                        {
//                            ChatMessage.FromUser("Order two cans of black eyed peas", StaticValues.ChatMessageRoles.User),
//                            ChatMessage.FromAssistant(
//        @"{
//          ""cmd"": ""order"",
//          ""response"": ""Sure, I have added two cans of Black_eyed Peas to your cart."",
//          ""items"": [
//            {
//              ""name"": ""Black_eyed Peas"",
//              ""quantity"": 2
//            }
//          ]
//        }",
//                                StaticValues.ChatMessageRoles.Assistant
//                                )
//                        });
//                    break;
//                case "products":
//                    chatPromptList.AddRange(new List<ChatMessage>
//                        {
//                            ChatMessage.FromUser("Change the unit type on olive oil to ounces", StaticValues.ChatMessageRoles.User),
//                            ChatMessage.FromAssistant(
//        @"{
//          ""cmd"": ""edit_product_unit_type"",
//          ""response"": ""Okay, the unit type has been changed to ounces for olive oil."",
//          ""product"": ""olive oil"",
//          ""unittype"": ""ounces"",
//        }",
//                                StaticValues.ChatMessageRoles.Assistant
//                            ),
//                            ChatMessage.FromUser("Add X", StaticValues.ChatMessageRoles.User),
//                            ChatMessage.FromAssistant(
//        @"{
//          ""cmd"": ""create_product"",
//          ""response"": ""Created."",
//          ""product"": ""X"",
//        }"
//                            ),
//                            ChatMessage.FromUser("Delete X", StaticValues.ChatMessageRoles.User),
//                            ChatMessage.FromAssistant(
//        @"{
//          ""cmd"": ""delete_product"",
//          ""response"": ""Deleted."",
//          ""product"": ""X"",
//        }",
//                                StaticValues.ChatMessageRoles.Assistant
//                            ),
//                        });
//                    break;
//                case "home":
//                default:
//                    return chatPromptList;
//            }
//            return chatPromptList;
//        }

        private List<ChatMessage> GetNormalChatPrompt()
        {
            var chatPromptList = new List<ChatMessage>
            {
                ChatMessage.FromSystem("You are a conversationalist. Have fun talking with the user.", StaticValues.ChatMessageRoles.System),
            };
            return chatPromptList;
        }

        public async Task<string> GetTextFromSpeech(byte[] speechBytes, string? previousMessage)
        {
            var response = await _openAIService.Audio.CreateTranscription(new AudioCreateTranscriptionRequest
            {
                FileName = "blob.wav",
                File = speechBytes,
                Model = Models.WhisperV1,
                ResponseFormat = StaticValues.AudioStatics.ResponseFormat.VerboseJson,
                Prompt = previousMessage,
                Temperature = 0.2f,
                Language = "en",
            });
            if (!response.Successful)
            {
                if (response.Error == null)
                {
                    throw new Exception("Unknown Error");
                }
                else
                {
                    throw new Exception(JsonSerializer.Serialize(response.Error));
                }
            }
            return string.Join("\n", response.Text);
        }
    }
}
