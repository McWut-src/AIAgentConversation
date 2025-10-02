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
            apiKey = "dummy-key"; // Prevent null reference
        }
        
        // Use OpenAI SDK with proper OpenAI client
        var client = new OpenAI.OpenAIClient(apiKey);
        _chatClient = client.GetChatClient(model);
    }

    public async Task<string> GenerateResponseAsync(string personality, string topic, string history, string politenessLevel = "medium")
    {
        try
        {
            _logger.LogInformation("Calling OpenAI with personality: {Personality}", personality);
            
            var maxTokens = _configuration.GetValue<int>("OpenAI:MaxTokens", 500);
            var temperature = _configuration.GetValue<float>("OpenAI:Temperature", 0.7f);
            
            // Determine conversation depth based on history length to adjust creativity
            var messageCount = string.IsNullOrEmpty(history) ? 0 : history.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
            var adjustedTemperature = temperature;
            
            // Increase temperature slightly as conversation progresses for more dynamic responses
            if (messageCount >= 2 && messageCount < 4)
            {
                adjustedTemperature = Math.Min(temperature + 0.1f, 1.0f);
            }
            else if (messageCount >= 4)
            {
                adjustedTemperature = Math.Min(temperature + 0.15f, 1.0f);
            }
            
            var chatOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = maxTokens,
                Temperature = adjustedTemperature
            };
            
            // Build enhanced prompt with system and user messages for better context
            var messages = new List<ChatMessage>();
            
            // Adjust tone based on politeness level
            var toneGuidance = politenessLevel?.ToLower() switch
            {
                "low" => "Be direct and assertive. Challenge points you disagree with. " +
                        "Avoid excessive politeness or overly agreeable language. " +
                        "Focus on your perspective and don't hesitate to express disagreement or skepticism.",
                "high" => "Be respectful and courteous. Acknowledge others' points graciously. " +
                         "Use phrases like 'I appreciate your perspective' or 'That's a valid point'. " +
                         "Maintain a collaborative and warm tone throughout.",
                _ => "Be balanced in tone - neither overly polite nor too confrontational. " +
                     "Engage thoughtfully with respect but without excessive pleasantries."
            };
            
            // System message to set up the agent's role and constraints
            var systemPrompt = $"You are {personality}. You are engaging in a thoughtful conversation about {topic}. " +
                             $"Stay true to your personality traits while being engaging and substantive. " +
                             $"{toneGuidance} " +
                             $"Provide responses that are 2-4 sentences long, balancing depth with conciseness. " +
                             $"Build upon previous points when continuing the conversation.";
            messages.Add(new SystemChatMessage(systemPrompt));
            
            // User message with the conversation context
            string userPrompt;
            if (string.IsNullOrEmpty(history))
            {
                // First message - introduce the topic thoughtfully
                userPrompt = $"Begin a conversation on the topic: {topic}. Share your initial perspective based on your personality.";
            }
            else
            {
                // Continuing conversation - provide history and ask for response
                userPrompt = $"Here is the conversation so far:\n{history}\n\n" +
                           $"Respond to the conversation above, addressing points made while staying in character as {personality}.";
            }
            messages.Add(new UserChatMessage(userPrompt));
            
            var completion = await _chatClient.CompleteChatAsync(messages, chatOptions);
            
            var response = completion.Value.Content[0].Text;
            
            _logger.LogInformation("OpenAI response received, length: {Length}, temperature: {Temp}", response.Length, adjustedTemperature);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            throw;
        }
    }
}
