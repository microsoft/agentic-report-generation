using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Concurrent;

namespace AgenticReportGenerationApi.Services
{
    public class BaseChatHistoryManager : IChatHistoryManager
    {
        protected readonly ConcurrentDictionary<string, (ChatHistory History, DateTime LastAccessed)> _chatHistories
        = new ConcurrentDictionary<string, (ChatHistory, DateTime)>();
        protected readonly string _systemMessage;
        protected readonly TimeSpan _expirationTime = TimeSpan.FromHours(1);

        public BaseChatHistoryManager(string? systemMessage)
        {
            if (!String.IsNullOrEmpty(systemMessage)) {
                _systemMessage = systemMessage;
            }
        }

        public virtual ChatHistory GetOrCreateChatHistory(string sessionId)
        {
            var (history, _) = _chatHistories.AddOrUpdate(
                sessionId,
                _ => (CreateNewChatHistory(), DateTime.UtcNow),
                (_, old) => (old.History, DateTime.UtcNow)
            );
            return history;
        }

        protected virtual ChatHistory CreateNewChatHistory()
        {
            var chatHistory = new ChatHistory();

            if (!String.IsNullOrEmpty(_systemMessage))
            {
                chatHistory.AddSystemMessage(_systemMessage);
            }
            return chatHistory;
        }

        public virtual void CleanupOldHistories()
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

        public virtual bool ClearChatHistory(string sessionId)
        {
            return _chatHistories.TryRemove(sessionId, out _);
        }
    }
}