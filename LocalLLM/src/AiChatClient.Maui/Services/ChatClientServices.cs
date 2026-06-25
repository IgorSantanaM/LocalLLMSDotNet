using Microsoft.Extensions.AI;

namespace AiChatClient.Maui.Services
{
    public class ChatClientServices(IChatClient client)
    {
        readonly IChatClient _client = client;

        readonly List<ChatMessage> _conversationHistory = [];

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(ChatMessage message, ChatOptions? options, CancellationToken cancellationToken = default)
        {

            _conversationHistory.Add(message);
            return _client.GetStreamingResponseAsync(_conversationHistory, options, cancellationToken);
        }

        public void AddAssistantResponse(string responseText)
        {
            _conversationHistory.Add(new ChatMessage(ChatRole.Assistant, responseText));
        }

        public void ClearConversationHistory()
        {
            _conversationHistory.Clear();
        }
    }
}
