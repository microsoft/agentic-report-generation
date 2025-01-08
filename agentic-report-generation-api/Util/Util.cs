namespace AgenticReportGenerationApi
{
    using Microsoft.SemanticKernel.ChatCompletion;
    using Microsoft.SemanticKernel.Connectors.OpenAI;
    
    internal static class Util
    {
        public static async Task<string> GetCompanyName(IChatCompletionService chat, string query)
        {
            // ChatHistory is local to this helper since we are only using it to detect intent
            ChatHistory chatHistory = new ChatHistory();

            var companyName = "not_found";

            chatHistory.AddSystemMessage(
                $@"Return the company name specified from the user. The user will ask to generate one or more of the following report sections for a company:
                    - Board Members
                    - Top Executives
                    - Corporate Timelines
                    - Financial Data
                    - News Data
                    - Summary Data
                It is important to find the name of the company this report is being generated for. The company name will be used to query the database for the company's data.

                You must return the name of the company as a string. If the company name cannot be found, return 'not_found'.

                [Examples user prompts]
                User question: Generate the executive summary for Tesla.
                Company Name: Tesla
                User question: Generate all sections of the report for Apple.
                Company Name: Apple
                User question: Give me the financial data for Microsoft.
                Company Name: Microsoft
                User question: Give me the news data and financial data.
                Company Name: not_found

                Per user query, what is the Company Name?
                Company Name:");

            chatHistory.AddUserMessage(query);

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                Temperature = .5,
                // This is very important as it allows us to instruct the model to give us 3 results for the prompt in one call, this is very powerful
                //ResultsPerPrompt = 3,
            };

            try
            {
                // Call the chat completion asking for 3 rounds to attempt to identify the intent
                var result = await chat.GetChatMessageContentsAsync(
                    chatHistory,
                    executionSettings);

                companyName = string.Join(", ", result.Select(o => o.ToString()));

                // Matches words containing hyphens
                /*var wordFrequencies = Regex.Matches(intentResult.ToLower(), @"\b[\w-]+\b")
                                          .Cast<Match>()
                                          .Select(m => m.Value.ToLower())
                                          .GroupBy(s => s)
                                          .OrderByDescending(g => g.Count());

                intent = wordFrequencies.FirstOrDefault()?.Key;*/
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return companyName;
        }
    }
}