using Microsoft.SemanticKernel.ChatCompletion;

namespace AgenticReportGenerationApi.Services;

public interface IChatHistoryManager
{
    ChatHistory GetOrCreateChatHistory(string sessionId);
    void CleanupOldHistories();
    bool ClearChatHistory(string sessionId);
}

public class ChatHistoryManager : BaseChatHistoryManager
{
    public ChatHistoryManager(string systemMessage) : base(systemMessage) { }
}

public class CompanyNameChatHistoryManager : BaseChatHistoryManager
{
    public CompanyNameChatHistoryManager(string systemMessage) : base(systemMessage) { }
}