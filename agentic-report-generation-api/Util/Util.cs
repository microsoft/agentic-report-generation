namespace AgenticReportGenerationApi
{
    using Microsoft.SemanticKernel.ChatCompletion;
    using Microsoft.SemanticKernel.Connectors.OpenAI;
    
    internal static class Util
    {
        public static async Task<string> GetCompanyName(IChatCompletionService chat, string query, string companyNamesPrompt)
        {
            // ChatHistory is local to this helper since we are only using it to detect intent
            ChatHistory chatHistory = new ChatHistory();

            var companyName = "not_found";

            chatHistory.AddSystemMessage(companyNamesPrompt);

            chatHistory.AddUserMessage(query);

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                Temperature = .5,
                // This is very important as it allows us to instruct the model to give us 3 results for the prompt in one call, this is very powerful
                //ResultsPerPrompt = 3,
            };

            // Call the chat completion asking for 3 rounds to attempt to identify the intent
            var result = await chat.GetChatMessageContentsAsync(
                chatHistory,
                executionSettings);

            companyName = string.Join(", ", result.Select(o => o.ToString()));

            return companyName;
        }
    }
}