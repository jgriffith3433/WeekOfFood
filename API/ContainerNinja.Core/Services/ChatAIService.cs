using ContainerNinja.Contracts.Services;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using OpenAI;
using OpenAI.Interfaces;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Handlers.ChatCommands;

namespace ContainerNinja.Core.Services
{
    public class ChatAIService : IChatAIService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IOpenAIService _openAIService;

        public ChatAIService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _openAIService = new OpenAIService(new OpenAiOptions()
            {
                Organization = Environment.GetEnvironmentVariable("OpenAIServiceOrganization"),
                ApiKey = Environment.GetEnvironmentVariable("OpenAIServiceApiKey"),
            });
        }

        public async Task<string> GetChatResponse(List<ChatMessageVM> chatMessages, string currentUrl)
        {
            var chatCompletionCreateRequest = CreateChatCompletionCreateRequest(currentUrl);
            chatMessages.ForEach(cm => chatCompletionCreateRequest.Messages.Add(new ChatMessage(cm.Role, cm.RawContent, cm.Name)));
            var completionResult = await _openAIService.ChatCompletion.CreateCompletion(chatCompletionCreateRequest);
            if (completionResult.Error != null)
            {
                return completionResult.Error.Message;
            }
            else
            {
                return completionResult.Choices.First().Message.Content;
            }
        }

        public async Task<string> GetNormalChatResponse(List<ChatMessageVM> chatMessages)
        {
            var chatCompletionCreateRequest = CreateNormalChatCompletionCreateRequest();
            chatMessages.ForEach(cm => chatCompletionCreateRequest.Messages.Add(new ChatMessage(cm.Role, cm.RawContent, cm.Name)));
            var completionResult = await _openAIService.ChatCompletion.CreateCompletion(chatCompletionCreateRequest);
            if (completionResult.Error != null)
            {
                return completionResult.Error.Message;
            }
            else
            {
                return completionResult.Choices.First().Message.Content;
            }
        }

        private ChatCompletionCreateRequest CreateChatCompletionCreateRequest(string currentUrl)
        {
            var chatCompletionCreateRequest = new ChatCompletionCreateRequest
            {
                Messages = GetChatPromptForCurrentUrl(currentUrl),
                Model = Models.ChatGpt3_5Turbo,
                //MaxTokens = 400,
                //FrequencyPenalty = _1,
                //PresencePenalty = _1,
                TopP = 0.1f
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
            "product_stocks",
            "products",
            "completed_orders",
            "recipes",
            "cooked_recipes",
                "called_ingredients"
            };
        }

        //        private List<ChatMessage> GetChatPromptForCurrentUrl(string currentUrl)
        //        {
        //            var chatPromptList = new List<ChatMessage>
        //            {
        //            ChatMessage.FromSystem(
        //@"You are a website assistant. You must respond only in JSON format that resembles this: 
        //{
        //""cmd"": ""X"",
        //""response"": ""Response goes here"",
        //""x"": ""?"",
        //}",
        ////The available commands are: " + GetAllCommands(),
        //                StaticValues.ChatMessageRoles.System
        //                )
        //            };
        //            return chatPromptList;
        //        }

        private List<ChatMessage> GetChatPromptForCurrentUrl(string currentUrl)
        {
            var chatPromptList = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(
                        "You are a website assistant. You must respond only with JSON and no extra text. The cmd and response fields are required. " +
                        "The user is on the home page. The available commands you can return are: none, go_to_page. The available pages are " + string.Join(", ", GetPages()) + ".",
                        StaticValues.ChatMessageRoles.System
                        ),
                    ChatMessage.FromAssistant(
                        @"{
        ""cmd"": ""none"",
        ""response"": ""How can I help you manage your home page?"",
        }",
                        StaticValues.ChatMessageRoles.Assistant),
                    ChatMessage.FromUser("Hello", StaticValues.ChatMessageRoles.User),
                    ChatMessage.FromAssistant(
        @"{
        ""cmd"": ""none"",
        ""response"": ""Hello, is there anything I can help with on your home page?"",
        }",
                        StaticValues.ChatMessageRoles.Assistant
                        )
                };
            if (currentUrl != "home")
            {
                chatPromptList.AddRange(new List<ChatMessage>
                        {
                        ChatMessage.FromUser("Go to " + currentUrl, StaticValues.ChatMessageRoles.User),
                        ChatMessage.FromAssistant(
        @"{
        ""cmd"": ""go_to_page"",
        ""response"": ""Okay, navigating to "
                            + currentUrl + @""",
        ""page"": """ + currentUrl + @""",
        }",
                        StaticValues.ChatMessageRoles.Assistant
                        ),
                        ChatMessage.FromSystem(
                            "The user is on the " + currentUrl + " page. The available commands you can return are: " + string.Join(", ", GetCommandsForCurrentUrl(currentUrl)) + ".",
                            StaticValues.ChatMessageRoles.System
                        ),
                        ChatMessage.FromAssistant(
        @"{
        ""cmd"": ""none"",
        ""response"": ""How can I help you manage your " + currentUrl + @""",
        }",
                        StaticValues.ChatMessageRoles.Assistant),
                    });
            }
            switch (currentUrl)
            {
                case "recipes":
                    chatPromptList.AddRange(new List<ChatMessage>
                            {
                            ChatMessage.FromUser("Change the bienenstich recipe to yummy", StaticValues.ChatMessageRoles.User),
                            ChatMessage.FromAssistant(
        @"{
        ""cmd"": ""edit_recipe_name"",
          ""response"": ""Okay, I have updated your recipe."",
          ""original"": ""bienenstich"",
          ""new"": ""yummy""
        }",
                                StaticValues.ChatMessageRoles.Assistant
                                ),
                            ChatMessage.FromUser("Can you substitute the bread for gluten free bread in the sandwich recipe?", StaticValues.ChatMessageRoles.User),
                            ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""edit_recipe_ingredient"",
          ""response"": ""Okay, I have substituted the bread ingredient for Gluten_free bread."",
          ""recipe"": ""sandwich"",
          ""original"": ""bread"",
          ""new"": ""Gluten_free bread""
        }",
                                StaticValues.ChatMessageRoles.Assistant
                                ),

                            ChatMessage.FromUser("Create a new recipe that has chicken", StaticValues.ChatMessageRoles.User),
                            ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""create_recipe"",
          ""response"": ""Created."",
          ""name"": ""Chicken Pot Pie"",
          ""serves"": 2,
          ""ingredients"": [
            {
              ""name"": ""chicken breast"",
              ""units"": 2,
              ""unittype"": ""cups""
            },
            {
              ""name"": ""mixed vegetables"",
              ""units"": 1,
              ""unittype"": ""can""
            },
            {
              ""name"": ""condensed cream of chicken soup"",
              ""units"": 1,
              ""unittype"": ""can""
            },
            {
              ""name"": ""milk"",
              ""units"": 0.5,
              ""unittype"": ""cups""
            },
            {
              ""name"": ""deep_dish frozen pie crusts"",
              ""units"": 2,
              ""unittype"": ""whole""
            },
          ]
        }",
                                StaticValues.ChatMessageRoles.Assistant
                                ),
                            ChatMessage.FromUser("Give me a barbeque chicken recipe", StaticValues.ChatMessageRoles.User),
                            ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""create_recipe"",
          ""response"": ""Created."",
          ""name"": ""Barbeque Chicken Thighs"",
          ""serves"": 2,
          ""ingredients"": [
            {
              ""name"": ""boneless chicken thighs"",
              ""units"": 3,
              ""unittype"": ""pounds""
            },
            {
              ""name"": ""garlic powder"",
              ""units"": .5,
              ""unittype"": ""teaspoons""
            },
            {
              ""name"": ""BBQ sauce"",
              ""units"": 1,
              ""unittype"": ""bottle""
            },
          ]
        }",
                                StaticValues.ChatMessageRoles.Assistant
                                ),
                            ChatMessage.FromUser("Remove the spaghetti with meat sauce recipe", StaticValues.ChatMessageRoles.User),
                            ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""delete_recipe"",
          ""response"": ""Removed."",
          ""recipe"": ""spaghetti with meat sauce"",
        }",
                                StaticValues.ChatMessageRoles.Assistant
                                ),
                            ChatMessage.FromUser("Change the butter unit type to tablespoons in the eggs and spinach recipe", StaticValues.ChatMessageRoles.User),
                            ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""edit_recipe_ingredient_unittype"",
          ""response"": ""Changed the butter unit type to tablespoons."",
          ""recipe"": ""eggs and spinach"",
          ""ingredient"": ""butter"",
          ""unittype"": ""tablespoons"",
        }",
                                StaticValues.ChatMessageRoles.Assistant
                                ),
                            ChatMessage.FromUser("Add two teaspoons of pepper to the spicy chili recipe", StaticValues.ChatMessageRoles.User),
                            ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""add_recipe_ingredient"",
          ""response"": ""Added."",
          ""recipe"": ""spicy chili"",
          ""ingredient"": ""pepper"",
          ""unittype"": ""teaspoons"",
          ""units"": 2
        }",
                                StaticValues.ChatMessageRoles.Assistant
                                ),
                            ChatMessage.FromUser("Substitute sesame oil for vegetable oil in the orange chicken recipe", StaticValues.ChatMessageRoles.User),
                            ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""substitute_recipe_ingredient"",
          ""response"": ""Substituted."",
          ""recipe"": ""orange chicken"",
          ""original"": ""sesame oil"",
          ""new"": ""vegetable oil"",
        }",
                                StaticValues.ChatMessageRoles.Assistant
                                ),
                            ChatMessage.FromUser("Remove the onions from the spicy chili recipe", StaticValues.ChatMessageRoles.User),
                            ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""remove_recipe_ingredient"",
          ""response"": ""Removed."",
          ""recipe"": ""spicy chili"",
          ""ingredient"": ""onions"",
        }",
                                StaticValues.ChatMessageRoles.Assistant
                                )
                        });
                    break;
                case "cooked_recipes":
                    chatPromptList.AddRange(new List<ChatMessage>
                            {
                                ChatMessage.FromUser("I added two teaspoons of salt to the bienenstich recipe.", StaticValues.ChatMessageRoles.User),
                                ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""add_cooked_recipe_ingredient"",
          ""response"": ""Okay, I have added salt to the cooked recipe."",
          ""recipe"": ""bienenstich"",
          ""name"": ""salt"",
          ""units"": 2,
          ""unittype"": ""teaspoon""
        }",
                                StaticValues.ChatMessageRoles.Assistant
                            ),
                                ChatMessage.FromUser("Add syrup to oatmeal", StaticValues.ChatMessageRoles.User),
                                ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""add_cooked_recipe_ingredient"",
          ""response"": ""Okay, I have added syrup to the oatmeal recipe."",
          ""recipe"": ""oatmeal"",
          ""name"": ""syrup"",
          ""units"": 1,
          ""unittype"": ""tablespoon""
        }",
                                StaticValues.ChatMessageRoles.Assistant
                            ),
                                ChatMessage.FromUser("I made tacos tonight.", StaticValues.ChatMessageRoles.User),
                                ChatMessage.FromAssistant(
                                @"{
          ""cmd"": ""create_cooked_recipe"",
          ""response"": ""Created."",
          ""recipe"": ""tacos"",
        }",
                                StaticValues.ChatMessageRoles.Assistant
                                ),
                                ChatMessage.FromUser("Substitute chicken broth for vegetable broth in the chicken and dumplings recipe", StaticValues.ChatMessageRoles.User),
                                ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""substitute_cooked_recipe_ingredient"",
          ""response"": ""Substituted."",
          ""recipe"": ""chicken and dumplings"",
          ""original"": ""chicken broth"",
          ""new"": ""vegetable broth"",
        }",
                                StaticValues.ChatMessageRoles.Assistant
                                ),
                                ChatMessage.FromUser("Change the butter unit type to tablespoons in the eggs and spinach recipe", StaticValues.ChatMessageRoles.User),
                                ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""edit_cooked_recipe_ingredient_unittype"",
          ""response"": ""Changed the butter unit type to tablespoons."",
          ""recipe"": ""eggs and spinach"",
          ""ingredient"": ""butter"",
          ""unittype"": ""tablespoons"",
        }",
                                StaticValues.ChatMessageRoles.Assistant
                                ),
                                ChatMessage.FromUser("Remove the onions from the spicy chili recipe", StaticValues.ChatMessageRoles.User),
                                ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""remove_cooked_recipe_ingredient"",
          ""response"": ""Removed."",
          ""recipe"": ""spicy chili"",
          ""ingredient"": ""onions"",
        }",
                                StaticValues.ChatMessageRoles.Assistant
                            ),
                                ChatMessage.FromUser("Remove the chili recipe", StaticValues.ChatMessageRoles.User),
                                ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""delete_cooked_recipe"",
          ""response"": ""Removed."",
          ""recipe"": ""chili""
        }",
                                StaticValues.ChatMessageRoles.Assistant
                            )

                        });
                    break;
                case "completed_orders":
                    chatPromptList.AddRange(new List<ChatMessage>
                        {
                            ChatMessage.FromUser("Order two cans of black eyed peas", StaticValues.ChatMessageRoles.User),
                            ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""order"",
          ""response"": ""Sure, I have added two cans of Black_eyed Peas to your cart."",
          ""items"": [
            {
              ""name"": ""Black_eyed Peas"",
              ""quantity"": 2
            }
          ]
        }",
                                StaticValues.ChatMessageRoles.Assistant
                                )
                        });
                    break;
                case "products":
                    chatPromptList.AddRange(new List<ChatMessage>
                        {
                            ChatMessage.FromUser("Change the unit type on olive oil to ounces", StaticValues.ChatMessageRoles.User),
                            ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""edit_product_unit_type"",
          ""response"": ""Okay, the unit type has been changed to ounces for olive oil."",
          ""product"": ""olive oil"",
          ""unittype"": ""ounces"",
        }",
                                StaticValues.ChatMessageRoles.Assistant
                            ),
                            ChatMessage.FromUser("Add X", StaticValues.ChatMessageRoles.User),
                            ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""create_product"",
          ""response"": ""Created."",
          ""product"": ""X"",
        }"
                            ),
                            ChatMessage.FromUser("Delete X", StaticValues.ChatMessageRoles.User),
                            ChatMessage.FromAssistant(
        @"{
          ""cmd"": ""delete_product"",
          ""response"": ""Deleted."",
          ""product"": ""X"",
        }",
                                StaticValues.ChatMessageRoles.Assistant
                            ),
                        });
                    break;
                case "home":
                default:
                    return chatPromptList;
            }
            return chatPromptList;
        }

        private List<ChatMessage> GetNormalChatPrompt()
        {
            var chatPromptList = new List<ChatMessage>
            {
                ChatMessage.FromSystem("You are a conversationalist. Have fun talking with the user.", StaticValues.ChatMessageRoles.System),
            };
            return chatPromptList;
        }

        public async Task<string> GetTextFromSpeech(byte[] speechBytes)
        {
            var response = await _openAIService.Audio.CreateTranscription(new AudioCreateTranscriptionRequest
            {
                FileName = "blob.webm",
                File = speechBytes,
                Model = Models.WhisperV1,
                ResponseFormat = StaticValues.AudioStatics.ResponseFormat.VerboseJson
            });
            if (!response.Successful)
            {
                if (response.Error == null)
                {
                    throw new Exception("Unknown Error");
                }
                else
                {
                    throw new Exception(JsonConvert.SerializeObject(response.Error));
                }
            }
            return string.Join("\n", response.Text);
        }
    }
}
