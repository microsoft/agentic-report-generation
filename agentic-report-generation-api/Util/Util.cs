namespace AgenticReportGenerationApi
{
    using AgenticReportGenerationApi.Prompts;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.ChatCompletion;
    using Microsoft.SemanticKernel.Connectors.OpenAI;
    using System.Text;

    internal static class Util
    {
        public static async Task<string> GetCompanyName(
            IChatCompletionService chat, 
            ChatHistory companyNameChatHistory
            )
        {
            var companyName = "not_found";

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                Temperature = .5,
            };

            var result = await chat.GetChatMessageContentsAsync(
                companyNameChatHistory,
                executionSettings);

            companyName = string.Join(", ", result.Select(o => o.ToString()));

            return companyName;
        }

        public static async Task<bool> HasCompanyId(
            IChatCompletionService chat,
            ChatHistory chatHistory)
        {
            ChatHistory tempChatHistory = new ChatHistory();

            tempChatHistory.AddSystemMessage(CorePrompts.CheckForCompanyIdPrompt());

            // copy all chat history into the temporary chat history and remove the system message so we can add the system message for checking for the company id
            List<ChatMessageContent> chatHistoryMessageList = chatHistory.ToArray().ToList();
            foreach (ChatMessageContent message in chatHistoryMessageList)
            {
                if (message.Content != null)
                {
                    tempChatHistory.AddUserMessage(message.Content);
                }
            }

            // we add the check company id system prompt to tempChatHistory first, so the second element is the system message
            // of the main system prompt we are using for conversation chat history, so we remove it
            tempChatHistory.RemoveAt(1);            

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                Temperature = .5,
            };

            var result = await chat.GetChatMessageContentsAsync(
                tempChatHistory,
                executionSettings);

            return Boolean.Parse(result[0].Content);
        }
    }
}