namespace AgenticReportGenerationApi
{
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.ChatCompletion;
    using Microsoft.SemanticKernel.Connectors.OpenAI;
    using System.Text;

    internal static class Util
    {
        public static async Task<string> GetCompanyName(
            IChatCompletionService chat, 
            ChatHistory chatHistory,
            string companyNamesPrompt)
        {
            // build the conversation from chat history so we can focus on just getting the company name
            var sbConversation = new StringBuilder();

            ChatMessageContent[] messageArray = new ChatMessageContent[chatHistory.Count];
            chatHistory.CopyTo(messageArray, 0);

            foreach (var message in messageArray)
            {
                sbConversation.AppendLine($"{message.Role}: {message.Content}");
            }
            
            // add the prompt to instruct the LLM how to pull out the company name
            sbConversation.Append(companyNamesPrompt);

            // ChatHistory is local to this helper since we are only using it to detect intent
            //ChatHistory chatHistory = new ChatHistory();

            var companyName = "not_found";

            //chatHistory.AddSystemMessage(companyNamesPrompt);

            //chatHistory.AddUserMessage(query);

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                Temperature = .5,
            };

            /*var result = await chat.GetChatMessageContentsAsync(
                chatHistory,
                executionSettings);*/

            var result = await chat.GetChatMessageContentsAsync(
                sbConversation.ToString(),
                executionSettings);

            companyName = string.Join(", ", result.Select(o => o.ToString()));

            return companyName;
        }
    }
}