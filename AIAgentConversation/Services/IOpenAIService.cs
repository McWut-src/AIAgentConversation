namespace AIAgentConversation.Services;

public interface IOpenAIService
{
    Task<string> GenerateResponseAsync(string personality, string topic, string history);
}
