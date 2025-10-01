using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace AIAgentConversation.Services;

public class OpenAIService : IOpenAIService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAIService> _logger;
    private readonly ChatClient _chatClient;

    public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var apiKey = _configuration["OpenAI:ApiKey"];
        var model = _configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";
        
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("OpenAI API key is not configured. Service will fail at runtime.");
        }
        
        _chatClient = new AzureOpenAIClient(new Uri("https://api.openai.com/v1"), new System.ClientModel.ApiKeyCredential(apiKey ?? ""))
            .GetChatClient(model);
    }

    public async Task<string> GenerateResponseAsync(string personality, string topic, string history)
    {
        try
        {
            // Build the exact prompt format as specified
            var prompt = $"You are {personality}. Respond to the conversation on {topic}: {history}";
            
            _logger.LogInformation("Calling OpenAI with personality: {Personality}", personality);
            
            var maxTokens = _configuration.GetValue<int>("OpenAI:MaxTokens", 500);
            var temperature = _configuration.GetValue<float>("OpenAI:Temperature", 0.7f);
            
            var chatOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = maxTokens,
                Temperature = temperature
            };
            
            var completion = await _chatClient.CompleteChatAsync(
                new[] { new UserChatMessage(prompt) },
                chatOptions
            );
            
            var response = completion.Value.Content[0].Text;
            
            _logger.LogInformation("OpenAI response received, length: {Length}", response.Length);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            throw;
        }
    }
}
