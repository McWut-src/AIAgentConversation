using AIAgentConversation.Models;

namespace AIAgentConversation.Services;

public interface IOpenAIService
{
    Task<string> GenerateResponseAsync(string personality, string topic, string history, string politenessLevel = "medium", ConversationPhase phase = ConversationPhase.Conversation);
}
