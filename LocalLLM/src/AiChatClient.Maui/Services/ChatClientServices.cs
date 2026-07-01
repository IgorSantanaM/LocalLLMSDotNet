using Microsoft.Extensions.AI;

namespace AiChatClient.Maui.Services
{
    public class ChatClientServices(IChatClient client)
    {
        readonly IChatClient _client = client;
        readonly List<ChatMessage> _conversationHistory = new() { new(ChatRole.System, "You always provides references. Every response must contain a link of the reference") };

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(ChatMessage message, ChatOptions? options, CancellationToken cancellationToken = default)
        {

            _conversationHistory.Add(message);
            return _client.GetStreamingResponseAsync(_conversationHistory, options, cancellationToken);
        }

        public void AddUserInput(string text)
        {
            _conversationHistory.Add(new ChatMessage(ChatRole.User, text));
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
