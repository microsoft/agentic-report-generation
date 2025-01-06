using System.Collections.Concurrent;
using Microsoft.SemanticKernel.ChatCompletion;  // For ChatHistory

namespace EntertainmentChatApi.Services;

public interface IChatHistoryManager
{
    ChatHistory GetOrCreateChatHistory(string sessionId);
    void CleanupOldHistories();
    bool ClearChatHistory(string sessionId);
}

public class ChatHistoryManager : IChatHistoryManager
{
    private readonly ConcurrentDictionary<string, (ChatHistory History, DateTime LastAccessed)> _chatHistories
        = new ConcurrentDictionary<string, (ChatHistory, DateTime)>();
    private readonly string _systemMessage;
    private readonly TimeSpan _expirationTime = TimeSpan.FromHours(1); // Adjust as needed

    public ChatHistoryManager(string systemMessage)
    {
        _systemMessage = systemMessage;
    }

    public ChatHistory GetOrCreateChatHistory(string sessionId)
    {
        var (history, _) = _chatHistories.AddOrUpdate(
            sessionId,
            _ => (CreateNewChatHistory(), DateTime.UtcNow),
            (_, old) => (old.History, DateTime.UtcNow)
        );
        return history;
    }

    private ChatHistory CreateNewChatHistory()
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(_systemMessage);
        return chatHistory;
    }

    public void CleanupOldHistories()
    {
        var cutoff = DateTime.UtcNow - _expirationTime;
        foreach (var key in _chatHistories.Keys)
        {
            if (_chatHistories.TryGetValue(key, out var value) && value.LastAccessed < cutoff)
            {
                _chatHistories.TryRemove(key, out _);
            }
        }
    }

    // added method to allow the removal of a ChatHistory for a given session,
    // can can be helpful when testing how the chathistory is impacting the responses
    public bool ClearChatHistory(string sessionId)
    {
        return _chatHistories.TryRemove(sessionId, out _);
    }
}