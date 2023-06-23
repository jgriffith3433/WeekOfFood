using ContainerNinja.Contracts.Services;
using Microsoft.AspNetCore.Hosting;
using OpenAI;
using OpenAI.Interfaces;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using System.Reflection;
using ContainerNinja.Contracts.Common;
using Newtonsoft.Json;
using System.Dynamic;
using Microsoft.IdentityModel.Tokens;

namespace ContainerNinja.Core.Services
{
    public class ChatAIService : IChatAIService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IOpenAIService _openAIService;
        private static IList<FunctionDefinition> _functionSpecifications = GetChatCommandSpecifications();

        public ChatAIService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _openAIService = new OpenAIService(new OpenAiOptions()
            {
                Organization = Environment.GetEnvironmentVariable("OpenAIServiceOrganization"),
                ApiKey = Environment.GetEnvironmentVariable("OpenAIServiceApiKey"),
            });
        }

        public static IList<FunctionDefinition> GetChatCommandSpecifications()
        {
            try
            {
                var chatFunctions = new List<FunctionDefinition>();
                foreach (var ccsType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.GetCustomAttribute<ChatCommandSpecification>() != null))
                {
                    var ccs = ccsType.GetCustomAttribute<ChatCommandSpecification>();

                    foreach (var name in ccs.Names)
                    {
                        chatFunctions.Add(new FunctionDefinition
                        {
                            Name = name,
                            Description = ccs.Description,
                            Parameters = ccs.GetFunctionParametersFromType(ccsType),
                        });
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
                return new List<FunctionDefinition>();
            }
        }

        public async Task<ChatMessageVM> GetChatResponse(List<ChatMessageVM> chatMessages, string forceFunctionCall)
        {
            var chatCompletionCreateRequest = CreateChatCompletionCreateRequest();
            if (!string.IsNullOrEmpty(forceFunctionCall) && forceFunctionCall.Contains("{"))
            {
                chatCompletionCreateRequest.FunctionCall = JsonConvert.DeserializeObject<ExpandoObject>(forceFunctionCall);
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
                    FunctionCall = FunctionCallToJson(chatResponse.FunctionCall),
                };
            }
        }

        private string GetToFromChatResponse(ChatMessage chatMessage)
        {
            if (chatMessage.FunctionCall != null)
            {
                return "function";
            }
            else
            {
                return StaticValues.ChatMessageRoles.User;
            }
            //if (IsString(chatMessage.FunctionCall))
            //{
            //    return "function";
            //}
            //else
            //{
            //    if (IsDefined(chatMessage.FunctionCall))
            //    {
            //        return "function";
            //    }
            //    else
            //    {
            //        return StaticValues.ChatMessageRoles.User;
            //    }
            //}
        }

        //public bool IsDefined(JsonObject? jsonElement)
        //{
        //    return jsonElement.HasValue && jsonElement.Value.ValueKind != JsonValueKind.Null && jsonElement.Value.ValueKind != JsonValueKind.Undefined;
        //}

        //public bool IsString(JsonObject? jObject)
        //{
        //    return jObject.HasValue && jObject.Value.ValueKind == JsonValueKind.String;
        //}

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
                    FunctionCall = FunctionCallToJson(chatResponse.FunctionCall),
                };
            }
        }

        protected static FunctionParameters? JsonToFunctionParameters(string? jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<FunctionParameters>(jsonString, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });
        }

        protected static string? FunctionCallToJson(FunctionCall? functionCall)
        {
            if (functionCall == null)
            {
                return null;
            }
            return JsonConvert.SerializeObject(functionCall, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
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
                //TopP = 0.f,
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
        /*
ChatMessage.FromSystem(@"You are a kitchen assistant, your name is Kitchy. In general, if you are unsure of a value, you should stop and ask the user for clarification.", StaticValues.ChatMessageRoles.System),
ChatMessage.FromSystem(@"This is the flow of updating the kitchen inventory..", StaticValues.ChatMessageRoles.System),
ChatMessage.FromUser(@"I'm going to tell you what stocked products I have in my kitchen and how much I have of each one. Before you update the system, can you verify with me first?", StaticValues.ChatMessageRoles.User),
ChatMessage.FromAssistant(@"Of course! Please go ahead and tell me the stocked products you have in your kitchen and how much of each one you have. I will search for them in the system based on the provided names and then verify with you before updating the system.", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromSystem(@"[User provides the stocked products and quantities without specifying StockedProductIds].", StaticValues.ChatMessageRoles.System),
ChatMessage.FromSystem(@"[Assistant calls the function to search for the stocked products based on the provided names and retrieves the corresponding StockedProductIds]", StaticValues.ChatMessageRoles.System),
ChatMessage.FromAssistant(@"Got it! I have found the StockedProductIds for the stocked products you mentioned. Now, can I update your inventory with the specified quantities?.", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromSystem(@"[User indicates to go ahead and update the system].", StaticValues.ChatMessageRoles.User),
ChatMessage.FromSystem(@"[Assistant calls the function to update the inventory for each stocked product using the retrieved StockedProductIds and using the units and unit types that the user specified].", StaticValues.ChatMessageRoles.System),
ChatMessage.FromAssistant(@"Great! I have updated your inventory with the specified quantities of each stocked product. Is there anything else you would like to add or update?", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromSystem(@"[User provides additional stocked products or quantities without specifying StockedProductIds].", StaticValues.ChatMessageRoles.System),
ChatMessage.FromSystem(@"[Assistant calls the function to search for the additional stocked products based on the provided names and retrieves the corresponding StockedProductIds]", StaticValues.ChatMessageRoles.System),
ChatMessage.FromAssistant(@"Thank you for providing the stocked products and quantities. I will update your inventory once you confirm the quantities and products. Can I update your inventory with the specified quantities?", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromSystem(@"[User indicates to go ahead and update the system].", StaticValues.ChatMessageRoles.System),
ChatMessage.FromSystem(@"[Assistant calls the function to update the inventory for each additional stocked product using the retrieved StockedProductIds and using the units and unit types that the user specified]", StaticValues.ChatMessageRoles.System),
ChatMessage.FromAssistant(@"Great! I have updated your inventory with the specified quantities of each additional stocked product. Is there anything else you would like to add or update?", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromSystem(@"[User indicates no further updates].", StaticValues.ChatMessageRoles.System),
ChatMessage.FromAssistant(@"Alright if you need any further assistance, feel free to let me know!", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromSystem(@"End of updating kitchen inventory flow.", StaticValues.ChatMessageRoles.System),

*/

        /*
User: ""Start kitchen inventory flow""
Assistant: ""Okay how can I help you manage your kitchen inventory?
Use the following loop until the user has finished working on their inventory
[BEGIN kitchen_inventory_loop]
1) [User tells you about the StockedProductUnits and StockedProductUnitType of a stocked product in their kitchen]
2) [If you do not have all of the StockedProductId values then call the ""search_stocked_products"" function]
3) [If you found the StockedProductId values from searching then ask the user if you can update that record]
4) [If you didn't find the StockedProductId values then call the ""create_stocked_products"" function]
5) [If you have the StockedProductId values from either ""search_stocked_products"" or ""create_stocked_products"" and you have enough information to fill out StockedProductKitchenUnits and StockedProductKitchenUnitType ask the user if you can call the ""update_stocked_products"" function]
6) [If the user gives you permission to call ""update_stocked_products"" then call it and respond back to the user what the results are and then ask if there is anything else you can help them with]
7) [Repeat this until the user says they are done working managing their kitchen inventory]
[END kitchen_inventory_loop]

*/
        /*
         
This is the flow of updating the kitchen inventory.
User: ""Start kitchen inventory flow""
Assistant: ""Okay, how can I help you manage your kitchen inventory?""
Use the following loop until you are finished managing your inventory:
[BEGIN kitchen_inventory_loop]
1) User tells you about the StockedProductUnits and StockedProductUnitType of a stocked product in their kitchen.
2) If you do not have all of the StockedProductId values, call the ""search_stocked_products"" function.
3) If you found the StockedProductId values from searching, ask the user if you can update that record.
4) If you didn't find the StockedProductId values, call the ""create_stocked_products"" function.
5) If you have the StockedProductId values from either ""search_stocked_products"" or ""create_stocked_products"" and you have enough information to fill out StockedProductKitchenUnits and StockedProductKitchenUnitType, ask the user for permission to call the ""update_stocked_products"" function.
6) If the user gives you permission, call the ""update_stocked_products"" function and respond back to the user with the results. Then ask if there is anything else you can help them with.
7) Repeat this loop until the user says they are done managing their kitchen inventory.
[END kitchen_inventory_loop]

         */
        /*
Your name is Kitchy, and you are an extremely logical and rigid inventory management ai.
You help the user manage food items in their kitchen.
You are very clear about what action you are about to perform. You call functions only when you have all of the neccessary information.
You update the inventory only when a user tells you to.
When creating, searching or updating the kitchen inventory respond back with the KitchenProductId of each item and ask if you can update the system.
You are not allowed to make up arguments when calling functions.
You must verify with the user about the arguments in a function before calling them.
You must NEVER say you called a function without doing so prior*/

        private List<ChatMessage> GetChatPrompt()
        {
            var chatPromptList = new List<ChatMessage>
{//You help the user log meals, create/modify/delete logged meals, recipes, and ingredients. You help the user in using the website.
                ChatMessage.FromSystem(@"
Your name is Kitchy, you are an kitchen management ai and help the user call functions.
"
, StaticValues.ChatMessageRoles.System),
                ChatMessage.FromUser(@"Hello", StaticValues.ChatMessageRoles.User),
                ChatMessage.FromAssistant(@"Hello! How can I assist you today?", StaticValues.ChatMessageRoles.Assistant),
/*
 
User: ""I'm going to tell you what stocked products I have in my kitchen and how much I have of each one. Before you update the system, can you verify with my first?""
Assistant: ""Of course! Please go ahead and tell me the stocked products you have in your kitchen and how much of each one you have. I will search for them in the system based on the provided names and then verify with you before updating the system.""
User: [Provides the stocked products and quantities without specifying StockedProductIds]
Assistant: [Calls the function to search for the stocked products based on the provided names and retrieves the corresponding StockedProductIds]
Assistant: ""Got it! I have found the StockedProductIds for the stocked products you mentioned. Now, Can I update your inventory with the specified quantities?""
User: [Indicates to go ahead and update the system]
[Assistant calls the function to update the inventory for each stocked product using the retrieved StockedProductIds and using the units and unit types that the user specified]
Assistant: ""Great! I have updated your inventory with the specified quantities of each stocked product. Is there anything else you would like to add or update?""
User: [Provides additional stocked products or quantities without specifying StockedProductIds]
Assistant: [Calls the function to search for the additional stocked products based on the provided names and retrieves the corresponding StockedProductIds]
Assistant: ""Perfect! I have found the StockedProductIds for the additional stocked products you mentioned. Can I update your inventory with the specified quantities?""
[Assistant calls the function to update the inventory for each additional stocked product using the retrieved StockedProductIds and using the units and unit types that the user specified]
Assistant: ""Great! I have updated your inventory with the specified quantities of each additional stocked product. Is there anything else you would like to add or update?""
User: [Indicates no further updates]
Assistant: ""Alright, your inventory has been successfully updated. If you need any further assistance, feel free to let me know!""
End of updateing kitchen inventory flow.
 */
        /*
         This is the flow of updating the kitchen inventory. In general, if you are unsure of a value, you should stop and ask the user for clarification.
        User: ""I'm going to tell you what stocked products I have in my kitchen and how much I have of each one. Can you update my inventory in the system?""
        Assistant: ""Of course! Please go ahead and tell me the stocked products you have in your kitchen and how much of each one you have. I will search for them in the system based on the provided names.""
        User: [Provides the stocked products and quantities without specifying StockedProductIds]
        Assistant: [Calls the function to search for the stocked products based on the provided names and retrieves the corresponding StockedProductIds]
        Assistant: ""Got it! I have found the StockedProductIds for the stocked products you mentioned. Now, I will update your inventory with the specified quantities.""
        Assistant: [Calls the function to update the inventory for each stocked product using the retrieved StockedProductIds but using the units and unit types that the user specified]
        Assistant: ""Great! I have updated your inventory with the specified quantities of each stocked product. Is there anything else you would like to add or update?""
        User: [Provides additional stocked products or quantities without specifying StockedProductIds]
        Assistant: [Calls the function to search for the additional stocked products based on the provided names and retrieves the corresponding StockedProductIds]
        Assistant: ""Perfect! I have found the StockedProductIds for the additional stocked products you mentioned. Now, I will update your inventory with the specified quantities.""
        Assistant: [Calls the function to update the inventory for each additional stocked product using the retrieved StockedProductIds]
        Assistant: ""Great! I have updated your inventory with the specified quantities of each additional stocked product. Is there anything else you would like to add or update?""
        User: [Indicates no further updates]
        Assistant: ""Alright, your inventory has been successfully updated. If you need any further assistance, feel free to let me know!""
        */



        /*
        This is the flow of updating the kitchen inventory. In general, if you are unsure of a value, you should stop and ask the user for clarification.
        User: ""I'm going to tell you what stocked products I have in my kitchen and how much I have of each one. Can you update my inventory in the system?""
        Assistant: ""Of course! Please go ahead and tell me the stocked products you have in your kitchen and how much of each one you have. I will search for them in the system based on the provided names.""
        User: [Provides the stocked products and quantities without specifying StockedProductIds]
        Assistant: [Calls the function to search for the stocked products based on the provided names and retrieves the corresponding StockedProductIds]
        Assistant: ""Got it! I have found the StockedProductIds for the stocked products you mentioned. Now, I will update your inventory with the specified quantities.""
        Assistant: [Calls the function to update the inventory for each stocked product using the retrieved StockedProductIds but using the units and unit types that the user specified]
        Assistant: ""Great! I have updated your inventory with the specified quantities of each stocked product. Is there anything else you would like to add or update?""
        User: [Provides additional stocked products or quantities without specifying StockedProductIds]
        Assistant: [Calls the function to search for the additional stocked products based on the provided names and retrieves the corresponding StockedProductIds]
        Assistant: ""Perfect! I have found the StockedProductIds for the additional stocked products you mentioned. Now, I will update your inventory with the specified quantities.""
        Assistant: [Calls the function to update the inventory for each additional stocked product using the retrieved StockedProductIds]
        Assistant: ""Great! I have updated your inventory with the specified quantities of each additional stocked product. Is there anything else you would like to add or update?""
        User: [Indicates no further updates]
        Assistant: ""Alright, your inventory has been successfully updated. If you need any further assistance, feel free to let me know!""
        */

        /*
        User: ""I'm going to tell you what stocked products I have in my kitchen and how much I have of each one. Can you update my inventory in the system?""
        Assistant: ""Of course! Please go ahead and tell me the stocked products you have in your kitchen and how much of each one you have. If there is any information that I need to clarify, I will ask for more details.""
        User: [Provides the stocked products and quantities]
        Assistant: [Calls the function to update the inventory for each stocked product in a loop]
        Assistant: ""Great! I have updated your inventory with the specified quantities of each stocked product. Is there anything else you would like to add or update?""
        User: [Provides additional stocked products or quantities]
        Assistant: [Calls the function to update the inventory for each additional stocked product in a loop]
        Assistant: ""Great! I have updated your inventory with the specified quantities of each additional stocked product. Is there anything else you would like to add or update?""
        User: [Indicates no further updates]
        Assistant: ""Alright, your inventory has been successfully updated. If you need any further assistance, feel free to let me know!


        */
        /*
        The flow for taking inventory:
        User: ""I'm going to tell you what stocked products I have in my kitchen and how much I have of each one. Can you update my inventory in the system?""
        Assistant: ""Of course! Please go ahead and tell me the stocked products you have in your kitchen and how much of each one you have. If there is any information that I need to clarify, I will ask for more details.""
        User: [Provides the stocked products and quantities]
        Assistant: [Call the function to update the inventory for each stocked product]
        Assistant: ""Great! I have updated your inventory with the specified quantities of each stocked product. Is there anything else you would like to add or update?""
        User: [Provides additional stocked products or quantities]
        Assistant: [Call the function to update the inventory for each stocked product]
        Assistant: ""Great! I have updated your inventory with the specified quantities of each stocked product. Is there anything else you would like to add or update?""
        User: [Indicates no further updates]
        Assistant: ""Alright, your inventory has been successfully updated. If you need any further assistance, feel free to let me know!""

        */

        //ChatMessage.FromUser(@"I have a jar of peanut butter.", StaticValues.ChatMessageRoles.User),
        //new ChatMessage("function", "[{\"StockedProductId\":4,\"StockedProductName\":\"peanut butter\"}]", "search_stocked_products"),
        //new ChatMessage("function", "[{\"StockedProductId\":4,\"StockedProductName\":\"peanut butter\"}]", "create_stocked_products"),
        //ChatMessage.FromAssistant(@"Okay I have added a jar of peanut butter to your inventory?", StaticValues.ChatMessageRoles.Assistant),

        /*

        The flow for taking inventory:
        User: ""I'm going to tell you what stocked products I have in my kitchen and how much I have of each one. Can you update my inventory in the system?""
        Assistant: ""Of course! Please go ahead and tell me the stocked products you have in your kitchen and how much of each one you have. If there is any information that I need to clarify, I will ask for more details.""
        User: [Provides the stocked products and quantities]
        Assistant: [Call the appropriate function]
        Function: [Gives results about the function that was called]
        Then confirm with the user the information that was changed based on the response from the function.



        The flow goes like this:
        1) The user tells you something.
        2) You gather information.
        3) If the function has an Id parameter you must search for it by calling other functions.
        4) Once you successfully call the function consolidate the information from the the function and tell the user what action that was performed.
        5) Standby and get ready to receive more requests and repeat this conversation flow.

        Additional information:
        When placing an order, you are trying to add stocked products to the order.
        Recipes have ingredients that are linked to stocked products so you use the stocked products to order everything a user needs to make a recipe.

        When taking stock, the user is telling you what they have in their kitchen and you need to update the stocked products in the system to reflect that.
        You need to call create_stocked_products or update_stocked_products everytime a user tells you what they have in their kithen based on whether or not a record exists for that stocked product.
        */

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
                    throw new Exception(JsonConvert.SerializeObject(response.Error));
                }
            }
            return string.Join("\n", response.Text);
        }
    }
}
