namespace AgenticReportGenerationApi
{
    using Microsoft.SemanticKernel.ChatCompletion;
    using Microsoft.SemanticKernel.Connectors.OpenAI;
    
    internal static class Util
    {
        public static async Task<string> GetCompanyName(
            IChatCompletionService chat, 
            string query, 
            string companyNamesPrompt)
        {
            // ChatHistory is local to this helper since we are only using it to detect intent
            ChatHistory chatHistory = new ChatHistory();

            var companyName = "not_found";

            chatHistory.AddSystemMessage(companyNamesPrompt);

            chatHistory.AddUserMessage(query);

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                Temperature = .5,
            };

            var result = await chat.GetChatMessageContentsAsync(
                chatHistory,
                executionSettings);

            companyName = string.Join(", ", result.Select(o => o.ToString()));

            return companyName;
        }
    }
}