﻿using ContainerNinja.Contracts.Services;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using OpenAI;
using OpenAI.Interfaces;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;

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

        public async Task<string> GetChatResponse(List<ChatMessage> chatMessages, string currentUrl)
        {
            var chatCompletionCreateRequest = CreateChatCompletionCreateRequest(currentUrl);
            chatMessages.ForEach(cm => chatCompletionCreateRequest.Messages.Add(cm));
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
                //FrequencyPenalty = -1,
                //PresencePenalty = -1,
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
                    "go-to-page",
                    "edit-recipe-name",
                    "add-recipe-ingredient",
                    "create-recipe",
                    "delete-recipe",
                    "edit-recipe-ingredient-unittype",
                    "add-recipe-ingredient",
                    "substitute-recipe-ingredient"
                    };
                case "cooked-recipes":
                    return new string[]
                    {
                    "none",
                    "go-to-page",
                    "add-cooked-recipe-ingredient",
                    "create-cooked-recipe",
                    "substitute-cooked-recipe-ingredient",
                    "edit-cooked-recipe-ingredient-unittype"
                    };
                case "products":
                    return new string[]
                    {
                    "none",
                    "go-to-page",
                    "edit-product-unit-type",
                    "create-product",
                    "delete-product"
                    };
                case "home":
                default:
                    return new string[]
                    {
                    "none",
                    "go-to-page"
                    };
            }
        }

        private string[] GetPages()
        {
            return new string[]
            {
            "home",
            "todo",
            "product-stock",
            "products",
            "completed-orders",
            "recipes",
            "cooked-recipes",
                "called-ingredients"
            };
        }

        private List<ChatMessage> GetChatPromptForCurrentUrl(string currentUrl)
        {
            var chatPromptList = new List<ChatMessage>
        {
            ChatMessage.FromSystem(
                "You are a website assistant. You must respond only with JSON and no extra text. The cmd and response fields are required. " +
                "The user is on the home page. The available commands you can return are: none, go-to-page. The available pages are " + string.Join(", ", GetPages()) + "."
                ),
            ChatMessage.FromUser("Hello"),
            ChatMessage.FromAssistant(
@"{
""cmd"": ""none"",
""response"": ""Hi, how may I help you manage your home page?"",
}"
                )
        };
            if (currentUrl != "home")
            {
                chatPromptList.AddRange(new List<ChatMessage>
                {
                ChatMessage.FromUser("Go to " + currentUrl),
                ChatMessage.FromAssistant(
@"{
""cmd"": ""go-to-page"",
""response"": ""Okay, navigating to "
                    + currentUrl + @""",
""page"": """ + currentUrl + @""",
}"
                ),
                ChatMessage.FromSystem(
                    "The user is on the " + currentUrl + " page. The available commands you can return are: " + string.Join(", ", GetCommandsForCurrentUrl(currentUrl)) + "."
                )
            });
            }
            switch (currentUrl)
            {
                case "recipes":
                    chatPromptList.AddRange(new List<ChatMessage>
                    {
                    ChatMessage.FromUser("Change the bienenstich recipe to yummy"),
                    ChatMessage.FromAssistant(
@"{
""cmd"": ""edit-recipe-name"",
  ""response"": ""Okay, I have updated your recipe."",
  ""original"": ""bienenstich"",
  ""new"": ""yummy""
}"
                        ),
                    ChatMessage.FromUser("Can you substitute the bread for gluten free bread in the sandwich recipe?"),
                    ChatMessage.FromAssistant(
@"{
  ""cmd"": ""edit-recipe-ingredient"",
  ""response"": ""Okay, I have substituted the bread ingredient for Gluten-free bread."",
  ""recipe"": ""sandwich"",
  ""original"": ""bread"",
  ""new"": ""Gluten-free bread""
}"
                        ),

                    ChatMessage.FromUser("Create a new recipe that has chicken"),
                    ChatMessage.FromAssistant(
@"{
  ""cmd"": ""create-recipe"",
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
      ""name"": ""deep-dish frozen pie crusts"",
      ""units"": 2,
      ""unittype"": ""whole""
    },
  ]
}"
                        ),
                    ChatMessage.FromUser("Give me a barbeque chicken recipe"),
                    ChatMessage.FromAssistant(
@"{
  ""cmd"": ""create-recipe"",
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
}"
                        ),
                    ChatMessage.FromUser("Remove the spaghetti with meat sauce recipe"),
                    ChatMessage.FromAssistant(
@"{
  ""cmd"": ""delete-recipe"",
  ""response"": ""Removed."",
  ""recipe"": ""spaghetti with meat sauce"",
}"
                        ),
                    ChatMessage.FromUser("Change the butter unit type to tablespoons in the eggs and spinach recipe"),
                    ChatMessage.FromAssistant(
@"{
  ""cmd"": ""edit-recipe-ingredient-unittype"",
  ""response"": ""Changed the butter unit type to tablespoons."",
  ""recipe"": ""eggs and spinach"",
  ""ingredient"": ""butter"",
  ""unittype"": ""tablespoons"",
}"
                        ),
                    ChatMessage.FromUser("Add two teaspoons of pepper to the spicy chili recipe"),
                    ChatMessage.FromAssistant(
@"{
  ""cmd"": ""add-recipe-ingredient"",
  ""response"": ""Added."",
  ""recipe"": ""spicy chili"",
  ""ingredient"": ""pepper"",
  ""unittype"": ""teaspoons"",
  ""units"": 2
}"
                        ),
                    ChatMessage.FromUser("Substitute sesame oil for vegetable oil in the orange chicken recipe"),
                    ChatMessage.FromAssistant(
@"{
  ""cmd"": ""substitute-recipe-ingredient"",
  ""response"": ""Substituted."",
  ""recipe"": ""orange chicken"",
  ""original"": ""sesame oil"",
  ""new"": ""vegetable oil"",
}"
                        ),
                    ChatMessage.FromUser("Remove the onions from the spicy chili recipe"),
                    ChatMessage.FromAssistant(
@"{
  ""cmd"": ""remove-recipe-ingredient"",
  ""response"": ""Removed."",
  ""recipe"": ""spicy chili"",
  ""ingredient"": ""onions"",
}"
                        )
                });
                    break;
                case "cooked-recipes":
                    chatPromptList.AddRange(new List<ChatMessage>
                    {
                        ChatMessage.FromUser("I added two teaspoons of salt to the bienenstich recipe."),
                        ChatMessage.FromAssistant(
@"{
  ""cmd"": ""add-cooked-recipe-ingredient"",
  ""response"": ""Okay, I have added salt to the cooked recipe."",
  ""recipe"": ""bienenstich"",
  ""name"": ""salt"",
  ""units"": 2,
  ""unittype"": ""teaspoon""
}"
                    ),
                        ChatMessage.FromUser("Add syrup to oatmeal"),
                        ChatMessage.FromAssistant(
@"{
  ""cmd"": ""add-cooked-recipe-ingredient"",
  ""response"": ""Okay, I have added syrup to the oatmeal recipe."",
  ""recipe"": ""oatmeal"",
  ""name"": ""syrup"",
  ""units"": 1,
  ""unittype"": ""tablespoon""
}"
                    ),
                        ChatMessage.FromUser("I made tacos tonight."),
                        ChatMessage.FromAssistant(
                        @"{
  ""cmd"": ""create-cooked-recipe"",
  ""response"": ""Created."",
  ""recipe"": ""tacos"",
}"
                        ),
                        ChatMessage.FromUser("Substitute chicken broth for vegetable broth in the chicken and dumplings recipe"),
                        ChatMessage.FromAssistant(
@"{
  ""cmd"": ""substitute-cooked-recipe-ingredient"",
  ""response"": ""Substituted."",
  ""recipe"": ""chicken and dumplings"",
  ""original"": ""chicken broth"",
  ""new"": ""vegetable broth"",
}"
                        ),
                        ChatMessage.FromUser("Change the butter unit type to tablespoons in the eggs and spinach recipe"),
                        ChatMessage.FromAssistant(
@"{
  ""cmd"": ""edit-cooked-recipe-ingredient-unittype"",
  ""response"": ""Changed the butter unit type to tablespoons."",
  ""recipe"": ""eggs and spinach"",
  ""ingredient"": ""butter"",
  ""unittype"": ""tablespoons"",
}"
                        ),
                        ChatMessage.FromUser("Remove the onions from the spicy chili recipe"),
                        ChatMessage.FromAssistant(
@"{
  ""cmd"": ""remove-cooked-recipe-ingredient"",
  ""response"": ""Removed."",
  ""recipe"": ""spicy chili"",
  ""ingredient"": ""onions"",
}"
                    )
                });
                    break;
                case "completed-orders":
                    chatPromptList.AddRange(new List<ChatMessage>
                {
                    ChatMessage.FromUser("Order two cans of black eyed peas"),
                    ChatMessage.FromAssistant(
@"{
  ""cmd"": ""order"",
  ""response"": ""Sure, I have added two cans of Black-eyed Peas to your cart."",
  ""items"": [
    {
      ""name"": ""Black-eyed Peas"",
      ""quantity"": 2
    }
  ]
}"
                        )
                });
                    break;
                case "products":
                    chatPromptList.AddRange(new List<ChatMessage>
                {
                    ChatMessage.FromUser("Change the unit type on olive oil to ounces"),
                    ChatMessage.FromAssistant(
@"{
  ""cmd"": ""edit-product-unit-type"",
  ""response"": ""Okay, the unit type has been changed to ounces for olive oil."",
  ""product"": ""olive oil"",
  ""unittype"": ""ounces"",
}"
                    ),
                    ChatMessage.FromUser("Add X"),
                    ChatMessage.FromAssistant(
@"{
  ""cmd"": ""create-product"",
  ""response"": ""Created."",
  ""product"": ""X"",
}"
                    ),
                    ChatMessage.FromUser("Delete X"),
                    ChatMessage.FromAssistant(
@"{
  ""cmd"": ""delete-product"",
  ""response"": ""Deleted."",
  ""product"": ""X"",
}"
                    ),
                });
                    break;
                case "home":
                default:
                    return chatPromptList;
            }
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