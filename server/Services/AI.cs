// // Copyright (c) Microsoft Corporation. All rights reserved.
// // Licensed under the MIT License.

// using System;
// using System.Threading.Tasks;
// using Azure.Identity;
// using Azure.AI.OpenAI;

using Azure;
using Azure.AI.OpenAI;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Tailwind.AI;

public partial class Chat
{

    public async Task<string> Prompt(string prompt)
    {
      var config = Viper.Config();
      Uri azureOpenAIResourceUri = new(config.Get("AZURE_OPENAI_ENDPOINT"));
      var key = config.Get("AZURE_OPENAI_API_KEY");
      AzureKeyCredential azureOpenAIApiKey = new(key);
      OpenAIClient client = new(azureOpenAIResourceUri, azureOpenAIApiKey);

      var chatCompletionsOptions = new ChatCompletionsOptions()
      {
          DeploymentName = "gpt-4", // Use DeploymentName for "model" with non-Azure clients
          Messages =
          {
              // The system message represents instructions or other guidance about how the assistant should behave
              // new ChatRequestSystemMessage("You are a helpful assistant. You will talk like a pirate."),
              // User messages represent current or historical input from the end user
              new ChatRequestUserMessage(prompt),
              // Assistant messages represent historical responses from the assistant
              //new ChatRequestAssistantMessage("Arrrr! Of course, me hearty! What can I do for ye?"),
              //new ChatRequestUserMessage("What's the best way to train a parrot?"),
          }
      };

      Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
      var responseMessage = response.Value.Choices[0].Message;
      //Console.WriteLine($"[{responseMessage.Role.ToString().ToUpperInvariant()}]: {responseMessage.Content}");
      return responseMessage.Content;
    }
}