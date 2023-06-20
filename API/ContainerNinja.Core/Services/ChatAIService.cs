using ContainerNinja.Contracts.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using OpenAI;
using OpenAI.Interfaces;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using System.Reflection;
using NJsonSchema;
using System.Linq;
using ContainerNinja.Contracts.Common;
using Newtonsoft.Json.Linq;

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
            try
            {
                var chatFunctions = new List<ChatFunction>();
                foreach (var ccsType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.GetCustomAttribute<ChatCommandSpecification>() != null))
                {
                    var ccs = ccsType.GetCustomAttribute<ChatCommandSpecification>();

                    foreach (var name in ccs.Names)
                    {
                        ccs.CreateParametersSchemaFromType(ccsType);
                        chatFunctions.Add(new ChatFunction(name, ccs.Description, ccs.ParametersSchema));
                    }
                }
                return chatFunctions;
            }
            catch (Exception ex)
            {
                //weird error
                /*Could not load type 'Castle.Proxies.CookedRecipeCalledIngredientProxy' from assembly 'DynamicProxyGenAssembly2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'.
                 at System.Reflection.RuntimeModule.GetTypes(RuntimeModule module)
                */
                return new List<ChatFunction>();
            }
        }

        public async Task<ChatMessageVM> GetChatResponse(List<ChatMessageVM> chatMessages, string forceFunctionCall)
        {
            var chatCompletionCreateRequest = CreateChatCompletionCreateRequest();
            if (!string.IsNullOrEmpty(forceFunctionCall) && forceFunctionCall.Contains("{"))
            {
                chatCompletionCreateRequest.FunctionCall = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(forceFunctionCall);
            }
            else
            {
                chatCompletionCreateRequest.FunctionCall = forceFunctionCall;
            }
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
                    FunctionCall = chatResponse.FunctionCall,
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
                    FunctionCall = chatResponse.FunctionCall,
                };
            }
        }

        private ChatCompletionCreateRequest CreateChatCompletionCreateRequest()
        {
            var chatCompletionCreateRequest = new ChatCompletionCreateRequest
            {
                Messages = GetChatPrompt(),
                Model = "gpt-3.5-turbo-16k-0613",//Models.ChatGpt3_5Turbo0613,
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
                Temperature = 0.6f
            };
            return chatCompletionCreateRequest;
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
{//You help the user log meals, create/modify/delete logged meals, recipes, and ingredients. You help the user in using the website.
ChatMessage.FromSystem(
@"You are a kitchen assistant. You can call functions that dont have the UserGavePermission field any time.
You can NOT call functions that have the UserGavePermission field if you have not asked permission to.
The user gives permission then you are allowed to set the UserGavePermission field to true for that one function call ONLY and then you must set it back to false until they give permission again.
The flow goes like this:
1) The user tells you something.
2) You gather information by asking the user or by calling a function.
3) If the function has an Id parameter you must search for it by calling other functions.
4) If the function requires permission with the UserGavePermission field you must ask the user first.
5) If the user gives permission go ahead and call the function, otherwise ask the user.
6) Once you successfully call the function consolidate the information from the the function and tell the user what action that was performed.

When placing an order, you are trying to add stocked products to the order.
Recipes have ingredients that are linked to stocked products so you are able to order everything a user needs to make a recipe.

When taking stock, the user is telling you what they have in their kitchen and you need to update the stocked products in the system to reflect that.

", StaticValues.ChatMessageRoles.System),
/*
ChatMessage.FromUser(@"I ate a sandwich for lunch.", StaticValues.ChatMessageRoles.User),
ChatMessage.FromFunction(_searchRecipesJsonArray, "search_recipes"),
ChatMessage.FromAssistant(@"Okay did you have a ham, turkey, or salami sandwich?", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromUser(@"It was a salami sandwich.", StaticValues.ChatMessageRoles.User),
ChatMessage.FromFunction(_salamiSandwichJsonObject, "get_recipe_ingredients"),
ChatMessage.FromAssistant(@"Okay did anything vary from the recipe?", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromUser(@"I didn't use mustard, I used mayo instead.", StaticValues.ChatMessageRoles.User),
ChatMessage.FromAssistant(@"Okay, the recipe called for 1 teaspoon of mustard, did you use about 1 teaspoon of mayo", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromUser(@"Yeah about", StaticValues.ChatMessageRoles.User),
ChatMessage.FromAssistant(@"Okay, would you like me to log that you ate a salami sandwich with mayo for lunch?", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromUser(@"Yes", StaticValues.ChatMessageRoles.User),
ChatMessage.FromFunction(_salamiSandwichWithMayoJsonObject, "log_recipe"),
ChatMessage.FromAssistant(@"Okay, I have logged that you had a salami sandwich for lunch and that you substituted mustard for mayo. Is there anything else I can help you with?", StaticValues.ChatMessageRoles.Assistant),
*/
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
